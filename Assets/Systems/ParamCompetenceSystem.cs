using UnityEngine;
using UnityEngine.UI;
using FYFY;
using TMPro;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Collections;

public class ParamCompetenceSystem : FSystem
{

	public static ParamCompetenceSystem instance;

	// Familles
	private Family f_competence = FamilyManager.getFamily(new AllOfComponents(typeof(Competence))); // Les Toogles comp?tences
	private Family f_menuElement = FamilyManager.getFamily(new AnyOfComponents(typeof(Competence), typeof(Category))); // Les Toogles comp?tences et les Cat?gories qui les r?unnissent en groupes
	private Family f_category = FamilyManager.getFamily(new AllOfComponents(typeof(Category))); // Les categories qui contiendront des sous categories ou des comp?tences

	// Variables
	public GameObject panelSelectComp; // Panneau de selection des comp?tences
	public GameObject panelInfoComp; // Panneau d'information des comp?tences
	public GameObject panelInfoUser; // Panneau pour informer le joueur (erreurs de chargement, conflit dans la selection des comp?tences etc...)
	public GameObject scrollViewComp; // Le contr?leur du scroll pour les comp?tences
	public string pathParamComp = "/StreamingAssets/ParamCompFunc/ParamCompetence.csv"; // Chemin d'acces du fichier CSV contenant les info des competences ? charger 
	public GameObject prefabCateComp; // Prefab de l'affichage d'une cat?gorie de comp?tence
	public GameObject prefabComp; // Prefab de l'affichage d'une comp?tence
	public GameObject ContentCompMenu; // Panneau qui contient la liste des cat?gories et comp?tences
	public TMP_Text messageForUser; // Zone de texte pour les messages d'erreur adress?s ? l'utilisateur
	
	private FunctionalityParam funcParam;
	private FunctionalityInLevel funcLevel;
	private List<string> listCompSelectUser = new List<string>(); // Enregistre temporairement les comp?tences s?l?ctionn?es par le user
	private List<string> listCompSelectUserSave = new List<string>(); // Contient les comp?tences selectionn?es par le user

	public ParamCompetenceSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		funcParam = GameObject.Find("FuncData").GetComponent<FunctionalityParam>();
		funcLevel = GameObject.Find("FuncData").GetComponent<FunctionalityInLevel>();
	}

	// used on TitleScreen scene
	public void openPanelSelectComp()
	{
		try
		{
			// Note pour chaque fonction les niveaux ou elles sont pr?sentes
			readXMLinfo();
		}
		catch
		{
			string message = "Erreur chargement fichiers de niveaux!\n";
			message += "V?rifier que les fichiers existent ou sont bien au format XML";
			displayMessageUser(message);
			// Permetra de fermer le panel de selection des competences lorsque le user appuie sur le bouton ok du message d'erreur
			panelSelectComp.GetComponent<ParamCompetenceSystemBridge>().closePanelParamComp = true;
		}

		try
		{
			// On charge les donn?es pour chaque comp?tence
			loadParamComp();
			MainLoop.instance.StartCoroutine(startAfterFamillyOk());
			// On d?marre la coroutine pour attacher chaque comp?tence et sous-categorie et leur cat?gorie
			MainLoop.instance.StartCoroutine(attacheComptWithCat());
		}
		catch
		{
			string message = "Erreur chargement fichier de parametrage des comp?tences!\n";
			message += "V?rifi? que le fichier csv et les informations contenues sont au bon format";
			displayMessageUser(message);
			// Permettra de fermer le panel de selection des comp?tences lorsque le user appuie sur le bouton ok du message d'erreur
			panelSelectComp.GetComponent<ParamCompetenceSystemBridge>().closePanelParamComp = true;
		}
	}

	private IEnumerator noSelect(GameObject comp)
    {
		yield return null;

		listCompSelectUser = new List<string>(listCompSelectUserSave);
		resetSelectComp();
		desactiveToogleComp();
		foreach (string level in listCompSelectUserSave)
        {
			foreach(GameObject c in f_competence)
            {
				if(c.name == level)
                {
					selectComp(c, false);
				}
			}
		}
		MainLoop.instance.StopCoroutine(noSelect(comp));
	}

	// Permet d'attacher ? chaque cat?gorie les sous-categories et comp?tences qui la compose
	private IEnumerator attacheComptWithCat()
    {
		yield return null;

		foreach (GameObject cat in f_category)
		{
			foreach(GameObject element in f_menuElement)
            {
				if (element.GetComponent<MenuComp>().catParent == cat.name)
				{
					cat.GetComponent<Category>().listAttachedElement.Add(element.name);
				}
			}
		}

		MainLoop.instance.StopCoroutine(attacheComptWithCat());
	}

	// Permet de lancer les diff?rentes fonctions que l'on a besoin pour le d?marrage APRES que les familles soient mises ? jour
	private IEnumerator startAfterFamillyOk() {
		yield return null;

		// On d?sactive les comp?tences pas encore impl?ment?es
		desactiveToogleComp();
		// On d?cale les sous-cat?gories et comp?tences selon leur place dans la hierarchie
		displayCatAndComp();

		MainLoop.instance.StopCoroutine(startAfterFamillyOk());
	}

	// Chargement des parametres des comp?tences
	private void loadParamComp()
	{
		StreamReader reader = new StreamReader("" + Application.dataPath + pathParamComp);
		bool endOfFile = false;
		while (!endOfFile)
		{
			string data_string = reader.ReadLine();
			if (data_string == null)
			{
				endOfFile = true;
				break;
			}
			string[] data = data_string.Split(';');

			// Si c'est une comp?tence
			if(data[0] == "Comp")
            {
				createCompObject(data);
			}// Sinon si c'est une cat?gorie
			else if(data[0] == "Cat"){
				createCatObject(data);
			}
		}
	}

	// Instancie et param?tre la comp?tence ? afficher
	private void createCatObject(string[] data)
    {
		// On instancie la cat?gorie
		GameObject category = UnityEngine.Object.Instantiate(prefabCateComp);
		// On l'attache au content
		category.transform.SetParent(ContentCompMenu.transform);
		// On signale ? quelle cat?gorie la comp?tence appartient
		if(data[1] != "None")
        {
			category.GetComponent<MenuComp>().catParent = data[1];
		}
		// On charge les donn?es
		category.name = data[2];
		category.transform.Find("Label").GetComponent<TMP_Text>().text = data[3];
		category.GetComponent<MenuComp>().info = data[4];

		GameObjectManager.bind(category);
	}

	// Instancie et param?tre la sous-comp?tence ? afficher
	private void createCompObject(string[] data)
	{
		// On instancie la cat?gorie
		GameObject competence = UnityEngine.Object.Instantiate(prefabComp);
		// On signale ? quel cat?gorie la comp?tence appartient
		competence.GetComponent<Competence>().catParent = data[1];
		// On l'attache au content
		competence.transform.SetParent(ContentCompMenu.transform);
		competence.name = data[2];
		// On charge le text de la comp?tence
		competence.transform.Find("Label").GetComponent<TMP_Text>().text = data[3];
		competence.transform.Find("Label").GetComponent<TMP_Text>().alignment = TMPro.TextAlignmentOptions.MidlineLeft;
		// On charge les info de la comp?tence qui sera affich?e lorsque l'on survolera celle-ci avec la souris
		competence.GetComponent<Competence>().info = data[4];
		// On charge le vecteur des Fonctions li?es ? la comp?tence
		if (data.Length >= 6)
        {
			var data_link = data[5].Split(',');
			foreach (string value in data_link)
			{
				competence.GetComponent<Competence>().compLinkWhitFunc.Add(value);
			}
			if (data.Length >= 7)
			{
				// On charge le vecteur des comp?tences qui seront automatiquement selectionn?es si la comp?tence est s?l?ctionn?e
				data_link = data[6].Split(',');
				foreach (string value in data_link)
				{
					competence.GetComponent<Competence>().compLinkWhitComp.Add(value);
				}
				if (data.Length >= 8)
				{
					// On charge le vecteur des comp?tences dont au moins l'une devra ?tre selectionn?e en m?me temps que celle selectionn?e actuellement
					data_link = data[7].Split(',');
					foreach (string value in data_link)
					{
						competence.GetComponent<Competence>().listSelectMinOneComp.Add(value);
					}
				}
			}
		}

		GameObjectManager.bind(competence);
	}

	// Mise en place: d?caller les sous-categories et comp?tences
	private void displayCatAndComp()
    {
		foreach(GameObject element in f_menuElement) { 
			// Si l'?l?ment a un parent
			if(element.GetComponent<MenuComp>().catParent != "")
            {
				int nbParent = nbParentInHierarchiComp(element);

                if (element.GetComponent<Competence>())
                {
					element.transform.Find("Background").position = new Vector3(element.transform.Find("Background").position.x + (nbParent * 15), element.transform.Find("Background").position.y, element.transform.Find("Background").position.z);
					element.transform.Find("Label").position = new Vector3(element.transform.Find("Label").position.x + (nbParent * 15), element.transform.Find("Label").position.y, element.transform.Find("Label").position.z);
				}
				else if (element.GetComponent<Category>())
                {
					element.transform.Find("Label").position = new Vector3(element.transform.Find("Label").position.x + (nbParent * 15), element.transform.Find("Label").position.y, element.transform.Find("Label").position.z);
					element.transform.Find("ButtonHide").position = new Vector3(element.transform.Find("ButtonHide").position.x + (nbParent * 15), element.transform.Find("ButtonHide").position.y, element.transform.Find("ButtonHide").position.z);
					element.transform.Find("ButtonShow").position = new Vector3(element.transform.Find("ButtonShow").position.x + (nbParent * 15), element.transform.Find("ButtonShow").position.y, element.transform.Find("ButtonShow").position.z);
				}
			}
		}
    }

	// Fonction pouvant ?tre appell?e par r?cursivit?
	// Permet de renvoyer ? quelle profondeur dans la hi?rarchie Categorie de la selection des comp?tences l'?l?ment se trouve
	private int nbParentInHierarchiComp(GameObject element)
    {
		int nbParent = 1;

		foreach (GameObject ele in f_menuElement){ 
			if(ele.name == element.GetComponent<MenuComp>().catParent && ele.GetComponent<MenuComp>().catParent != "")
            {
				nbParent += nbParentInHierarchiComp(ele);
			}
		}
			return nbParent;
    }

	// Lit tous les fichiers XML des niveaux de chaque dossier afin de charger quelle fonctionalit? se trouve dans quel niveau  
	private void readXMLinfo()
	{
		// foreach (List<string> levels in Global.GD.treeLevelList.Values)
		// {
		// 	foreach (string level in levels)
		// 	{
		// 		XmlDocument doc = new XmlDocument();
		// 		if (Application.platform == RuntimePlatform.WebGLPlayer)
		// 		{
		// 			doc.LoadXml(level);
		// 			loadInfo(doc, level);
		// 		}
		// 		else
		// 		{
		// 			doc.Load(level);
		// 			loadInfo(doc, level);
		// 		}
		// 	}
		// }
	}

	// Parcourt le noeud d'information et appelle les bonnes fonctions pour traiter l'information du niveau
	private void loadInfo(XmlDocument doc, string namelevel)
	{
		XmlNode root = doc.ChildNodes[1];
		foreach (XmlNode child in root.ChildNodes)
		{
			switch (child.Name)
			{
				case "info":
					foreach (XmlNode infoNode in child.ChildNodes)
					{
						switch (infoNode.Name)
						{
							case "func":
								addInfo(infoNode, namelevel);
								break;
						}
					}
					break;
			}
		}
	}

	// Associe ? chaque fonctionalit? renseign?e sa pr?sence dans le niveau
	private void addInfo(XmlNode node, string namelevel)
	{
		if(funcParam.levelDesign[node.Attributes.GetNamedItem("name").Value])
        {
			// Si la fonctionnalit? n'est pas encore connue dans le dictionnaire, on l'ajoute
			if (!funcLevel.levelByFuncLevelDesign.ContainsKey(node.Attributes.GetNamedItem("name").Value))
			{
				funcLevel.levelByFuncLevelDesign.Add(node.Attributes.GetNamedItem("name").Value, new List<string>());
			}
			// On r?cup?re la liste d?j? pr?sente
			List<string> listLevelForFuncLevelDesign = funcLevel.levelByFuncLevelDesign[node.Attributes.GetNamedItem("name").Value];
			listLevelForFuncLevelDesign.Add(namelevel);
		}
        else
        {
			// Si la fonctionnalit? n'est pas encore connue dans le dictionnaire, on l'ajoute
			if (!funcLevel.levelByFunc.ContainsKey(node.Attributes.GetNamedItem("name").Value))
			{
				funcLevel.levelByFunc.Add(node.Attributes.GetNamedItem("name").Value, new List<string>());
			}
			// On r?cup?re la liste d?j? pr?sente
			List<string> listLevelForFunc = funcLevel.levelByFunc[node.Attributes.GetNamedItem("name").Value];
			listLevelForFunc.Add(namelevel);
		}
	}

	// D?sactive les toggles pas encore impl?ment?s
	private void desactiveToogleComp()
	{

		foreach(string nameFunc in funcParam.active.Keys)
        {
			if (!funcParam.active[nameFunc])
			{
				foreach (GameObject comp in f_competence)
				{
					if (comp.GetComponent<Competence>().compLinkWhitFunc.Contains(nameFunc) && comp.GetComponent<Toggle>().interactable)
					{
						comp.GetComponent<Toggle>().interactable = false;
						comp.GetComponent<Competence>().active = false;
					}
				}
			}
        }
	}

	// Permet de selectionn? aussi les functionnalit?s linker avec la fonctionalit? selectionn?e
	private void addSelectFuncLinkbyFunc(string nameFunc)
    {
		foreach(string f_name in funcParam.activeFunc[nameFunc])
        {
            // Si la fonction na pas encore ?t? selectionn?e
			// alors on l'ajoute ? la s?l?ction et on fait un appel r?cursif dessus
            if (f_name != "" && !funcParam.funcActiveInLevel.Contains(f_name))
            {
				funcParam.funcActiveInLevel.Add(f_name);
				addSelectFuncLinkbyFunc(f_name);
			}
        }
    }

	// Pour certaines comp?tences il est indispensable que d'autres soient aussi selectionn?es
	// Cette fonction v?rifie que c'est bien le cas avant de lancer la selection de niveau auto
	// Sinon il signale au User quelle comp?tence pose probl?me ainsi qu'une comp?tence minimum qu'il doit cocher parmis la liste propos?e
	public void verificationSelectedComp()
    {
		saveListUser();
		bool verif = true;
		List<GameObject> listCompSelect = new List<GameObject>();
		List<string> listNameCompSelect = new List<string>();
		GameObject errorSelectComp = null;

		//On verifie
		foreach (GameObject comp in f_competence)
        {
            // Si la comp?tence est s?l?ctionn?e on le note
            if (comp.GetComponent<Toggle>().isOn)
            {
				// Si la comp?tence demande ? avoir une autre comp
				listCompSelect.Add(comp);
				listNameCompSelect.Add(comp.name);
			}
        }

		foreach(GameObject comp in listCompSelect)
        {
			if(comp.GetComponent<Competence>().listSelectMinOneComp[0] != "")
            {
				verif = false;
				foreach (string nameComp in comp.GetComponent<Competence>().listSelectMinOneComp)
                {
                    if (listNameCompSelect.Contains(nameComp))
                    {
						verif = true;
					}
                }
                if (!verif)
                {
					errorSelectComp = comp;
				}
            }
        }

		// Si tout va bien on lance la s?lection du niveau
        if (verif)
        {
			startLevel();
        }
        else // Sinon on signale au joueur l'erreur
        {
			// Message au User en lui signalant quelle competence il doit choisir 
			string message = "Pour la comp?tence " + errorSelectComp + " Il faut aussi selectionner une de ces comp?tences :\n";
			foreach(string comp in errorSelectComp.GetComponent<Competence>().listSelectMinOneComp)
            {
				message += comp + " ";
            }
			displayMessageUser(message);
		}
    }

	// Use in ButtonStartLevel in ParamCompPanel prefab
	public void startLevel()
    {
		// On parcourt tous les levels disponibles pour les copier dans une liste temporaire
		List<string> copyLevel = new List<string>();
		int nbCompActive = 0;
		bool conditionStartLevelOk = true;

		bool levelLD = false;
		// On regarde si des competences concernant le level design on ?t? selectionn?es
		foreach (GameObject comp in f_competence)
		{
            if (comp.GetComponent<Toggle>().isOn)
            {
				nbCompActive += 1;
				// On fait ?a avec le level design
				foreach (string f_key in funcParam.levelDesign.Keys)
				{
                    if (!funcParam.funcActiveInLevel.Contains(f_key) && comp.GetComponent<Competence>().compLinkWhitFunc.Contains(f_key))
                    {
						funcParam.funcActiveInLevel.Add(f_key);
						addSelectFuncLinkbyFunc(f_key);
					}
					if (comp.GetComponent<Competence>().compLinkWhitFunc.Contains(f_key) && funcParam.levelDesign[f_key])
                    {
						levelLD = true;
                    }
				}
			}
		}

        // Si aucune comp?tence n'a ?t? selectionn?e on ne chargera pas de niveau
        if (nbCompActive <= 0)
        {
			conditionStartLevelOk = false;
		}

        if (conditionStartLevelOk)
        {
			// 2 cas de figures : 
			// Demande de niveau sp?cial pour la comp?tence
			// Demande de niveau sans comp?tence LD
			if (levelLD)
			{
				// On parcourt le dictionnaires des fonctionnalit?s de level design
				// Si elle fait partie des fonctionnalit?s selectionn?es, alors on enregistre les levels associ?s ? la fonctionnalit?
				foreach (string f_key in funcLevel.levelByFuncLevelDesign.Keys)
				{
                    if (funcParam.funcActiveInLevel.Contains(f_key))
                    {
						foreach(string level in funcLevel.levelByFuncLevelDesign[f_key])
                        {
							copyLevel.Add(level);
						}
					}
				}
				// On garde ensuite les niveaux qui contienent exclusivement toutes les fonctionalit?s selectionn?es
				foreach (string f_key in funcLevel.levelByFuncLevelDesign.Keys)
				{
					if (funcParam.funcActiveInLevel.Contains(f_key))
					{
						for(int i = 0; i < copyLevel.Count;)
                        {
                            if (!funcLevel.levelByFuncLevelDesign[f_key].Contains(copyLevel[i]))
                            {
								copyLevel.Remove(copyLevel[i]);
                            }
                            else
                            {
								i++;
                            }
                        }
					}
				}
			}
			else if (!levelLD)
			{
				// On parcourt le dictionnaire des fonctionnalit?s level design
				// On supprime de la liste des niveaux possibles tous les niveaux appellant des fonctionnalit?s de level design
				// foreach (List<string> levels in Global.GD.treeLevelList.Values)
				// {
				// 	// On cr?er une copie de la liste des niveaux disponibles
				// 	foreach (string level in levels)
				// 		copyLevel.Add(level);
				// }

				foreach (List<string> levels in funcLevel.levelByFuncLevelDesign.Values)
				{
					foreach(string level in levels)
                    {
						copyLevel.Remove(level);
                    }
				}
			}
		}
        else
        {
			string message = "Erreur, pas de comp?tence s?lectionn?e!";
			displayMessageUser(message);
		}

		// Si on a au moins une comp?tence activ?e et un niveau en commun
		// On lance un niveau selectionn? al?atoirement parmis la liste des niveaux restants
		if (copyLevel.Count != 0)
        {
			if (copyLevel.Count > 1)
            {
				// On selectionne le niveau al?atoirement
				var rand = new System.Random();
				int r = rand.Next(0, copyLevel.Count);
				string levelSelected = copyLevel[r];
				// On split la chaine de caract?re pour pouvoir r?cup?rer le dossier ou se trouve le niveau selectionn?
				var level = levelSelected.Split('\\');
				string folder = level[level.Length - 2];
				// Global.GD.level.name = (folder, ((List<string>)Global.GD.treeLevelList[folder]).IndexOf(levelSelected));
			}
            else
            {
				string levelSelected = copyLevel[0];
				// On split la chaine de caract?re pour pouvoir r?cup?rer le dossier ou se trouve le niveau selectionn?
				var level = levelSelected.Split('\\');
				string folder = level[level.Length - 2];
				// Global.GD.level.name = (folder, ((List<string>)Global.GD.treeLevelList[folder]).IndexOf(levelSelected));
			}
			GameObjectManager.loadScene("GameScene");
		}
		else // Sinon on signale qu'aucune comp?tence n'est selectionn?e ou qu'aucun niveau n'est disponible
        {
			string message = "Pas de niveau disponible pour l'ensemble des comp?tences selectionn?es";
			displayMessageUser(message);
		}
	}

	// Used when PointerOver CategorizeCompetence prefab (see in editor)
	public void infoCompetence(GameObject comp)
	{
		panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = comp.GetComponent<MenuComp>().info;
		comp.transform.Find("Label").GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Bold;

		// Si la comp?tence enclanche la s?lection d'autre comp?tence, on l'affiche dans les infos
		if(comp.GetComponent<Competence>() && comp.GetComponent<Competence>().compLinkWhitComp[0] != "")
        {
			string infoMsg = panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text;
			infoMsg += "\n\nCompetence selectionn?e automatiquement : \n";
			foreach(string nameComp in comp.GetComponent<Competence>().compLinkWhitComp)
            {
				infoMsg += nameComp + " ";
			}
			panelInfoComp.transform.Find("InfoText").GetComponent<TMP_Text>().text = infoMsg;
		}

		// Si on survole une categorie, on change la couleur du bouton
        if (comp.GetComponent<Category>())
        {
			foreach(Transform child in comp.transform){
                if (child.GetComponent<Button>())
                {
					Color col = new Color(1f, 1f, 1f);
					if (child.name == "ButtonHide")
                    {
						col = new Color(0.8313726f, 0.2862745f, 0.2235294f);
					}
					else if (child.name == "ButtonShow")
                    {
						col = new Color(0.2392157f, 0.8313726f, 0.2235294f);
					}
					child.GetComponent<Image>().color = col;
				}
            }
        }
	}

	// Lorsque la souris sort de la zone de text de la comp?tence ou cat?gorie, on remet le text ? son ?tat initial
	public void resetViewInfoCompetence(GameObject comp)
    {
		comp.transform.Find("Label").GetComponent<TMP_Text>().fontStyle = TMPro.FontStyles.Normal;

		if (comp.GetComponent<Category>()){
			foreach (Transform child in comp.transform)
			{
				if (child.GetComponent<Button>())
				{
					child.GetComponent<Image>().color = new Color(1f, 1f, 1f);
				}
			}
		}
	}

	// On parcourt toutes les comp?tences
	// On desactive toutes les comp?tences non impl?ment?es et les comp?tences ne pouvant plus ?tre selectionn?es
	// On selectionne automatiquement les competences linker
	public void selectComp(GameObject comp, bool userSelect)
    {
        if (userSelect)
        {
			addOrRemoveCompSelect(comp, true);
		}
        else
        {
			comp.GetComponent<Toggle>().isOn = true;
		}

		bool error = false;

		// On parcourt la liste des fonctions ? activer pour la comp?tence
		foreach (string funcNameActive in comp.GetComponent<Competence>().compLinkWhitFunc)
		{
			//Pour chaque fonction on regarde si cela emp?che une comp?tence d'?tre selectionn?e
			foreach (string funcNameDesactive in funcParam.enableFunc[funcNameActive])
			{
				// Pour chaque fonction non possible, on regarde les comp?tences les utilisant pour en d?sactiver la selection
				foreach (GameObject c in f_competence)
				{
					if (c.GetComponent<Competence>().compLinkWhitFunc.Contains(funcNameDesactive))
					{
						if (!c.GetComponent<Toggle>().isOn)
						{
							c.GetComponent<Toggle>().interactable = false;
						}
						else
						{
							error = true;
							break;
						}
					}
				}
			}

			foreach(string nameComp in comp.GetComponent<Competence>().compLinkWhitComp)
            {
				foreach(GameObject c in f_competence)
                {
					if(c.name == nameComp)
                    {
						// Les comp?tences non active sont les comp?tences dont au moins une des fonctionalit?s n'est pas encore impl?ment?e
						// Pour ?viter tout bug (comme ?tre consid?r? comme inactive ? cause d'une autre comp?tence s?l?ctionn?e) on teste si la comp?tence est d?sactiv?e par le biais d'un manque de fonction ou non
						if (c.GetComponent<Competence>().active)
						{
							if (c.GetComponent<Toggle>().interactable)
							{
								// Pour ?viter les boucles infinies, si la comp?tence est d?j? activ?e, alors la r?cursive a d?j? eu lieu
								if (!c.GetComponent<Toggle>().isOn)
								{
									selectComp(c, false);
								}
							}
							else
							{
								Debug.Log("error");
								error = true;
								break;
							}
						}
					}
                }
            }
		}

        if (error)
        {
			string message = "Conflit concernant l'interactibilit? de la comp?tence s?lectionn?";
			displayMessageUser(message);
			// Deselectionner la comp?tence
			stratCoroutineNoSelect(comp);
		}
	}

	private void stratCoroutineNoSelect(GameObject comp)
    {
		MainLoop.instance.StartCoroutine(noSelect(comp));
	}

	//Lors de la des?lection d'une comp?tence on des?lectionne toutes les comp?tences reli?es
	public void unselectComp(GameObject comp, bool userUnselect)
    {
		// On retire la comp?tence de la liste des comp?tences s?lectionn?es
		addOrRemoveCompSelect(comp, false);

		// On reset l'affichage de toutes les comp?tences.
		resetSelectComp();

		// Le toggle va ?tre d?sactiv? automatiquement par le programme apr?s le traitement de la fonction 
		if (userUnselect)
		{
			comp.GetComponent<Toggle>().isOn = true;
		}

		//On d?sactive tous les toggle des comp pas impl?ment?s
		desactiveToogleComp();

		// On res?lectionne toutes les comp?tences
		foreach (string compName in listCompSelectUser)
		{
			foreach (GameObject c in f_competence)
			{
				if (c.name == compName)
				{
					selectComp(c, false);
				}
			}
		}

	}

	// Ajoute ou retire la comp?tence de la liste des comp?tences selectionn?es manuellement par l'utilisateur
	public void addOrRemoveCompSelect(GameObject comp, bool value)
	{
        if (value)
        {
			// Si la comp?tence n'est pas encore not?e comme avoir ?t? selectionn?e par le user
            if (!listCompSelectUser.Contains(comp.name))
            {
				listCompSelectUser.Add(comp.name);
			}
		}
        else
        {
			// Si la comp?tence avait ?t? s?l?ctionn?e par le user
			if(listCompSelectUser.Contains(comp.name)){
				listCompSelectUser.Remove(comp.name);
			}
		}
	}

	// Reset toutes les comp?tences en "non selectionn?e"
	private void resetSelectComp()
    {
		foreach (GameObject comp in f_competence)
		{
			comp.GetComponent<Toggle>().isOn = false;
			comp.GetComponent<Toggle>().interactable = true;
		}
	}

	// Enregistre la liste des comp?tences s?lectionn?es par le user
	public void saveListUser()
    {
		listCompSelectUserSave = new List<string>(listCompSelectUser);
	}

	// Ferme le panel de s?lection des comp?tences
	// D?coche toutes les comp?tences coch?es
	// vide les listes de suivis des comp?tences selectionn?es
	public void closeSelectCompetencePanel()
    {
		panelSelectComp.SetActive(false);
		resetSelectComp();
		listCompSelectUser = new List<string>();
		listCompSelectUserSave = new List<string>();
	}

	// Affiche le panel message avec le bon message
	public void displayMessageUser(string message)
    {
		messageForUser.text = message;
		panelInfoUser.SetActive(true);
	}

	// Cache ou montre les ?l?ments associ?s ? la cat?gorie
	public void viewOrHideCompList(GameObject category)
    {
		category.GetComponent<Category>().hideList = !category.GetComponent<Category>().hideList;

		foreach (GameObject element in f_menuElement)
        {
            if (category.GetComponent<Category>().listAttachedElement.Contains(element.name))
            {
				element.SetActive(!category.GetComponent<Category>().hideList);
            }
        }
	}

	// Active ou d?sactive la bouton
	// Cette fonction est r?serv?e ? la gestion du bouton ? afficher ? cot? de la cat?gorie si jamais le user appuie sur le text pour faire apparaitre ou disparaitre la liste associ?e
	public void hideOrShowButtonCategory(GameObject button)
    {
		button.SetActive(!button.activeSelf);
	}
}
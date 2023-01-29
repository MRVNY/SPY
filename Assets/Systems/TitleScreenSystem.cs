using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Xml;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Object = UnityEngine.Object;
using System.Diagnostics;

/// <summary>
/// Manage main menu to launch a specific mission
/// </summary>
public class TitleScreenSystem : FSystem {
	public GameObject prefabFuncData;
	public GameObject mainMenu;
	public GameObject campagneMenu;
	public GameObject compLevelButton;
	public GameObject cList;
	public string pathFileParamFunct = "/StreamingAssets/ParamCompFunc/FunctionConstraint.csv"; // Chemin d'acces pour la chargement des paramètres des functions
	public string pathFileParamRequiermentLibrary = "/StreamingAssets/ParamCompFunc/FunctionalityRequiermentLibrairy.xml"; // Chemin d'acces pour la chargement des paramètres des functions

	private FunctionalityParam funcParam;
	private FunctionalityInLevel funcLevel;

	public Task buildingTree;

	private Dictionary<GameObject, List<GameObject>> levelButtons; //key = directory button,  value = list of level buttons

	private string[] languages = new string[] {"en", "fr"};

	public GameObject settingsPanel;
	public GameObject menuPanel;

	protected override void onStart()
	{
		if (funcParam == null)
        {
            GameObject funcData = UnityEngine.Object.Instantiate(prefabFuncData);
            funcData.name = "FuncData";
            GameObjectManager.dontDestroyOnLoadAndRebind(funcData);
            funcParam = funcData.GetComponent<FunctionalityParam>();
            funcLevel = funcData.GetComponent<FunctionalityInLevel>();
        }

		if(Global.GD == null) GameStateManager.LoadGD();
		if (Global.GD == null || Global.GD.levelNameList == null)
		{
			Global.GD = new GameData();
			Global.GD.score = new Hashtable();
			Global.GD.path = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" + Path.DirectorySeparatorChar;
			buildingTree = TreeManager.ConstructTree();
			Global.GD.levelNameList = new Hashtable();
		}


		Global.GD.levelNameList = new Hashtable();
        levelButtons = new Dictionary<GameObject, List<GameObject>>();

		GameObjectManager.setGameObjectState(campagneMenu, false);
		string levelsPath;
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			//paramFunction();
			Global.GD.levelNameList["SkillTree"] = new List<string>();
			for (int i = 1; i <= 20; i++)
				((List<string>)Global.GD.levelNameList["SkillTree"]).Add(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
				                                                         Path.DirectorySeparatorChar + "SkillTree" + Path.DirectorySeparatorChar +"Niveau" + i + ".xml");
			// Hide Competence button
			GameObjectManager.setGameObjectState(compLevelButton, false);
			ParamCompetenceSystem.instance.Pause = true;
		}
		else
		{
			paramFunction();
			levelsPath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels";
			List<string> levels;
			foreach (string directory in Directory.GetDirectories(levelsPath))
			{
				levels = readScenario(directory);
				if (levels != null)
					Global.GD.levelNameList[Path.GetFileNameWithoutExtension(directory)] = levels; //key = directory name
			}
		}

		//create level directory buttons
		foreach (string key in new List<string>{"SkillTree", "Homemade"})
		{
			GameObject directoryButton = Object.Instantiate<GameObject>(Resources.Load("Prefabs/Button") as GameObject, cList.transform);
			directoryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = key;
			levelButtons[directoryButton] = new List<GameObject>();
			GameObjectManager.bind(directoryButton);
			// add on click
			directoryButton.GetComponent<Button>().onClick.AddListener(delegate { showLevels(directoryButton); });
			// create level buttons
			for (int i = 0; i < ((List<string>)Global.GD.levelNameList[key]).Count; i++)
			{
				GameObject button = Object.Instantiate<GameObject>(Resources.Load("Prefabs/LevelButton") as GameObject, cList.transform);
				button.transform.Find("Button").GetChild(0).GetComponent<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(((List<string>)Global.GD.levelNameList[key])[i]);
				//Level lvl = ((List<Level>)Global.GD.levelObjectList[key])[i]; // need to use local variable instead all buttons launch the last
				Level lvl = new Level();
				lvl.name = Path.GetFileNameWithoutExtension(((List<string>)Global.GD.levelNameList[key])[i]);
				button.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { launchLevel(key, lvl); });
				levelButtons[directoryButton].Add(button);
				GameObjectManager.bind(button);
				GameObjectManager.setGameObjectState(button, false);
			}
		}

		openMenu();
	}

	public static List<string> readScenario(string repositoryPath) {
		if (File.Exists(repositoryPath + Path.DirectorySeparatorChar + "Scenario.xml")) {
			List<string> levelNameList = new List<string>();
			XmlDocument doc = new XmlDocument();
			doc.Load(repositoryPath + Path.DirectorySeparatorChar + "Scenario.xml");
			XmlNode root = doc.ChildNodes[1]; //root = <scenario/>
			foreach (XmlNode child in root.ChildNodes) {
				if (child.Name.Equals("level")) {
					levelNameList.Add(repositoryPath + Path.DirectorySeparatorChar + (child.Attributes.GetNamedItem("name").Value));
				}
			}
			return levelNameList;
		}
		return null;
	}

	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetButtonDown("Cancel")) {
			Application.Quit();
		}
	}

	// See Jouer button in editor
	public void showCampagneMenu() {
		GameObjectManager.setGameObjectState(campagneMenu, true);
		GameObjectManager.setGameObjectState(mainMenu, false);
		foreach (GameObject directory in levelButtons.Keys) {
			//show directory buttons
			GameObjectManager.setGameObjectState(directory, true);
			//hide level buttons
			foreach (GameObject level in levelButtons[directory]) {
				GameObjectManager.setGameObjectState(level, false);
			}
		}
	}

	private void showLevels(GameObject levelDirectory) {
		//show/hide levels
		foreach (GameObject directory in levelButtons.Keys) {
			//hide level directories
			GameObjectManager.setGameObjectState(directory, false);
			//show levels
			if (directory.Equals(levelDirectory)) {
				for (int i = 0; i < levelButtons[directory].Count; i++) {
					GameObjectManager.setGameObjectState(levelButtons[directory][i], true);

					string directoryName = levelDirectory.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
					//locked levels
					if (i <= PlayerPrefs.GetInt(directoryName, 0)) //by default first level of directory is the only unlocked level of directory
						levelButtons[directory][i].transform.Find("Button").GetComponent<Button>().interactable = true;
					//unlocked levels
					else
						//levelButtons[directory][i].transform.Find("Button").GetComponent<Button>().interactable = false;
						levelButtons[directory][i].transform.Find("Button").GetComponent<Button>().interactable = true;
					//scores
					// int scoredStars = PlayerPrefs.GetInt(directoryName + Path.DirectorySeparatorChar + i + Global.GD.scoreKey, 0); //0 star by default
					// Transform scoreCanvas = levelButtons[directory][i].transform.Find("ScoreCanvas");
					// for (int nbStar = 0; nbStar < 4; nbStar++) {
					// 	if (nbStar == scoredStars)
					// 		GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, true);
					// 	else
					// 		GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, false);
					// }
				}
			}
			//hide other levels
			else {
				foreach (GameObject go in levelButtons[directory]) {
					GameObjectManager.setGameObjectState(go, false);
				}
			}
		}
	}

	public void launchLevel(string mode, Level level) {
		Global.GD.mode = mode;
		Global.GD.level = level;
		SendStatements.Globals.start = DateTime.Now;
		SendStatements.instance.SendLevel(int.Parse(level.name.Replace("Niveau", "")));
		//watch.Start();
		//SendStatements.instance.SendLevel(int.Parse(level.name.Replace("Niveau", "")));
		GameStateManager.SaveGD();
		GameObjectManager.loadScene("GameScene");
	}

	public async void launchLevelMap()
	{
		if(buildingTree!=null) await buildingTree;
		//GameStateManager.SaveGD();
		GameObjectManager.loadScene("LevelMap");
	}

	// See Retour button in editor
	public void backFromCampagneMenu() {
		foreach (GameObject directory in levelButtons.Keys) {
			if (directory.activeSelf) {
				//main menu
				GameObjectManager.setGameObjectState(mainMenu, true);
				GameObjectManager.setGameObjectState(campagneMenu, false);
				break;
			}
			else {
				//show directory buttons
				GameObjectManager.setGameObjectState(directory, true);
				//hide level buttons
				foreach (GameObject go in levelButtons[directory]) {
					GameObjectManager.setGameObjectState(go, false);
				}
			}
		}
	}

	// Initialise tout ce qui concerne les fonctionalités
	private void paramFunction()
	{
		//loadConstraintFunction();
		//loadRequiermentLibrary();
	}

	// Charge les différentes contraintes qui existent entre les fonctionalités
	private void loadConstraintFunction()
	{
		StreamReader reader = new StreamReader("" + Application.dataPath + pathFileParamFunct);
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
			funcParam.active.Add(data[0], Convert.ToBoolean(data[4]));
			funcParam.levelDesign.Add(data[0], Convert.ToBoolean(data[3]));
			List<string> tmp = new List<string>();
			var data_link = data[1].Split(',');
			foreach (string value in data_link)
			{
				tmp.Add(value);
			}
			funcParam.activeFunc.Add(data[0], new List<string>(tmp));
			tmp = new List<string>();
			data_link = data[2].Split(',');
			foreach (string value in data_link)
			{
				tmp.Add(value);
			}
			funcParam.enableFunc.Add(data[0], new List<string>(tmp));
		}
	}

	private void loadRequiermentLibrary(){
		XmlDocument doc = new XmlDocument();
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			doc.LoadXml("" + Application.dataPath + pathFileParamRequiermentLibrary);
			XMLRequiermentLibrary(doc);
		}
		else
		{
			doc.Load("" + Application.dataPath + pathFileParamRequiermentLibrary);
			XMLRequiermentLibrary(doc);
		}
	}

	private void XMLRequiermentLibrary(XmlDocument doc)
    {
		XmlNode root = doc.ChildNodes[1];
		foreach (XmlNode child in root.ChildNodes)
		{
            if (child.Name == "CaptorList")
            {
				foreach (XmlNode childEle in child)
                {
					funcParam.listCaptor.Add(childEle.Attributes.GetNamedItem("name").Value);
				}
			}
			else if(child.Name == "func")
            {
				List<string> listEleTemp = new List<string>();
				foreach (XmlNode childEle in child)
				{
					listEleTemp.Add(childEle.Attributes.GetNamedItem("name").Value);
				}
				funcParam.elementRequiermentLibrary.Add(child.Attributes.GetNamedItem("name").Value, listEleTemp);
			}
		}
	}

	public void changeLanguage(int i)
	{
		Global.GD.gameLanguage = languages[i];
	}

	public void clearSaves()
	{
		GameStateManager.DeleteAllSaveFiles();
		Global.GD = null;
	}

	public void openSettings()
	{
		GameObjectManager.setGameObjectState(settingsPanel, true);
		GameObjectManager.setGameObjectState(menuPanel, false);
	}

	public void openMenu()
	{
		GameObjectManager.setGameObjectState(settingsPanel, false);
		GameObjectManager.setGameObjectState(menuPanel, true);
	}

	// See Quitter button in editor
	public void quitGame(){
		Application.Quit();
	}
}

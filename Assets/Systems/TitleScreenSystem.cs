﻿using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Xml;
using System;
using Object = UnityEngine.Object;
using UnityEngine.Networking;

/// <summary>
/// Manage main menu to launch a specific mission
/// </summary>
public class TitleScreenSystem : FSystem {
	private GameData gameData;
	public GameData prefabGameData;
	public GameObject campagneMenu;
	public GameObject compLevelButton;
	public GameObject playButton;
	public GameObject quitButton;
	public GameObject backButton;
	public GameObject cList;
	public string pathFileParamFunct = "/StreamingAssets/ParamCompFunc/FunctionConstraint.csv"; // Chemin d'acces pour la chargement des paramétres des functions
	public string pathFileParamRequiermentLibrary = "/StreamingAssets/ParamCompFunc/FunctionalityRequiermentLibrairy.xml"; // Chemin d'acces pour la chargement des paramétres des functions

	private Dictionary<GameObject, List<GameObject>> levelButtons; //key = directory button,  value = list of level buttons

	protected override void onStart()
	{
		if (!GameObject.Find("GameData"))
		{
			gameData = UnityEngine.Object.Instantiate(prefabGameData);
			gameData.name = "GameData";
			paramFunction();
			GameObjectManager.dontDestroyOnLoadAndRebind(gameData.gameObject);
		}
		else
		{
			gameData = GameObject.Find("GameData").GetComponent<GameData>();
		}

		gameData.levelList = new Dictionary<string, List<string>>();

		levelButtons = new Dictionary<GameObject, List<GameObject>>();

		GameObjectManager.setGameObjectState(campagneMenu, false);
		GameObjectManager.setGameObjectState(backButton, false);
		string levelsPath;
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			gameData.levelList["Campagne"] = new List<string>();
			for (int i = 1; i <= 20; i++)
				gameData.levelList["Campagne"].Add(Application.streamingAssetsPath + "/Levels/Campagne/Niveau" + i + ".xml");
		}
		else
		{
			levelsPath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels";
			List<string> levels;
			foreach (string directory in Directory.GetDirectories(levelsPath))
			{
				levels = readScenario(directory);
				if (levels != null)
					gameData.levelList[Path.GetFileName(directory)] = levels; //key = directory name
			}
		}

		//create level directory buttons
		foreach (string key in gameData.levelList.Keys)
		{
			GameObject directoryButton = Object.Instantiate<GameObject>(Resources.Load("Prefabs/Button") as GameObject, cList.transform);
			directoryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = key;
			levelButtons[directoryButton] = new List<GameObject>();
			GameObjectManager.bind(directoryButton);
			// add on click
			directoryButton.GetComponent<Button>().onClick.AddListener(delegate { showLevels(directoryButton); });
			// create level buttons
			for (int i = 0; i < gameData.levelList[key].Count; i++)
			{
				GameObject button = Object.Instantiate<GameObject>(Resources.Load("Prefabs/LevelButton") as GameObject, cList.transform);
				button.transform.Find("Button").GetChild(0).GetComponent<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(gameData.levelList[key][i]);
				int indice = i;
				button.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { launchLevel(key, indice); });
				levelButtons[directoryButton].Add(button);
				GameObjectManager.bind(button);
				GameObjectManager.setGameObjectState(button, false);
			}
		}
	}

	private List<string> readScenario(string repositoryPath) {
		if (File.Exists(repositoryPath + Path.DirectorySeparatorChar + "Scenario.xml")) {
			List<string> levelList = new List<string>();
			XmlDocument doc = new XmlDocument();
			doc.Load(repositoryPath + Path.DirectorySeparatorChar + "Scenario.xml");
			XmlNode root = doc.ChildNodes[1]; //root = <scenario/>
			foreach (XmlNode child in root.ChildNodes) {
				if (child.Name.Equals("level")) {
					levelList.Add(repositoryPath + Path.DirectorySeparatorChar + (child.Attributes.GetNamedItem("name").Value));
				}
			}
			return levelList;
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
		foreach (GameObject directory in levelButtons.Keys) {
			//show directory buttons
			GameObjectManager.setGameObjectState(directory, true);
			//hide level buttons
			foreach (GameObject level in levelButtons[directory]) {
				GameObjectManager.setGameObjectState(level, false);
			}
		}
		GameObjectManager.setGameObjectState(playButton, false);
		GameObjectManager.setGameObjectState(quitButton, false);
		GameObjectManager.setGameObjectState(compLevelButton, false);
		GameObjectManager.setGameObjectState(backButton, true);
	}

	private void showLevels(GameObject levelDirectory) {
		//show/hide levels
		foreach (GameObject directory in levelButtons.Keys) {
			//hide level directories
			GameObjectManager.setGameObjectState(directory, false);
			//show levels
			if (directory.Equals(levelDirectory)) {
				//foreach(GameObject go in levelButtons[directory]){
				for (int i = 0; i < levelButtons[directory].Count; i++) {
					GameObjectManager.setGameObjectState(levelButtons[directory][i], true);

					string directoryName = levelDirectory.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
					//locked levels
					if (i > PlayerPrefs.GetInt(directoryName, 0)) //by default first level of directory is the only unlocked level of directory
						levelButtons[directory][i].transform.Find("Button").GetComponent<Button>().interactable = true;
					//unlocked levels
					else {
						levelButtons[directory][i].transform.Find("Button").GetComponent<Button>().interactable = true;
						//scores
						int scoredStars = PlayerPrefs.GetInt(directoryName + Path.DirectorySeparatorChar + i + gameData.scoreKey, 0); //0 star by default
						Transform scoreCanvas = levelButtons[directory][i].transform.Find("ScoreCanvas");
						for (int nbStar = 0; nbStar < 4; nbStar++) {
							if (nbStar == scoredStars)
								GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, true);
							else
								GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, false);
						}
					}
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

	public void launchLevel(string levelDirectory, int level) {
		gameData.levelToLoad = (levelDirectory, level);
		GameObjectManager.loadScene("MainScene");
	}

	// See Retour button in editor
	public void backFromCampagneMenu() {
		foreach (GameObject directory in levelButtons.Keys) {
			if (directory.activeSelf) {
				//main menu
				GameObjectManager.setGameObjectState(campagneMenu, false);
				GameObjectManager.setGameObjectState(playButton, true);
				GameObjectManager.setGameObjectState(quitButton, true);
				GameObjectManager.setGameObjectState(backButton, false);
				GameObjectManager.setGameObjectState(compLevelButton, true);
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

	// Initialise tous ce qui concerne les fonctionalités
	private void paramFunction()
	{
		loadConstraintFunction();
		loadRequiermentLibrairy();
	}

	// Charge les différentes contraintes qui existe entre les fonctionalités
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
			gameData.GetComponent<FunctionalityParam>().active.Add(data[0], Convert.ToBoolean(data[4]));
			gameData.GetComponent<FunctionalityParam>().levelDesign.Add(data[0], Convert.ToBoolean(data[3]));
			List<string> tmp = new List<string>();
			var data_link = data[1].Split(',');
			foreach (string value in data_link)
			{
				tmp.Add(value);
			}
			gameData.GetComponent<FunctionalityParam>().activeFunc.Add(data[0], new List<string>(tmp));
			tmp = new List<string>();
			data_link = data[2].Split(',');
			foreach (string value in data_link)
			{
				tmp.Add(value);
			}
			gameData.GetComponent<FunctionalityParam>().enableFunc.Add(data[0], new List<string>(tmp));
		}
	}

	private void loadRequiermentLibrairy(){
		XmlDocument doc = new XmlDocument();
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			LevelGenerator.instance.startReadDocXML(doc);
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
					gameData.GetComponent<FunctionalityParam>().listCaptor.Add(childEle.Attributes.GetNamedItem("name").Value);
				}
			}
			else if(child.Name == "func")
            {
				List<string> listEleTemp = new List<string>();
				foreach (XmlNode childEle in child)
				{
					listEleTemp.Add(childEle.Attributes.GetNamedItem("name").Value);
					/*
					if (childEle.Attributes.GetNamedItem("name").Value != "Captor")
					{
						listEleTemp.Add(childEle.Attributes.GetNamedItem("name").Value);
					}
					else
					{
						for (int i = 0; i < gameData.GetComponent<FunctionalityParam>().listCaptor.Count; i++)
						{
							listEleTemp.Add(gameData.GetComponent<FunctionalityParam>().listCaptor[i]);
						}
					}
					*/
				}
				gameData.GetComponent<FunctionalityParam>().elementRequiermentLibrary.Add(child.Attributes.GetNamedItem("name").Value, listEleTemp);
			}
		}
	}

	// See Quitter button in editor
	public void quitGame(){
		Application.Quit();
	}
}
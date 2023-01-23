using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class GameData {
	public static string mode = "Campagne infiltration"; //Campagne infiltration / Homemade

	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	// public static GameObject Level;
	public static Hashtable levelList; //key = directory name, value = list of level file name
	// public static Dictionary <string, List<string>> levelList; //key = directory name, value = list of level file name
	public static (string, int) levelToLoad = ("Campagne infiltration", 1); //directory name, level index
	public static string homemadeLevelToLoad = "level1"; 
	public static int[] levelToLoadScore; //levelToLoadScore[0] = best score (3 stars) ; levelToLoadScore[1] = medium score (2 stars)

	public static List<Pool> poolTree;
	public static List<Level> homemadeList;

	public static List<(string,float,string,float, int, int)> dialogMessage; //list of (dialogText, dialogHeight, imageName, imageHeight, camX, camY)
	public static Hashtable actionBlockLimit; //Is block available in library?
	// public static Dictionary<string, int> actionBlockLimit; //Is block available in library?
	public static string scoreKey = "score";
	public static int totalStep;
	public static int totalActionBlocUsed;
	public static int totalExecute;
	public static int totalCoin;
	// public static GameObject actionsHistory; //all actions made in the level, displayed at the end
	public static float gameSpeed_default = 1f;
	public static float gameSpeed_current = 1f;
	public static bool dragDropEnabled = true;

	public static string gameLanguage = "en";
	public static string convoNode = "-1";
	
	
}
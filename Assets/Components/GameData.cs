using UnityEngine;
using System.Collections.Generic;

public class GameData : MonoBehaviour {
	public static GameData Instance;
	public string mode = "Campagne infiltration"; //Campagne infiltration / Homemade

	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public GameObject Level;
	public Dictionary <string, List<string>> levelList; //key = directory name, value = list of level file name
	public (string, int) levelToLoad = ("Campagne infiltration", 1); //directory name, level index
	public string homemadeLevelToLoad; 
	public int[] levelToLoadScore; //levelToLoadScore[0] = best score (3 stars) ; levelToLoadScore[1] = medium score (2 stars)
	
	public List<(string,float,string,float, int, int)> dialogMessage; //list of (dialogText, dialogHeight, imageName, imageHeight, camX, camY)
	
	public Dictionary<string, int> actionBlockLimit; //Is block available in library?
	public string scoreKey = "score";
	public int totalStep;
	public int totalActionBlocUsed;
	public int totalExecute;
	public int totalCoin;
	public GameObject actionsHistory; //all actions made in the level, displayed at the end
	public float gameSpeed_default = 1f;
	public float gameSpeed_current = 1f;
	public bool dragDropEnabled = true;

	public string gameLanguage = "en";
	public string convoNode = "0";
}
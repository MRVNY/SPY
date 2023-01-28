using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;


[Serializable]
public class GameData {
	public string path = "/StreamingAssets/Levels/";
	public string mode = "SkillTree"; //SkillTree / Homemade
	public Level level;
	public int ending = 0;
	public string player = "Student";

	public Hashtable levelNameList = new Hashtable();
	public int[] levelScore; //levelToLoadScore[0] = best score (3 stars) ; levelToLoadScore[1] = medium score (2 stars)

	public Node Tree;
	//public List<Level> homemadeList;

	public List<(string,float,string,float, int, int)> dialogMessage; //list of (dialogText, dialogHeight, imageName, imageHeight, camX, camY)
	public Hashtable actionBlockLimit; //Is block available in library?
	public Hashtable score;

	public string scoreKey = "score";
	public int totalStep;
	public int totalActionBlocUsed;
	public int totalExecute;
	public int totalCoin;
	public int difficulty = 1;

	public float gameSpeed_default = 1f;
	public float gameSpeed_current = 1f;
	public bool dragDropEnabled = true;

	public string gameLanguage = "en";
	public string convoNode = "-1";
}

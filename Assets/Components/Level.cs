using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class Level
{
	public Node node;

	public string name = "Niveau1";
	public bool active = false;
	public int difficulty = 0;
	public Lvltype type = Lvltype.normal;
	public List<Level> next;
	
	public int bestCode = 50;
	public int bestExec = 50;

	public Star score = Star.Undone;
	public int codeScore = 0;
	public int execScore = 0;
	public Hashtable Competence_lv;
}

public enum Lvltype { normal, debug };
public enum Star { Undone, Done, Code, Exec, All };

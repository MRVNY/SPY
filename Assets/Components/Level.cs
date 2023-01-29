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
	public int difficulty = 0;
	public Lvltype type = Lvltype.normal;
	public Level next;

	public int bestCode = Int32.MaxValue;
	public int bestExec = Int32.MaxValue;

	public Star score = Star.Undone;
	public int codeScore = 0;
	public int execScore = 0;
	public Hashtable Competence_lv;
}

public enum Lvltype { normal, debug };
public enum Star { Undone, Done, Code, Exec, All };

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Level
{
	public Node node;

	public string name = "Niveau1";
	public bool active = false;
	public int difficulty = 0;
	public Lvltype type = Lvltype.normal;
	public List<Level> next;
	
	public int bestCode = Int32.MaxValue;
	public int bestExec = Int32.MaxValue;
	
	public Star score = Star.Undone;
	public int codeScore = 0;
	public int execScore = 0;
}

public enum Lvltype { normal, debug };
public enum Star { Undone, Done, Code, Exec, All };
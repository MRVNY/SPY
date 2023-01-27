using System;
using UnityEngine;

[Serializable]
public class Level
{
	public string name = "Niveau1";
	public int difficulty = 0;
	public Lvltype type = Lvltype.normal;
	public Level next;
	
	public int bestCode = Int32.MaxValue;
	public int bestExec = Int32.MaxValue;
	
	public Star score = Star.Undone;
	public int codeScore = 0;
	public int execScore = 0;
}

public enum Lvltype { normal, debug };
public enum Star { Undone, Done, Code, Exec, All };
using System;
using UnityEngine;

[Serializable]
public class Level
{
	public string name = "Niveau1";
	public Star score = Star.Undone;
	public int difficulty = 0;
	public Lvltype type = Lvltype.normal;
	public Level next;
}

public enum Lvltype { normal, debug };
public enum Star { Undone, Done, Code, Exec, All };
using System;
using UnityEngine;

[Serializable]
public class Level : MonoBehaviour
{
	public difficulty difficulty;
	public lvltype type;
}

public enum difficulty { easy, medium, hard };
public enum lvltype { normal, debug };
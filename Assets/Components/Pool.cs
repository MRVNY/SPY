using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Pool : MonoBehaviour {
	public List<Level> levelPool;
	public Level enterLevel;
	public Level exitLevel;
	public Pool dependentPool;
}
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Node
{
	public string name;
	
	public List<Level> levelPool;
	
	public List<Level> trainingLevels;
	public List<Level> introLevels;
	public List<Level> outroLevels;
	
	//public List<Node> lastNodes;
	public List<Node> nextNodes;
}
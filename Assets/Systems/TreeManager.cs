using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using FYFY;

/// <summary>
/// Manage dialogs at the begining of the level
/// </summary>
public class TreeManager : FSystem
{
	public List<Node> nodes;
	public List<Level> levels;

	protected override void onStart()
	{
		//Global.GD;
		// Node Intro = makeNode("Intro");
		// Node If = makeNode("If");
		// Node While = makeNode("While");
		// Node WhileIf = makeNode("WhileIf");
		//
		// Intro.nextNodes.Add(If);
		// Intro.nextNodes.Add(While);
		//
		// If.nextNodes.Add(WhileIf);
		// While.nextNodes.Add(WhileIf);
		//
		// Global.GD.Tree = Intro;
		//
		// levels = ToList(Intro);
	}

	private static Node makeNode(string name)
	{
		Node node = new Node();
		node.name = name;
		node.levelPool = new List<Level>();
		node.introLevels = new List<Level>();
		node.outroLevels = new List<Level>();
		node.trainingLevels = new List<Level>();
		node.nextNodes = new List<Node>();
		return node;
	}

	private static Level makeLevel(string name, int difficulty, Lvltype type)
	{
		Level level = new Level();
		level.name = name;
		level.difficulty = difficulty;
		level.type = Lvltype.normal;
		return level;
	}

	public List<Level> ToList(Node node)
	{
		nodes = new List<Node>();
		flattenTree(node);
		return extractLevels();
	}

	private void flattenTree(Node node)
	{
		if (!nodes.Contains(node))
		{
			nodes.Add(node);
			foreach (var next in node.nextNodes)
			{
				flattenTree(next);
			}
		}
	}

	private List<Level> extractLevels()
	{
		List<Level> tmp = new List<Level>();
		foreach (var node in nodes)
		{
			tmp.AddRange(node.introLevels);
			tmp.AddRange(node.levelPool);
			tmp.AddRange(node.outroLevels);
		}

		return tmp;
	}

	private static Node findNode(Node start, string name)
	{
		if (start != null)
		{
			if (start.name == name) return start;
			foreach (var node in start.nextNodes)
			{
				Node output = findNode(node, name);
				if (output != null) return output;
			}
		}
		return null;
	}
	
	public static void ConstructTree()
	{
		Global.GD.path = Application.streamingAssetsPath + "/Levels/";
		//get all the folder names under a path
		foreach (string directory in Directory.GetDirectories(Global.GD.path))
		{
			foreach (string lvl in Directory.GetFiles(directory))
			{
				if (File.Exists(lvl) && lvl.EndsWith(".xml") && !lvl.EndsWith("Scenario.xml")){
					XDocument doc = XDocument.Load(lvl);
					XElement levelInfo = doc.Element("level").Element("levelInfo");
					XElement nodeInfo = doc.Element("level").Element("nodeInfo");

					if (nodeInfo != null)
					{
						Node target = findNode(Global.GD.Tree, nodeInfo.Attribute("name").Value);
						if (target == null)
						{
							target = makeNode(nodeInfo.Attribute("name").Value);
							Global.GD.Tree = target;
						}

						foreach (var next in nodeInfo.Attribute("next").Value.Split(','))
						{
							if (next != "")
							{
								Node nextNode = findNode(Global.GD.Tree, next);
								if(nextNode == null) nextNode = makeNode(next);
								if(!target.nextNodes.Contains(nextNode)) target.nextNodes.Add(nextNode);
							}
						}
					}

					if (levelInfo != null)
					{
						Node parent = findNode(Global.GD.Tree, levelInfo.Attribute("node").Value);
						Level level = makeLevel(Path.GetFileNameWithoutExtension(lvl),0,Lvltype.normal);
						//level.name = levelInfo.Attribute("name").Value;
						string type = levelInfo.Attribute("type").Value;
						if (levelInfo.LastAttribute.Name == "difficulty")
						{
							level.difficulty = int.Parse(levelInfo.Attribute("difficulty").Value);
						}

						switch (type)
						{
							case "intro":
								parent.introLevels.Add(level);
								break;
							case "outro":
								parent.outroLevels.Add(level);
								break;
							case "pool":
								parent.levelPool.Add(level);
								break;
						}
					}
					// else
					// {
					// 	Node parent = findNode(Global.GD.Tree, "Intro");
					// 	Level level = new Level();
					// 	//level.name = levelInfo.Attribute("name").Value;
					// 	level.name = Path.GetFileName(lvl);
					// }
				}
			}
		}
	}
}
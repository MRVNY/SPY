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

	private static Level makeLevel(string name, int difficulty, Lvltype type, Node node)
	{
		Level level = new Level();
		level.name = name;
		level.difficulty = difficulty;
		level.type = Lvltype.normal;
		level.node = node;
		level.next = new List<Level>();
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
	
	public static async Task ConstructTree()
	{
		Global.GD.path = Application.streamingAssetsPath + Path.DirectorySeparatorChar +
		                 "Levels" + Path.DirectorySeparatorChar;
		//get all the folder names under a path
		foreach (string directory in Directory.GetDirectories(Global.GD.path))
		{
			if(File.Exists(directory+Path.DirectorySeparatorChar+"Nodes.xml")){
				string nodesXml = Directory.GetFiles(directory,"*Nodes.xml").First();
				if (File.Exists(nodesXml) && nodesXml.EndsWith("Nodes.xml"))
				{
					XDocument doc = XDocument.Load(nodesXml);

					foreach (var nodeInfo in doc.Element("level").Elements("nodeInfo").ToList())
					{
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
									if (nextNode == null) nextNode = makeNode(next);
									if (!target.nextNodes.Contains(nextNode)) target.nextNodes.Add(nextNode);
								}
							}
						}
					}
				}
			}
			
			// var files = Directory.GetFiles(directory, "*.xml").ToList();
			// files.Remove(Directory.GetFiles(directory, "*Scenario.xml").First());
			// files.Sort((s, s1) => 
			// 	int.Parse(Path.GetFileNameWithoutExtension(s).Substring(6)).CompareTo(
			// 		int.Parse(Path.GetFileNameWithoutExtension(s1).Substring(6))));
			// Debug.Log(files);
			foreach (string lvl in Directory.GetFiles(directory))
			{

				if (File.Exists(lvl) && lvl.EndsWith(".xml") && !lvl.EndsWith("Scenario.xml")){
					Debug.Log(lvl);
					XDocument doc = XDocument.Load(lvl);
					string[] levelInfo = doc.Element("level").Element("levelInfo").ToString().Split("-");
					XElement competenceInfo = doc.Element("level").Element("blockLimits");


					if (levelInfo.Length==3)
					{
						string node = levelInfo[0];
						string type = levelInfo[1];
						int index = int.Parse(levelInfo[2])-1;
						
						Node parent = findNode(Global.GD.Tree, node);
						if (parent != null)
						{
							Level level = makeLevel(Path.GetFileNameWithoutExtension(lvl), 0, Lvltype.normal, parent);
							//level.name = levelInfo.Attribute("name").Value;
							// if (levelInfo.LastAttribute.Name == "difficulty")
							// {
							// 	level.difficulty = int.Parse(levelInfo.Attribute("difficulty").Value);
							// }

							switch (type)
							{
								case "intro":
									parent.introLevels.Insert(index,level);
									break;
								case "outro":
									parent.outroLevels.Insert(index,level);
									break;
								case "pool":
									parent.levelPool.Insert(index,level);
									parent.trainingLevels.Insert(index,level);
									break;
							}

							if (competenceInfo != null)
							{
								Debug.Log("notnull");
								foreach (XElement element in competenceInfo.Elements())
								{
									Debug.Log("child");
									//Debug.Log(element.Attribute("limit").Value);
									if (element.Attribute("limit").Value != "1")
									{
										string block_name = element.Attribute("blockType").Value;
										Debug.Log(block_name);
										if (("If" == block_name) | ("IfElse" == block_name))
											level.Competence_lv["If"] = 1;
										else if ("While" == block_name) level.Competence_lv["While"] = 1;
										else if ("For" == block_name) level.Competence_lv["For"] = 1;
										else if (("AndOperator" == block_name) | ("OrOperator" == block_name) |
										         ("NotOperator" == block_name)) level.Competence_lv["Operator"] = 1;
										else break;
										Debug.Log(level.Competence_lv);
									}
								}
							}
							else Debug.Log("whyNull");
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
		LinkLevels(Global.GD.Tree);
		
		await Task.Delay(0);
	}
	
	private static void LinkLevels(Node node)
	{
		if (node != null)
		{
			for(int i=0; i<node.introLevels.Count-1; i++)
			{
				node.introLevels[i].next.Add(node.introLevels[i + 1]);
			}

			if (node.trainingLevels.Count > 0)
			{
				node.introLevels.Last().next.Add(node.trainingLevels[0]);
				for (int i = 0; i < node.trainingLevels.Count - 1; i++)
				{
					node.trainingLevels[i].next.Add(node.trainingLevels[i + 1]);
				}

				node.trainingLevels.Last().next.Add(node.outroLevels[0]);
			}

			for(int i=0; i<node.outroLevels.Count-1; i++)
			{
				node.outroLevels[i].next.Add(node.outroLevels[i + 1]);
			}
			
			foreach (var next in node.nextNodes)
			{
				node.outroLevels.Last().next.Add(next.introLevels[0]);
				LinkLevels(next);
			}
		}
	}
}
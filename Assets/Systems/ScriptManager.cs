using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using UnityEngine;
using FYFY;

/// <summary>
/// This system executes new currentActions
/// </summary>
public class ScriptManager : FSystem {
	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)), new AnyOfTags("Player"));

	public static ScriptManager instance;
	public XElement script;
	public XDocument doc;
	private string[] CondOps = new []{"AND", "OR", "NOT"};
	
	protected override void onStart()
	{
		instance = this;
		if(LevelEditorSystem.xmlPath != null)
			doc = XDocument.Load(LevelEditorSystem.xmlPath);
	}
	
	public void TranslateScript()
	{
		foreach (GameObject robot in f_player)
		{
			GameObject root = robot.GetComponent<ScriptRef>().executableScript;
			string name = robot.GetComponent<AgentEdit>().associatedScriptName;
			
			script = new XElement("script", 
				new XAttribute("name", name),
				new XAttribute("editMode", "2"),
				new XAttribute("type", "3"));

			script = TranslateNode(root.transform.GetChild(0).gameObject, script);
		}
	}
	
	public void SaveScript()
	{
		Debug.Log(doc);
		Debug.Log(script);
		doc.Element("level").Add(script);
		Debug.Log(doc);

		doc.Save(LevelEditorSystem.xmlPath);
		
		GameObjectManager.loadScene("MainScene");
	}

	public void DeleteLevel()
	{
		System.IO.File.Delete(LevelEditorSystem.xmlPath);
		
		XDocument scenario = XDocument.Load(LevelEditorSystem.scenarioPath);
		scenario.Element("scenario").Elements("level").Where(x => x.Attribute("name").Value == LevelEditorSystem.fileName).Remove();
		scenario.Save(LevelEditorSystem.scenarioPath);
		
		GameObjectManager.loadScene("LevelEditor");
	}

	public XElement TranslateNode(GameObject node, XElement script)
	{
		BaseElement ele = node.GetComponent<BaseElement>();
		if (ele is BasicAction)
		{
			script.Add(new XElement("action", 
				new XAttribute("type", ((BasicAction)ele).actionType.ToString())));
		}
		
		else if (ele is ControlElement)
		{
			XElement control = null;
			
			if (ele is ForeverControl)
			{//forever
				control = TranslateNode(((ForeverControl)ele).firstChild, new XElement("forever"));
			}
			
			else if (ele is ForControl)
			{
				if (ele is WhileControl)
				{//while
					WhileControl castEle = (WhileControl)ele;
					control = new XElement("while");

					
					control.Add(new XElement("condition", TranslateConditions(castEle.condition)));
					control.Add(TranslateNode(castEle.firstChild, new XElement("container")));
				}
				else
				{//for
					ForControl castEle = (ForControl)ele;
					control = new XElement("forever", new XAttribute("nbFor",castEle.nbFor));
					control = TranslateNode(castEle.firstChild, control);
				}
			}
			
			else if (ele is IfControl)
			{
				if (ele is IfElseControl)
				{//ifelse
					IfElseControl castEle = (IfElseControl)ele;
					control = new XElement("if");
				
					control.Add(new XElement("condition", TranslateConditions(castEle.condition)));
					control.Add(TranslateNode(castEle.firstChild, new XElement("thenContainer")));
					control.Add(TranslateNode(castEle.elseFirstChild, new XElement("elseContainer")));
				}
				else
				{//if
					IfControl castEle = (IfControl)ele;
					control = new XElement("if");
					
					control.Add(new XElement("condition", TranslateConditions(castEle.condition)));
					control.Add(TranslateNode(castEle.firstChild, new XElement("container")));
				}
			}
			
			if(control != null)
				script.Add(control);
		}
		
		if (ele.next != null && ele.next != ele.transform.parent.parent.gameObject)
			script = TranslateNode(ele.next, script);
		
		return script;
	}

	XElement TranslateConditions(List<string> conditions)
	{
		if (conditions.Count > 0 && conditions[0] == "(")
		{
			int layer = 0;
			for (int i = 1; i < conditions.Count; i++)
			{
				string line = conditions[i];

				if (line == "(")
					layer++;
				else if (line == ")")
					layer--;
				else if (CondOps.Contains(line) && layer == 0)
				{
					XElement cond = new XElement(line.ToLower());

					if (line == "NOT")
					{
						cond.Add(TranslateConditions(conditions.GetRange(i+1,conditions.Count-i-2)));
					}
					else
					{
						XElement left = new XElement("conditionLeft");
						XElement right = new XElement("conditionRight");
						
						left.Add(TranslateConditions(conditions.GetRange(1,i-1)));
						right.Add(TranslateConditions(conditions.GetRange(i+1,conditions.Count-i-2)));
						
						cond.Add(left);
						cond.Add(right);
					}
					
					return cond;
				}
			}
		}
		else
		{
			return new XElement("captor", new XAttribute("type", conditions[0]));
		}

		return null;
	}
	
	
}

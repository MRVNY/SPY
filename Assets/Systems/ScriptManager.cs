using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using FYFY;

/// <summary>
/// This system executes new currentActions
/// </summary>
public class ScriptManager : FSystem {
	private Family f_wall = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall", "Door"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_activableConsole = FamilyManager.getFamily(new AllOfComponents(typeof(Activable),typeof(Position),typeof(AudioSource)));
    private Family f_newCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));
	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)), new AnyOfTags("Player"));

	protected override void onStart()
	{
		f_newCurrentAction.addEntryCallback(onNewCurrentAction);
		Pause = true;
	}

	protected override void onProcess(int familiesUpdateCount)
	{
		// count inaction if a robot have no CurrentAction
		foreach (GameObject robot in f_player)
			if (robot.GetComponent<ScriptRef>().executableScript.GetComponentInChildren<CurrentAction>() == null)
				robot.GetComponent<ScriptRef>().nbOfInactions++;
		Pause = true;
	}

	// each time a new currentAction is added, 
	private void onNewCurrentAction(GameObject currentAction) {
		Pause = false; // activates onProcess to identify inactive robots
		
		CurrentAction ca = currentAction.GetComponent<CurrentAction>();	

		// // process action depending on action type
		// switch (currentAction.GetComponent<BasicAction>().actionType){
		// 	case BasicAction.ActionType.Forward:
		// 		ApplyForward(ca.agent);
		// 		break;
		// 	case BasicAction.ActionType.TurnLeft:
		// 		ApplyTurnLeft(ca.agent);
		// 		break;
		// 	case BasicAction.ActionType.TurnRight:
		// 		ApplyTurnRight(ca.agent);
		// 		break;
		// 	case BasicAction.ActionType.TurnBack:
		// 		ApplyTurnBack(ca.agent);
		// 		break;
		// 	case BasicAction.ActionType.Wait:
		// 		break;
		// 	case BasicAction.ActionType.Activate:
		// 		Position agentPos = ca.agent.GetComponent<Position>();
		// 		foreach ( GameObject actGo in f_activableConsole){
		// 			if(actGo.GetComponent<Position>().x == agentPos.x && actGo.GetComponent<Position>().y == agentPos.y){
		// 				actGo.GetComponent<AudioSource>().Play();
		// 				// toggle activable GameObject
		// 				if (actGo.GetComponent<TurnedOn>())
		// 					GameObjectManager.removeComponent<TurnedOn>(actGo);
		// 				else
		// 					GameObjectManager.addComponent<TurnedOn>(actGo);
		// 			}
		// 		}
		// 		ca.agent.GetComponent<Animator>().SetTrigger("Action");
		// 		break;
		// }
		// notify agent moving
		if (ca.agent.CompareTag("Drone") && !ca.agent.GetComponent<Moved>())
			GameObjectManager.addComponent<Moved>(ca.agent);
	}

	public void TranslateScript(XDocument doc)
	{
		foreach (GameObject robot in f_player)
		{
			GameObject root = robot.GetComponent<ScriptRef>().executableScript;
			string name = robot.GetComponent<AgentEdit>().associatedScriptName;
			
			XElement script = new XElement("script", 
				new XAttribute("name", name),
				new XAttribute("editMode", "2"),
				new XAttribute("type", "3"));
			doc.Add(TranslateNode(root.transform.GetChild(0).gameObject, script));
		}

	}

	public XElement TranslateNode(GameObject node, XElement script)
	{
		BaseElement ele = node.GetComponent<BaseElement>();
		if (ele is BasicAction)
		{
			script.Add(new XElement("action", 
				new XAttribute("type", ((BasicAction)ele).actionType.ToString())));
			
			// switch (((BasicAction)ele).actionType)
			// {
			// 	case BasicAction.ActionType.Forward:
			// 		script.Add(new XElement("Forward"));
			// 		break;
			// 	case BasicAction.ActionType.TurnLeft:
			// 		script.Add(new XElement("TurnLeft"));
			// 		break;
			// 	case BasicAction.ActionType.TurnRight:
			// 		script.Add(new XElement("TurnRight"));
			// 		break;
			// 	case BasicAction.ActionType.TurnBack:
			// 		script.Add(new XElement("TurnBack"));
			// 		break;
			// 	case BasicAction.ActionType.Wait:
			// 		script.Add(new XElement("Wait"));
			// 		break;
			// 	case BasicAction.ActionType.Activate:
			// 		script.Add(new XElement("Activate"));
			// 		break;
			// }
		}
		
		else if (ele is ControlElement)
		{
			XElement control = null;
			
			if (ele is ForeverControl)
			{//forever
				control = new XElement("forever");
				control.Add(TranslateNode(((ForeverControl)ele).firstChild, control));
			}
			
			else if (ele is ForControl)
			{
				if (ele is WhileControl)
				{//while
					control = new XElement("while");
					control.Add(TranslateNode(((ForeverControl)ele).firstChild, control));
				}
				else
				{//for
					
				}
			}
			
			else if (ele is IfControl)
			{
				if (ele is IfElseControl)
				{//ifelse
					
				}
				else
				{//if
					
				}
			}
			
			if(control != null)
				script.Add(control);
		}
		
		if (ele.next != null)
			TranslateNode(ele.next, script);
		
		return script;
	}

	XElement TranslateConditions(List<string> conditions)
	{

		return null;
	}
}

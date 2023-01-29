using System;
using UnityEngine;
using FYFY;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FYFY_plugins.TriggerManager;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = System.Random;

/// <summary>
/// Manage collision between player agents and Coins
/// </summary>
public class LevelEditorSystem : FSystem {
	private Family f_levelEditor = FamilyManager.getFamily(new AllOfComponents(typeof(LevelEditor)));
	
	private LevelEditor levelEditor;
	private List<string> dirs = new List<string>{"N","S","W","E"};
	private List<string> dorDirs = new List<string>{"H","N","V"};
	private string[] limits = new String[]
	{
		"Forward", "TurnLeft", "TurnRight", "Wait", "Activate", "TurnBack", "If", "IfElse", "For", "While",
		"Forever", "AndOperator", "OrOperator", "NotOperator", "Wall", "Enemie", "RedArea", "FieldGate", "Terminal",
		"Exit"
	};
	
	private XDocument xml;
	private XElement xmlLevel;
	private XElement xmlMap;
	
	Hashtable AgentsInputBoxes = new Hashtable();
	Hashtable ConsoleInputBoxes = new Hashtable();
	Hashtable DoorInputBoxes = new Hashtable();
	
	string[] autoNames = new []{"K", "B", "C", "D", "E", "F", "G", "H", "I", "J", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

	public static string fileName;
	public static string xmlPath;
	public static string scenarioPath;
    
	protected override void onStart()
    {
	    levelEditor = f_levelEditor.First().GetComponent<LevelEditor>();

	    Vector2 agentPos = Vector2.zero;
		
		xml = new XDocument();
		xml.Declaration = new XDeclaration("1.0", "utf-8", "true");
		
		xmlLevel = new XElement("level");
		xmlMap = new XElement("map");
		
		levelEditor.Map.CompressBounds();
		var bounds = levelEditor.Map.cellBounds;
		
		for (int y = bounds.yMax; y >= bounds.yMin; y--)
		{
			XElement xmlLine = new XElement("line");

	        for (int x = bounds.xMin; x < bounds.xMax; x++)
	        {
		        Vector3Int localPlace = (new Vector3Int(x, y, (int)levelEditor.Map.transform.position.y));

		        levelEditor.Map.SetTileFlags(localPlace, TileFlags.None);
                if (levelEditor.Map.HasTile(localPlace))
                {
	                var tileName = levelEditor.Map.GetTile(localPlace).name;

	                switch (tileName)
	                {
		                case "Wall":
			                xmlLine.Add(new XElement("cell", new XAttribute("value", "1")));
			                break;
		                case "Cell":
			                xmlLine.Add(new XElement("cell",new XAttribute("value", "0")));
			                break;
		                case "Red":
			                xmlLine.Add(new XElement("cell",new XAttribute("value", "3")));
			                break;
		                case "Blue":
			                xmlLine.Add(new XElement("cell",new XAttribute("value", "2")));
			                break;
	                }

	                if (levelEditor.Objects.HasTile(localPlace))
	                {
		                var obounds = levelEditor.Objects.cellBounds;

		                int xx = bounds.xMax + x;
		                int yy = bounds.yMax - y;
		                var objectName = levelEditor.Objects.GetTile(localPlace).name.Split("_");
		                string objectType = objectName[0];
		                string objectDir = objectName[objectName.Length - 1];
		                GameObject inputBox = null;
		                Vector3 screenPos;
		                
		                switch (objectType)
		                {
			                case "Coin":
				                xmlLevel.Add(new XElement("coin",
					                new XAttribute("posY", yy),
					                new XAttribute("posX", xx)));
				                break;
			                
			                case "Robot":
				                // Debug.Log("x : " + x + " y : " + y);
				                // Debug.Log("xx : " + xx + " yy : " + yy);
				                // Debug.Log(bounds);
				                // Debug.Log(obounds);
				                
				                if (!levelEditor.AgentsAutoNameing){
					                screenPos = Camera.main.WorldToScreenPoint(levelEditor.Objects.CellToWorld(localPlace));
					                inputBox = GameObject.Instantiate(levelEditor.InputAgent, screenPos, Quaternion.identity, levelEditor.Canvas);
					                AgentsInputBoxes.Add(new Vector3(xx,yy,dirs.IndexOf(objectDir)), inputBox.GetComponent<TMP_InputField>());
				                }
				                else
				                {
					                AgentsInputBoxes.Add(new Vector3(xx,yy,dirs.IndexOf(objectDir)), null);
				                }
				                
				                break;

			                case "Drone":
				                xmlLevel.Add(new XElement("enemy",
					                new XAttribute("associatedScriptName","Guarde"),
					                new XAttribute("posY",yy),
					                new XAttribute("posX",xx),
					                new XAttribute("direction", dirs.IndexOf(objectDir).ToString()),
					                new XAttribute("range","2"),
					                new XAttribute("selfRange","False"),
					                new XAttribute("typeRange","0")));
				                break;

			                case "Console":
				                if (!levelEditor.AgentsAutoNameing)
								{
					                screenPos = Camera.main.WorldToScreenPoint(levelEditor.Objects.CellToWorld(localPlace));
					                inputBox = GameObject.Instantiate(levelEditor.InputId, screenPos, Quaternion.identity, levelEditor.Canvas);
					                ConsoleInputBoxes.Add(new Vector3(xx,yy,dirs.IndexOf(objectDir)), inputBox.GetComponent<TMP_InputField>());
				                }
				                else
				                {
					                ConsoleInputBoxes.Add(new Vector3(xx,yy,dirs.IndexOf(objectDir)), null);
				                }
				                break;

			                case "Door":
				                if (!levelEditor.ConsoleAutoLinking)
				                {
					                screenPos = Camera.main.WorldToScreenPoint(levelEditor.Objects.CellToWorld(localPlace));
					                inputBox = GameObject.Instantiate(levelEditor.InputId, screenPos, Quaternion.identity, levelEditor.Canvas);
					                DoorInputBoxes.Add(new Vector3(xx,yy,dorDirs.IndexOf(objectDir)), inputBox.GetComponent<TMP_InputField>());
				                }
				                else
				                {
					                DoorInputBoxes.Add(new Vector3(xx,yy,dorDirs.IndexOf(objectDir)), null);
				                }
				                break;
		                }
	                }
                }
                else
                {
	                //xmlLine.Add(new XElement("cell", "-1"));
	                xmlLine.Add(new XElement("cell",new XAttribute("value", "-1")));
                }
	        }
	        xmlMap.Add(xmlLine);
        }

        if (!levelEditor.ConsoleAutoLinking)
	    {
		    //GameObject.Instantiate(levelEditor.InputId, new Vector3(i * 2.0f, 0, 0), Quaternion.identity);
	    }
    }

	public void ReadLevel()
	{
		xmlLevel.Add(xmlMap);

		List<Vector3> agents = AgentsInputBoxes.Keys.OfType<Vector3>().ToList();
		foreach (var box in agents)
		{
			string tmpName = "K";
			if (levelEditor.AgentsAutoNameing) tmpName = autoNames[agents.IndexOf(box)];
			else tmpName = ((TMP_InputField)AgentsInputBoxes[box]).text;
			
			if (tmpName == "") tmpName = "K";

			xmlLevel.Add(new XElement("player",
				new XAttribute("associatedScriptName",tmpName),
				new XAttribute("posY",box.y),
				new XAttribute("posX",box.x),
				new XAttribute("direction", box.z.ToString())));
		}

		//Console
		List<Vector3> consoles = ConsoleInputBoxes.Keys.OfType<Vector3>().ToList();
		foreach (var box in consoles)
		{
			XElement tmpConsole = new XElement("console",
				new XAttribute("state", "1"),
				new XAttribute("posY", box.y),
				new XAttribute("posX", box.x),
				new XAttribute("direction", box.z.ToString()));
			
			string tmpId = "0";
			if (!levelEditor.ConsoleAutoLinking) tmpId = ((TMP_InputField)ConsoleInputBoxes[box]).text;
			if (tmpId == "") tmpId = "0";
			tmpConsole.Add(new XElement("slot", new XAttribute("slotId",tmpId)));

			xmlLevel.Add(tmpConsole);
		}
		
		//Door
		List<Vector3> doors = DoorInputBoxes.Keys.OfType<Vector3>().ToList();
		foreach (var box in doors)
		{
			string tmpId = "0";
			if (!levelEditor.ConsoleAutoLinking) tmpId = ((TMP_InputField)DoorInputBoxes[box]).text;
			if (tmpId == "") tmpId = "0";

			xmlLevel.Add( new XElement("door",
				new XAttribute("slotId", tmpId),
				new XAttribute("posY", box.y),
				new XAttribute("posX", box.x),
				new XAttribute("direction", box.z.ToString())));
		}

		XElement blockLimits = new XElement("blockLimits");
		foreach (string limit in limits)
		{
			int value = (int)levelEditor.GetType().GetField(limit).GetValue(levelEditor);
			//Debug.Log(limit + " " + value);
			blockLimits.Add(new XElement("blockLmit", new XAttribute("blockType",limit), new XAttribute("limit",value)));
		}

		xmlLevel.Add(blockLimits);

		xml.Add(xmlLevel);
        
		//Debug.Log(xml);

        ExportXML();
	}

	public void ReadXML()
	{
		
	}

	public void ExportXML()
	{
		fileName = DateTime.Now.ToString("s") + ".xml";
		xmlPath = Application.streamingAssetsPath + "/Levels/Homemade/" + fileName;
		scenarioPath = Application.streamingAssetsPath + "/Levels/Homemade/Scenario.xml";
		
		//Save to Homemade folder
		xml.Save(xmlPath);

		//Add to Scenario.xml
		XDocument scenario = XDocument.Load(scenarioPath);
		scenario.Element("scenario").Add(new XElement("level", new XAttribute("name",fileName)));
		scenario.Save(scenarioPath);
		
		LoadLevel(Path.GetFileNameWithoutExtension(xmlPath));
	}

	public void LoadLevel(string levelName)
	{
		//if(Global.GD == null) GameStateManager.LoadGD();
		if (Global.GD == null)
		{
			Global.GD = new GameData();
			Global.GD.path = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" + Path.DirectorySeparatorChar;
		}
		
		Global.GD.mode = "Homemade";
		Level tmp = new Level();
		tmp.name = levelName;
		Global.GD.level = tmp;
		GameStateManager.SaveGD();
		GameStateManager.LoadGD();
		GameObjectManager.loadScene("ScriptEditor");
	}
	
	
	public void WriteLevel()
	{
		
	}
}
using System;
using UnityEngine;
using FYFY;
using System.Collections;
using System.Collections.Generic;
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
	
	public GameData prefabGameData;

	private LevelEditor levelEditor;
	private List<string> dirs = new List<string>{"N","S","W","E"};
	private List<string> dorDirs = new List<string>{"V","H"};
	
	private XDocument xml;
	private XElement xmlLevel;
	private XElement xmlMap;
	
	Hashtable AgentsInputBoxes = new Hashtable();
	Hashtable ConsoleInputBoxes = new Hashtable();
	Hashtable DoorOutputBoxes = new Hashtable();
	
	string[] autoNames = new []{"K", "B", "C", "D", "E", "F", "G", "H", "I", "J", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
    
	protected override void onStart()
    {
	    levelEditor = f_levelEditor.First().GetComponent<LevelEditor>();
	    
	    Vector2 agentPos = Vector2.zero;
		
		xml = new XDocument();
		xml.Declaration = new XDeclaration("1.0", "utf-8", "true");
		
		xmlLevel = new XElement("level");
		xmlMap = new XElement("map");
		var bounds = levelEditor.Map.cellBounds;
		for (int y = bounds.yMax; y >= bounds.yMin; y--)
		{
			XElement xmlLine = new XElement("line");

	        for (int x = bounds.xMin; x < bounds.xMax; x++)
	        {
		        Vector3Int localPlace = (new Vector3Int(x, y, (int)levelEditor.Map.transform.position.y));
		        
		        // x = x - bounds.xMin;
		        // y = y - bounds.yMin;

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
		                Debug.Log(x+","+y);
		                int xx = bounds.xMax + x;
		                int yy = bounds.yMax - y;
		                var objectName = levelEditor.Objects.GetTile(localPlace).name.Split("_");
		                string objectType = objectName[0];
		                string objectDir = objectName[objectName.Length - 1];
		                
		                switch (objectType)
		                {
			                case "Coin":
				                xmlLevel.Add(new XElement("coin",
					                new XAttribute("posY", y),
					                new XAttribute("posX", x)));
				                break;
			                
			                case "Robot":
				                if (!levelEditor.AgentsAutoNameing)
				                {
					                Vector3 screenPos = Camera.main.WorldToScreenPoint(levelEditor.Objects.CellToWorld(localPlace));
					                GameObject inputBox = GameObject.Instantiate(levelEditor.InputAgent, screenPos, Quaternion.identity);
					                inputBox.transform.SetParent(levelEditor.Canvas);
									AgentsInputBoxes.Add(new Vector3(xx,yy,dirs.IndexOf(objectDir)),
										inputBox.GetComponent<InputRef>());
				                }
				                break;

			                case "Drone":
				                xmlLevel.Add(new XElement("enemy",
					                new XAttribute("associatedScriptName","Guarde"),
					                new XAttribute("posY",y),
					                new XAttribute("posX",x),
					                new XAttribute("direction", dirs.IndexOf(objectDir).ToString()),
					                new XAttribute("range","2"),
					                new XAttribute("selfRange","False"),
					                new XAttribute("typeRange","0")));
				                break;

			                case "Console":
				                xmlLevel.Add(new XElement("console",
					                new XAttribute("state","1"), //todo
					                new XAttribute("posY",y),
					                new XAttribute("posX",x),
					                new XAttribute("direction", dirs.IndexOf(objectDir).ToString())));
				                break;
			                
			                case "Door":
				                xmlLevel.Add(new XElement("console",
					                new XAttribute("posY",y),
					                new XAttribute("posX",x),
					                new XAttribute("slotId","0"), //todo
					                new XAttribute("direction", dirs.IndexOf(objectDir).ToString())));
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
			if (levelEditor.AgentsAutoNameing)
			{
				tmpName = autoNames[agents.IndexOf(box)];
			}
			// else { tmpName = ((GameObject)AgentsInputBoxes[box]).transform.Find("Text").GetComponent<InputField>().text; }
			else
			{
				tmpName = ((InputRef)AgentsInputBoxes[box]).inputField.text;
			}

			xmlLevel.Add(new XElement("player",
				new XAttribute("associatedScriptName",tmpName), //todo
				new XAttribute("posY",box.y),
				new XAttribute("posX",box.x),
				new XAttribute("direction", box.z.ToString())));
			break;
		}
		

        
        // xmlLevel.Add(
	       //  XDocument.Parse("<dialogs><dialog text=\"23 août 2041...&#xA;&#xA;Vous avez un message...\"/></dialogs>"));
        // xmlLevel.Add(
	       //  XDocument.Parse("<blockLimits><blockLimit blockType=\"Forward\" limit=\"1\" /></blockLimits>"));
        // xmlLevel.Add(
		      //   XDocument.Parse("<player associatedScriptName=\"Karl\" posY=\"2\" posX=\"1\" direction=\"0\" /><script name='Karl' editMode='0' /><score twoStars='0' threeStars='10500'/>"));

        xml.Add(xmlLevel);

        string fileName = DateTime.Now.ToString("s") + ".xml";
        string filePath = Application.streamingAssetsPath + "/Levels/Homemade/";
        xml.Save(filePath + fileName);
        
        Debug.Log(xml);
        
        XDocument scenario = XDocument.Load(Application.streamingAssetsPath + "/Levels/Homemade/Scenario.xml");
        scenario.Element("scenario").Add(new XElement("level", new XAttribute("name",fileName)));
		scenario.Save(Application.streamingAssetsPath + "/Levels/Homemade/Scenario.xml");
		
		// XmlDocument doc = new XmlDocument();
		// doc.Load(filePath);
		// LevelGenerator.instance.XmlToLevel(doc);
		if (GameData.Instance == null)
		{
			GameData.Instance = UnityEngine.Object.Instantiate(prefabGameData);
			GameData.Instance.name = "GameData";
			GameObjectManager.dontDestroyOnLoadAndRebind(GameData.Instance.gameObject);
		}
		
		GameData.Instance.mode = "Homemade";
		GameData.Instance.homemadeLevelToLoad = (filePath + fileName);
		GameObjectManager.loadScene("MainScene");
	}
	
	
	public void WriteLevel()
	{
		
	}
}
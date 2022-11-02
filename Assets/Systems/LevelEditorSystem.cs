using System;
using UnityEngine;
using FYFY;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using FYFY_plugins.TriggerManager;
using UnityEngine.Tilemaps;

/// <summary>
/// Manage collision between player agents and Coins
/// </summary>
public class LevelEditorSystem : FSystem {
	private Family f_levelEditor = FamilyManager.getFamily(new AllOfComponents(typeof(LevelEditor)));

	private LevelEditor levelEditor;
	private List<string> dirs = new List<string>{"N","S","E","W"};
	private List<string> dorDirs = new List<string>{"V","H"};
    
	protected override void onStart()
    {
	    levelEditor = f_levelEditor.First().GetComponent<LevelEditor>();
    }

	public void ReadLevel()
	{
		Vector2 agentPos = Vector2.zero;
		
		XDocument xml = new XDocument();
		xml.Declaration = new XDeclaration("1.0", "utf-8", "true");
		
		XElement xmlLevel = new XElement("level");

		XElement xmlMap = new XElement("map");
		
        for (int x = levelEditor.Map.cellBounds.xMin; x < levelEditor.Map.cellBounds.xMax; x++)
        {
	        XElement xmlLine = new XElement("line");
	        for (int y = levelEditor.Map.cellBounds.yMin; y < levelEditor.Map.cellBounds.yMax; y++)
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
		                var objectName = levelEditor.Objects.GetTile(localPlace).name.Split("_");
		                string objectType = objectName[0];
		                string objectDir = objectName[1];
		                
		                switch (objectType)
		                {
			                case "Robot":
				                xmlLevel.Add(new XElement("player",
					                new XAttribute("associatedScriptName","K"), //todo
					                new XAttribute("posY",x),
					                new XAttribute("posX",y),
					                new XAttribute("direction", dirs.IndexOf(objectDir).ToString())));
				                break;

			                case "Drone":
				                xmlLevel.Add(new XElement("enemy",
					                new XAttribute("associatedScriptName","Guarde"),
					                new XAttribute("posY",x),
					                new XAttribute("posX",y),
					                new XAttribute("direction", dirs.IndexOf(objectDir).ToString()),
					                new XAttribute("range","2"),
					                new XAttribute("selfRange","False"),
					                new XAttribute("typeRange","0")));
				                break;
			                
			                case "Coin":
				                xmlLevel.Add(new XElement("coin",
					                new XAttribute("posY", x),
					                new XAttribute("posX", y)));
				                break;
			                
			                case "Console":
				                xmlLevel.Add(new XElement("console",
					                new XAttribute("state","1"), //todo
					                new XAttribute("posY",x),
					                new XAttribute("posX",y),
					                new XAttribute("direction", dirs.IndexOf(objectDir).ToString())));
				                break;
			                
			                case "Door":
				                xmlLevel.Add(new XElement("console",
					                new XAttribute("posY",x),
					                new XAttribute("posX",y),
					                new XAttribute("slotId","0"), //todo
					                new XAttribute("direction", dirs.IndexOf(objectDir).ToString())));
				                break;
				                
			                
			                
			                
		                }
		                

	                }
                }
                else
                {
	                xmlLine.Add(new XElement("cell", "-1"));
                }
	        }
	        
	        xmlMap.Add(xmlLine);

	        // { levelEditor.availablePlaces.Add(place); }
		}
        
        xmlLevel.Add(xmlMap);

        xmlLevel.Add(new XElement("player",
	        new XAttribute("associatedScriptName","Karl"), 
	        new XAttribute("posY",agentPos.x),
	        new XAttribute("posX",agentPos.y),
			new XAttribute("direction","0")));
        
        // xmlLevel.Add(
	       //  XDocument.Parse("<dialogs><dialog text=\"23 août 2041...&#xA;&#xA;Vous avez un message...\"/></dialogs>"));
        // xmlLevel.Add(
	       //  XDocument.Parse("<blockLimits><blockLimit blockType=\"Forward\" limit=\"1\" /></blockLimits>"));
        // xmlLevel.Add(
		      //   XDocument.Parse("<player associatedScriptName=\"Karl\" posY=\"2\" posX=\"1\" direction=\"0\" /><script name='Karl' editMode='0' /><score twoStars='0' threeStars='10500'/>"));

        xml.Add(xmlLevel);
        
        xml.Save("/StreamingAssets/Levels/Homemade/" + DateTime.Now.ToString("g") + "out.xml");
        
        Debug.Log(xml);
        Debug.Log("done");
	}
	
	
	public void WriteLevel()
	{
		
	}
}
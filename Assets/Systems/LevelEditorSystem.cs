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
    private Tilemap tileMap;
    
        protected override void onStart()
    {
	    levelEditor = f_levelEditor.First().GetComponent<LevelEditor>();
        tileMap = levelEditor.transform.GetChild(0).GetComponent<Tilemap>();
    }

	public void ReadLevel()
	{
		Vector2 agentPos = Vector2.zero;
		
		XDocument xml = new XDocument();

		XElement xmlLevel = new XElement("level");

		XElement xmlMap = new XElement("map");
		
        for (int n = tileMap.cellBounds.xMin; n < tileMap.cellBounds.xMax; n++)
        {
	        XElement xmlLine = new XElement("line");
	        for (int p = tileMap.cellBounds.yMin; p < tileMap.cellBounds.yMax; p++)
            {
                Vector3Int localPlace = (new Vector3Int(n, p, (int)tileMap.transform.position.y));
                tileMap.SetTileFlags(localPlace, TileFlags.None);
                Vector3 place = tileMap.CellToWorld(localPlace);
                if (tileMap.HasTile(localPlace))
                {
	                Color tileColor = tileMap.GetColor(localPlace);
	                
	                if(ColorEquals(tileColor,levelEditor.Wall.color))
		                xmlLine.Add(new XElement("cell",new XAttribute("value", "1")));
	                else if(ColorEquals(tileColor,levelEditor.Road.color))
		                xmlLine.Add(new XElement("cell",new XAttribute("value", "0")));
	                else if (ColorEquals(tileColor, levelEditor.Start.color))
	                {
		                xmlLine.Add(new XElement("cell",new XAttribute("value", "2")));
		                agentPos = new Vector2(n-tileMap.cellBounds.xMin,p-tileMap.cellBounds.yMin);
	                }
	                else if(ColorEquals(tileColor,levelEditor.Goal.color))
		                xmlLine.Add(new XElement("cell",new XAttribute("value", "3")));
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
        
        xml.Save("out.xml");
        
        Debug.Log(xml);
        Debug.Log("done");
	}
        
        // var tilePos = tileMap.WorldToCell(downFace.transform.position);
        

	public static bool ColorEquals(Color a, Color b)
    {
        var eps = 0.1f;
        /*Debug.Log(a.ToString());
        Debug.Log(b.ToString());
        Debug.Log(a.r + " , " + b.r + " , " + a.g + " , " + b.g + " , " + a.b + " , " + b.b + " , " + a.a + " , " + b.a);*/
        return (Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) + Mathf.Abs(a.a - b.a)) < eps;
    }
	
	public void WriteLevel()
	{
		
	}
}
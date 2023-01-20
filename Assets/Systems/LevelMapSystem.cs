using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using FYFY;
using UnityEditor.Tilemaps;
using UnityEngine.Tilemaps;

/// <summary>
/// Manage dialogs at the begining of the level
/// </summary>
public class LevelMapSystem : FSystem
{
	private Family f_LM = FamilyManager.getFamily(new AllOfComponents(typeof(LevelMap)));
	private LevelMap LM;

	private List<string> LevelList;
	private Dictionary<Vector3Int, string> LevelNames;

	protected override void onStart()
	{
		LM = f_LM.First().GetComponent<LevelMap>();
		LevelNames = new Dictionary<Vector3Int, string>();

		ReadLevels();

		LevelList = (List<string>)GameData.levelList[GameData.mode];
		
		LoadLevels();
		
		LM.CharacPos = new Vector3Int(0, 0, 0);
		LM.CharacMap.ClearAllTiles();
		LM.CharacMap.SetTile(LM.CharacPos, LM.Charac);

		LM.Map.SetTile(Vector3Int.right, LM.Road);

		// tmp = LM.TM.GetTileFlags();

	}
	
	protected override async void onProcess(int familiesUpdateCount)
	{
		if (Input.GetMouseButton(0))
		{
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int tilePos = LM.Map.WorldToCell(mousePos);
			TileBase tile = LM.Map.GetTile(tilePos);

			if (tile != null && LevelList.Contains(tile.name))
			{
				var tmp = LevelNames[tilePos].Split('/', '.');
				LM.CharacPos = tilePos;
				LM.CharacMap.ClearAllTiles();
				LM.CharacMap.SetTile(LM.CharacPos, LM.Charac);
				LM.LevelName.text = tmp[tmp.Length-2];
				LM.StartLevel.onClick.AddListener(delegate { launchLevel(GameData.mode, LevelList.IndexOf(LevelNames[tilePos])); });
				await CameraTranstion(new Vector3(mousePos.x, mousePos.y, Camera.main.transform.position.z));
				// Camera.main.transform.position = new Vector3(mousePos.x, mousePos.y, Camera.main.transform.position.z);
			}

		}
	}
	
	async Task CameraTranstion(Vector3 pos)
	{
		await Task.Delay(100);
		
		while ((Camera.main.transform.position-pos).magnitude > 0.5f)
		{
			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, pos, 0.1f);
			await Task.Delay(50);
		}
	}

	private void ReadLevels()
	{
		GameData.levelList = new Hashtable();
		string levelsPath;
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			//paramFunction();
			GameData.levelList["Campagne infiltration"] = new List<string>();
			for (int i = 1; i <= 20; i++)
				((List<string>)GameData.levelList["Campagne infiltration"]).Add(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
					Path.DirectorySeparatorChar + "Campagne infiltration" + Path.DirectorySeparatorChar +"Niveau" + i + ".xml");
			// Hide Competence button
			ParamCompetenceSystem.instance.Pause = true;
		}
		else
		{
			levelsPath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels";
			List<string> levels;
			foreach (string directory in Directory.GetDirectories(levelsPath))
			{
				levels = readScenario(directory);
				if (levels != null)
					GameData.levelList[Path.GetFileName(directory)] = levels; //key = directory name
			}
		}
	}
	private List<string> readScenario(string repositoryPath) {
		if (File.Exists(repositoryPath + Path.DirectorySeparatorChar + "Scenario.xml")) {
			List<string> levelList = new List<string>();
			XmlDocument doc = new XmlDocument();
			doc.Load(repositoryPath + Path.DirectorySeparatorChar + "Scenario.xml");
			XmlNode root = doc.ChildNodes[1]; //root = <scenario/>
			foreach (XmlNode child in root.ChildNodes) {
				if (child.Name.Equals("level")) {
					levelList.Add(repositoryPath + Path.DirectorySeparatorChar + (child.Attributes.GetNamedItem("name").Value));
				}
			}
			return levelList;
		}
		return null;
	}
	
	public void launchLevel(string levelDirectory, int level) {
		GameData.levelToLoad = (levelDirectory, level);
		GameObjectManager.loadScene("MainScene");
	}

	private void LoadLevels()
	{
		int x = 0;
		foreach (var level in LevelList)
		{
			Vector3Int pos = new Vector3Int(x, 0, 0);
			LM.Map.SetTile(pos, LM.Base);
			LevelNames.Add(pos, level);
			LM.Map.SetTile(new Vector3Int(x+1,0,0), LM.Road);
			x += 2;
		}
	}
}
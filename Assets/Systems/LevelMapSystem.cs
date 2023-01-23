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
	private List<Tile> Scores;
	
	private Task cameraMoving;
	private Vector3 toPos;

	protected override void onStart()
	{
		Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
			
		LM = f_LM.First().GetComponent<LevelMap>();
		Scores = new List<Tile>() { LM.Undone, LM.Done, LM.Code, LM.All, LM.Exec };
		LevelNames = new Dictionary<Vector3Int, string>();

		GameStateManager.LoadGD();
		if (Global.GD == null || Global.GD.levelList == null)
		{
			Global.GD = new GameData();
			ReadLevels();
		}

		LevelList = (List<string>)Global.GD.levelList[Global.GD.mode];
		
		LoadLevels();
		
		string level = ((List<string>)Global.GD.levelList[Global.GD.mode])[Global.GD.levelToLoad.Item2];
		if (LevelList.Contains(level))
			LM.CharacPos = LevelNames.FirstOrDefault(x => x.Value == level).Key;
		else
			LM.CharacPos = Vector3Int.zero;

		Vector3 mousePos = LM.CharacMap.CellToWorld(LM.CharacPos);
		Camera.main.transform.position = new Vector3(mousePos.x, mousePos.y, Camera.main.transform.position.z);
		LoadUI(LM.CharacPos);
	}
	
	protected override async void onProcess(int familiesUpdateCount)
	{
		if (Input.anyKeyDown)
		{
			Vector3Int tilePos = Vector3Int.up;
			if (Input.GetMouseButton(0))
			{
				Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				tilePos = LM.Map.WorldToCell(mousePos);
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow))
				tilePos = Left(LM.CharacPos);
			else if (Input.GetKeyDown(KeyCode.RightArrow))
				tilePos = Right(LM.CharacPos);
				
			if (tilePos != Vector3Int.up && LM.Map.HasTile(tilePos) && LevelNames.ContainsKey(tilePos)){
				LM.CharacPos = tilePos;
				LoadUI(LM.CharacPos);
			}
		}
	}
	
	private Vector3Int Right(Vector3Int v)
	{
		return v + 2*Vector3Int.right;
	}
	
	private Vector3Int Left(Vector3Int v)
	{
		return v + 2*Vector3Int.left;
	}
	
	private Vector3Int Up(Vector3Int v)
	{
		return v + Vector3Int.up + 2*Vector3Int.right;
	}
	
	private Vector3Int Down(Vector3Int v)
	{
		return v + Vector3Int.down + 2*Vector3Int.right;
	}

	private async void LoadUI(Vector3Int tilePos)
	{
		Vector2 mousePos = LM.CharacMap.CellToWorld(tilePos);
		LM.CharacPos = tilePos;
		LM.CharacMap.ClearAllTiles();
		LM.CharacMap.SetTile(LM.CharacPos, LM.Charac);
		LM.LevelName.text = LevelNames[tilePos].Split('/', '.')[^2];
		LM.StartLevel.onClick.RemoveAllListeners();
		LM.StartLevel.onClick.AddListener(delegate { launchLevel(Global.GD.mode, LevelList.IndexOf(LevelNames[tilePos])); });
		toPos = new Vector3(mousePos.x, mousePos.y, Camera.main.transform.position.z);
		cameraMoving = CameraTranstion();
	}
	
	async Task CameraTranstion()
	{
		if (cameraMoving != null)
		{
			await cameraMoving;
		}
		await Task.Delay(100);
		
		while ((Camera.main.transform.position-toPos).magnitude > 0.7f)
		{
			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, toPos, 0.1f);
			await Task.Delay(10);
		}
	}

	public static void ReadLevels()
	{
		Global.GD.levelList = new Hashtable();
		string levelsPath;
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			//paramFunction();
			Global.GD.levelList["Campagne infiltration"] = new List<string>();
			for (int i = 1; i <= 20; i++)
				((List<string>)Global.GD.levelList["Campagne infiltration"]).Add(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
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
					Global.GD.levelList[Path.GetFileName(directory)] = levels; //key = directory name
			}
		}
	}
	public static List<string> readScenario(string repositoryPath) {
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
		Global.GD.levelToLoad = (levelDirectory, level);
		GameObjectManager.loadScene("MainScene");
	}

	private void LoadLevels()
	{
		int x = 0;
		foreach (var level in LevelList)
		{
			int scoredStars = PlayerPrefs.GetInt(Global.GD.mode + Path.DirectorySeparatorChar + LevelList.IndexOf(level) + Global.GD.scoreKey, 0); //0 star by default
			LM.Stars.SetTile(new Vector3Int(x, 0, 0), Scores[scoredStars]);

			Vector3Int pos = new Vector3Int(x, 0, 0);
			LM.Map.SetTile(pos, LM.Base);
			LevelNames.Add(pos, level);
			LM.Map.SetTile(new Vector3Int(x+1,0,0), LM.Road);
			x += 2;
		}
	}
	
	private void ConstructRoad()
	{
		int x = 0;
		foreach (var level in LevelList)
		{
			int scoredStars = PlayerPrefs.GetInt(Global.GD.mode + Path.DirectorySeparatorChar + LevelList.IndexOf(level) + Global.GD.scoreKey, 0); //0 star by default
			LM.Stars.SetTile(new Vector3Int(x, 0, 0), Scores[scoredStars]);

			Vector3Int pos = new Vector3Int(x, 0, 0);
			LM.Map.SetTile(pos, LM.Base);
			LevelNames.Add(pos, level);
			LM.Map.SetTile(new Vector3Int(x+1,0,0), LM.Road);
			x += 2;
		}
	}

	private void RoadFoward()
	{
		
	}

	private void RoadSplit()
	{
		
	}
	
	private void RoadMerge()
	{
		
	}
	
}
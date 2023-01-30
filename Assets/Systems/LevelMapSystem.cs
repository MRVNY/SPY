using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using FYFY;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Manage dialogs at the begining of the level
/// </summary>
public class LevelMapSystem : FSystem
{
	private Family f_LM = FamilyManager.getFamily(new AllOfComponents(typeof(LevelMap)));
	private LevelMap LM;

	private Dictionary<Vector3Int, Level> LevelDict;
	private List<Tile> Scores;
	
	private Task cameraMoving;
	private Vector3 toPos;
	
	public static LevelMapSystem Instance;

	protected async override void onStart()
	{
		if(Instance == null) Instance = this;
		
		Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
			
		LM = f_LM.First().GetComponent<LevelMap>();
		Scores = new List<Tile>() { LM.Undone, LM.Done, LM.Code, LM.Exec, LM.All };
		LevelDict = new Dictionary<Vector3Int, Level>();

		if(Global.GD == null) GameStateManager.LoadGD();
		if (Global.GD == null || Global.GD.Tree == null)
		{
			Global.GD = new GameData();
			Global.GD.path = Application.streamingAssetsPath + "/Levels/";
			await TreeManager.ConstructTree();
			Global.GD.Tree.introLevels.First().active = true;
		}

		//LoadLevels();
		ConstructRoad(Global.GD.Tree, Vector3Int.zero, 0);
		
		if (LevelDict.ContainsValue(Global.GD.level))
			LM.CharacPos = LevelDict.FirstOrDefault(x => x.Value == Global.GD.level).Key;
		else
			LM.CharacPos = Vector3Int.zero;

		Vector3 mousePos = LM.CharacMap.CellToWorld(LM.CharacPos);
		Camera.main.transform.position = new Vector3(mousePos.x, mousePos.y, Camera.main.transform.position.z);
		LoadUI(LM.CharacPos);

		if(Global.GD.ending==2) await Ending2();

		if (Global.GD.player == "Student") Global.GD.convoNode = "askName";
		else if (Global.GD.level != null && Global.GD.level.node != null)
		{
			if (Global.GD.level.score>0 && Global.GD.level.next[0].score==0 && Global.GD.level.node.trainingLevels.First() == Global.GD.level)
			{
				Global.GD.convoNode = "askDifficulty";
			}
			else Global.GD.convoNode = Global.GD.level.name + ".0";	
		}
	}
	
	protected override void onProcess(int familiesUpdateCount)
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
				
			if (tilePos != Vector3Int.up && LM.Map.HasTile(tilePos) && LevelDict.ContainsKey(tilePos) && LM.Map.GetTile(tilePos) != LM.LockedBase){
				LM.CharacPos = tilePos;
				LoadUI(LM.CharacPos);
			}
		}
	}

	private void LoadUI(Vector3Int tilePos)
	{
		Vector2 mousePos = LM.CharacMap.CellToWorld(tilePos);
		LM.CharacPos = tilePos;
		LM.CharacMap.ClearAllTiles();
		LM.CharacMap.SetTile(LM.CharacPos, LM.Charac);
		LM.LevelName.text = LevelDict[tilePos].name;
		LM.StartLevel.onClick.RemoveAllListeners();
		LM.StartLevel.onClick.AddListener(delegate { launchLevel(Global.GD.mode, LevelDict[tilePos]); });
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
	public void launchLevel(string mode, Level level) {
		Global.GD.mode = mode;
		Global.GD.level = level;
		SendStatements.instance.SendLevel(level.name);
		GameStateManager.SaveGD();
		GameObjectManager.loadScene("GameScene");
	}

	public void toTiltle()
	{
		GameStateManager.SaveGD();
		GameObjectManager.loadScene("TitleScreen");
	}

	private void ConstructRoad(Node node, Vector3Int pos, int split)
	{
		foreach (var lvl in node.introLevels)
			pos = RoadFoward(pos, lvl);
		foreach (var lvl in node.trainingLevels)
			pos = RoadFoward(pos, lvl);
		foreach (var lvl in node.outroLevels)
			pos = RoadFoward(pos, lvl);

		switch (node.nextNodes.Count)
		{
			case 1:
				if (pos.x > 0) pos = RoadMerge(pos, split);
				if(LM.Map.GetTile(pos)!=LM.Base) ConstructRoad(node.nextNodes[0], pos, 0);
				break;
			case 2:
				(Vector3Int,Vector3Int) upAndDown = RoadSplit(pos);
				ConstructRoad(node.nextNodes[0], upAndDown.Item1, 1);
				ConstructRoad(node.nextNodes[1], upAndDown.Item2, -1);
				break;
			default: break;
		}
	}
	private void LoadLevels()
	{
		int x = 0;
		foreach (var level in LevelDict.Values.ToList())
		{
			int scoredStars = PlayerPrefs.GetInt(Global.GD.mode + Path.DirectorySeparatorChar + LevelDict.Values.ToList().IndexOf(level) + Global.GD.scoreKey, 0); //0 star by default
			LM.Stars.SetTile(new Vector3Int(x, 0, 0), Scores[scoredStars]);

			Vector3Int pos = new Vector3Int(x, 0, 0);
			LM.Map.SetTile(pos, LM.Base);
			LevelDict.Add(pos, level);
			LM.Map.SetTile(new Vector3Int(x+1,0,0), LM.Road);
			x += 2;
		}
	}
	
	private Vector3Int Right(Vector3Int v) { return v + 2*Vector3Int.right; }
	private Vector3Int Left(Vector3Int v) { return v + 2*Vector3Int.left; }
	private Vector3Int Up(Vector3Int v) { return v + Vector3Int.up + 2*Vector3Int.right; }
	private Vector3Int Down(Vector3Int v) { return v + Vector3Int.down + 2*Vector3Int.right; }
	

	private Vector3Int RoadFoward(Vector3Int pos, Level lvl)
	{
		if (!LevelDict.ContainsKey(pos+2*Vector3Int.right))
		{
			if (lvl != lvl.node.introLevels.First())
			{
				pos += Vector3Int.right;
				LM.Map.SetTile(pos, LM.Road);
				pos += Vector3Int.right;
			}
			
			if (lvl == lvl.node.outroLevels.Last() && (lvl.node.introLevels.First().score > 0 || lvl.node == Global.GD.Tree))
			{
				LM.Map.SetTile(pos, LM.Castle);
				if(lvl.score > 0)
					foreach (var next in lvl.next)
						next.active = true;
			}
			else
			{
				if (lvl.active || (int)lvl.score > 0 || (Global.GD.level != null &&
				                           (Global.GD.level == lvl || Global.GD.level.next.Contains(lvl))))
				{
					LM.Map.SetTile(pos, LM.Base);
					if ((int)lvl.score > 0)
						foreach (var next in lvl.next)
							next.active = true;
				}
				else LM.Map.SetTile(pos, LM.LockedBase);
			}

			LM.Stars.SetTile(pos, Scores[(int)lvl.score]);
			LevelDict.Add(pos, lvl);
		}

		return pos;
	}

	private (Vector3Int,Vector3Int) RoadSplit(Vector3Int pos)
	{
		Vector3Int up;
		Vector3Int down;
		if (pos.y == 0)
		{
			pos += Vector3Int.right;
			LM.Map.SetTile(pos, LM.Split);
			
			up = pos + Vector3Int.up;
			LM.Map.SetTile(up, LM.UpRight);
			up += Vector3Int.right + Vector3Int.up;
			
			down = pos + Vector3Int.down;
			LM.Map.SetTile(down, LM.DownRight);
			down += Vector3Int.right + Vector3Int.down;
		}
		else
		{
			pos += Vector3Int.right;
			LM.Map.SetTile(pos, LM.Split);
			
			up = pos + Vector3Int.up;
			down = pos + Vector3Int.down;
		}
		return (up,down);
	}
	
	private Vector3Int RoadMerge(Vector3Int pos, int split)
	{
		if (pos.y - 2*split == 0) //if it's the first split
		{
			pos += split * Vector3Int.down;
			if(split>0) LM.Map.SetTile(pos, LM.DownRight);
			else LM.Map.SetTile(pos, LM.UpRight);
			
			pos += Vector3Int.right + split * Vector3Int.down;
			LM.Map.SetTile(pos, LM.Merge);
			pos += Vector3Int.right;
		}
		else
		{
			pos += Vector3Int.right + split * Vector3Int.down;
			LM.Map.SetTile(pos, LM.Merge);
			pos += Vector3Int.right;
		}
		return pos;
	}

	public async Task Ending2()
	{
		LM.Map.CompressBounds();
		List<Vector3Int>[] toRemove = new List<Vector3Int>[LM.Map.cellBounds.size.x];
		foreach (var pos in LevelDict.Keys.ToList())
		{
			if(toRemove[pos.x]==null){ toRemove[pos.x] = new List<Vector3Int>();}
			toRemove[pos.x].Add(pos);
		}
		bool charaRemoved = false;
		for(int i = toRemove.Length-1; i>=0; i--)
		{
			if(!charaRemoved && toRemove[i]!=null)
			{
				LoadUI(toRemove[i][^1]);
				await Task.Delay(2000);
				LM.CharacMap.ClearAllTiles();
				LM.LevelName.text = "";
				LM.StartLevel.gameObject.SetActive(false);
				charaRemoved = true;
			}
			
			if(toRemove[i]!=null)
			{
				Vector2 mousePos = LM.CharacMap.CellToWorld(new Vector3Int(toRemove[i][0].x, 0, 0));
				toPos = new Vector3(mousePos.x, 0, Camera.main.transform.position.z);
				cameraMoving = CameraTranstion();
				await Task.Delay(1000);
				
				foreach (var pos in toRemove[i])
				{
					LM.Map.SetTile(pos, LM.LockedBase);
					LM.Stars.SetTile(pos, LM.Undone);
				}
				await Task.Delay(1000);
			}
		}

		Global.GD.ending = -2;
		await Task.Delay(1000);
		SceneManager.LoadScene("TitleScreen");
	}
}
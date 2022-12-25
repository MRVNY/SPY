using System.Threading.Tasks;
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


	protected override void onStart()
	{
		LM = f_LM.First().GetComponent<LevelMap>();

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

			if (tile != null && tile.name == "Base")
			{
				LM.CharacPos = tilePos;
				LM.CharacMap.ClearAllTiles();
				LM.CharacMap.SetTile(LM.CharacPos, LM.Charac);
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
}
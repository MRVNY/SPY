using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEditor.Tilemaps;
using UnityEngine.Accessibility;

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
		Grid grid = LM.GetComponent<Grid>();

		GridSelection tmp;

		Debug.Log(LM.Map.GetTile(Vector3Int.zero).name);
		LM.Map.SetTile(Vector3Int.right, LM.Road);

		// tmp = LM.TM.GetTileFlags();

	}
	
	protected override void onProcess(int familiesUpdateCount)
	{
		//Debug.Log("hi");
	}
}
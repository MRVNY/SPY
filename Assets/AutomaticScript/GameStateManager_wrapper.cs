using UnityEngine;
using FYFY;

public class GameStateManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject playButtonAmount;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "playButtonAmount", playButtonAmount);
	}

	public void SaveGD()
	{
		MainLoop.callAppropriateSystemMethod (system, "SaveGD", null);
	}

	public void LoadGD()
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadGD", null);
	}

	public void LoadState()
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadState", null);
	}

	public void DeleteAllSaveFiles()
	{
		MainLoop.callAppropriateSystemMethod (system, "DeleteAllSaveFiles", null);
	}

}

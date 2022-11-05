using UnityEngine;
using FYFY;

public class LevelEditorSystem_wrapper : BaseWrapper
{
	public GameData prefabGameData;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "prefabGameData", prefabGameData);
	}

	public void ReadLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "ReadLevel", null);
	}

	public void WriteLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "WriteLevel", null);
	}

}

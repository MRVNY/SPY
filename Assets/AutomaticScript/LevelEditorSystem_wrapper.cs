using UnityEngine;
using FYFY;

public class LevelEditorSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void ReadLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "ReadLevel", null);
	}

	public void ExportXML()
	{
		MainLoop.callAppropriateSystemMethod (system, "ExportXML", null);
	}

	public void LoadLevel(System.String levelName)
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadLevel", levelName);
	}

	public void WriteLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "WriteLevel", null);
	}

}

using UnityEngine;
using FYFY;

public class ScriptManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void TranslateScript()
	{
		MainLoop.callAppropriateSystemMethod (system, "TranslateScript", null);
	}

	public void SaveScript()
	{
		MainLoop.callAppropriateSystemMethod (system, "SaveScript", null);
	}

	public void DeleteLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "DeleteLevel", null);
	}

}

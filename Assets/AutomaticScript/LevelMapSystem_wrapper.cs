using UnityEngine;
using FYFY;

public class LevelMapSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void ReadLevels()
	{
		MainLoop.callAppropriateSystemMethod (system, "ReadLevels", null);
	}

	public void launchLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevel", null);
	}

}

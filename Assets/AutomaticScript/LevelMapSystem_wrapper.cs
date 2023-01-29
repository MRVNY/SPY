using UnityEngine;
using FYFY;

public class LevelMapSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void launchLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevel", null);
	}

	public void toTiltle()
	{
		MainLoop.callAppropriateSystemMethod (system, "toTiltle", null);
	}

}

using UnityEngine;
using FYFY;

public class DoorAndConsoleManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject doorPathPrefab;
	public UnityEngine.Color pathOn;
	public UnityEngine.Color pathOff;
	public UnityEngine.GameObject Level;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "doorPathPrefab", doorPathPrefab);
		MainLoop.initAppropriateSystemField (system, "pathOn", pathOn);
		MainLoop.initAppropriateSystemField (system, "pathOff", pathOff);
		MainLoop.initAppropriateSystemField (system, "Level", Level);
	}

}

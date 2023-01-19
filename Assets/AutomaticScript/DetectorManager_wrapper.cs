using UnityEngine;
using FYFY;

public class DetectorManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject Level;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "Level", Level);
	}

	public void updateDetectors()
	{
		MainLoop.callAppropriateSystemMethod (system, "updateDetectors", null);
	}

}

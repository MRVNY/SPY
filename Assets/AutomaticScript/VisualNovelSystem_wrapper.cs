using UnityEngine;
using FYFY;

public class VisualNovelSystem_wrapper : BaseWrapper
{
	public System.String node;
	public UnityEngine.UI.Button skipButton;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "node", node);
		MainLoop.initAppropriateSystemField (system, "skipButton", skipButton);
	}

	public void Skip()
	{
		MainLoop.callAppropriateSystemMethod (system, "Skip", null);
	}

}
using UnityEngine;
using FYFY;

public class VisualNovelSystem_wrapper : BaseWrapper
{
	public UnityEngine.UI.Button skipButton;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "skipButton", skipButton);
	}

	public void setVN()
	{
		MainLoop.callAppropriateSystemMethod (system, "setVN", null);
	}

	public void Next()
	{
		MainLoop.callAppropriateSystemMethod (system, "Next", null);
	}

	public void endLevelConvo()
	{
		MainLoop.callAppropriateSystemMethod (system, "endLevelConvo", null);
	}

}

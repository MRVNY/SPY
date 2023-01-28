using UnityEngine;
using FYFY;

public class EndGameManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject playButtonAmount;
	public UnityEngine.GameObject endPanel;
	public UnityEngine.GameObject Restart;
	public UnityEngine.GameObject Rewind;
	public UnityEngine.GameObject Menu;
	public UnityEngine.GameObject Next;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "playButtonAmount", playButtonAmount);
		MainLoop.initAppropriateSystemField (system, "endPanel", endPanel);
		MainLoop.initAppropriateSystemField (system, "Restart", Restart);
		MainLoop.initAppropriateSystemField (system, "Rewind", Rewind);
		MainLoop.initAppropriateSystemField (system, "Menu", Menu);
		MainLoop.initAppropriateSystemField (system, "Next", Next);
	}

	public void cancelEnd()
	{
		MainLoop.callAppropriateSystemMethod (system, "cancelEnd", null);
	}

}

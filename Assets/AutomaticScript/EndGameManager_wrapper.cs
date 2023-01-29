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
	public UnityEngine.GameObject stars;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "playButtonAmount", playButtonAmount);
		MainLoop.initAppropriateSystemField (system, "endPanel", endPanel);
		MainLoop.initAppropriateSystemField (system, "Restart", Restart);
		MainLoop.initAppropriateSystemField (system, "Rewind", Rewind);
		MainLoop.initAppropriateSystemField (system, "Menu", Menu);
		MainLoop.initAppropriateSystemField (system, "Next", Next);
		MainLoop.initAppropriateSystemField (system, "stars", stars);
	}

	public void cancelEnd()
	{
		MainLoop.callAppropriateSystemMethod (system, "cancelEnd", null);
	}

}

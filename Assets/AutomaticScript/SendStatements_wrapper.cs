using UnityEngine;
using FYFY;

public class SendStatements_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void initGBLXAPI()
	{
		MainLoop.callAppropriateSystemMethod (system, "initGBLXAPI", null);
	}

	public void testSendStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "testSendStatement", null);
	}

	public void SendLevel(System.String lv)
	{
		MainLoop.callAppropriateSystemMethod (system, "SendLevel", lv);
	}

	public void WinLevel(System.Int32 score)
	{
		MainLoop.callAppropriateSystemMethod (system, "WinLevel", score);
	}

	public void SendRestart()
	{
		MainLoop.callAppropriateSystemMethod (system, "SendRestart", null);
	}

	public void SendBackMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "SendBackMenu", null);
	}

	public void SendActions(System.String actions)
	{
		MainLoop.callAppropriateSystemMethod (system, "SendActions", actions);
	}

	public void SendBeginGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "SendBeginGame", null);
	}

	public void SendQuitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "SendQuitGame", null);
	}

}

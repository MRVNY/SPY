using UnityEngine;
using FYFY;

public class TitleScreenSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject prefabFuncData;
	public UnityEngine.GameObject mainMenu;
	public UnityEngine.GameObject campagneMenu;
	public UnityEngine.GameObject compLevelButton;
	public UnityEngine.GameObject cList;
	public System.String pathFileParamFunct;
	public System.String pathFileParamRequiermentLibrary;
	public UnityEngine.GameObject settingsPanel;
	public UnityEngine.GameObject menuPanel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "prefabFuncData", prefabFuncData);
		MainLoop.initAppropriateSystemField (system, "mainMenu", mainMenu);
		MainLoop.initAppropriateSystemField (system, "campagneMenu", campagneMenu);
		MainLoop.initAppropriateSystemField (system, "compLevelButton", compLevelButton);
		MainLoop.initAppropriateSystemField (system, "cList", cList);
		MainLoop.initAppropriateSystemField (system, "pathFileParamFunct", pathFileParamFunct);
		MainLoop.initAppropriateSystemField (system, "pathFileParamRequiermentLibrary", pathFileParamRequiermentLibrary);
		MainLoop.initAppropriateSystemField (system, "settingsPanel", settingsPanel);
		MainLoop.initAppropriateSystemField (system, "menuPanel", menuPanel);
	}

	public void showCampagneMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "showCampagneMenu", null);
	}

	public void launchLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevel", null);
	}

	public void launchLevelMap()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevelMap", null);
	}

	public void backFromCampagneMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "backFromCampagneMenu", null);
	}

	public void changeLanguage(System.Int32 i)
	{
		MainLoop.callAppropriateSystemMethod (system, "changeLanguage", i);
	}

	public void clearSaves()
	{
		MainLoop.callAppropriateSystemMethod (system, "clearSaves", null);
	}

	public void openSettings()
	{
		MainLoop.callAppropriateSystemMethod (system, "openSettings", null);
	}

	public void openMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "openMenu", null);
	}

	public void quitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "quitGame", null);
	}

}

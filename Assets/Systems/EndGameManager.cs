using UnityEngine;
using FYFY;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This system check if the end of the level is reached and display end panel accordingly
/// </summary>
public class EndGameManager : FSystem {

	public static EndGameManager instance;

	private Family f_requireEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));

	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
    private Family f_newCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));
	private Family f_exit = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(AudioSource)), new AnyOfTags("Exit"));

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	
	public GameObject playButtonAmount;
	public GameObject endPanel;

    public GameObject Restart;
    public GameObject Rewind;
    public GameObject Menu;
    public GameObject Next;

    private Color gold = new Color(228,255,0,255);
    private Color gray = new Color(137,137,137,255);
    private Color blue = new Color(0,255,255,255);
    private Color red = new Color(255,0,0,255);

    public GameObject stars;

	public EndGameManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObjectManager.setGameObjectState(endPanel.transform.parent.gameObject, false);

		GameStateManager.SaveGD();

		f_requireEndPanel.addEntryCallback(displayEndPanel);

		// each time a current action is removed, we check if the level is over
		f_newCurrentAction.addExitCallback(delegate {
			MainLoop.instance.StartCoroutine(delayCheckEnd());
		});

		f_playingMode.addExitCallback(delegate {
			MainLoop.instance.StartCoroutine(delayNoMoreAttemptDetection());
		});
	}

	private IEnumerator delayCheckEnd()
	{
		// wait 2 frames before checking if a new currentAction was produced
		yield return null; // this frame the currentAction is removed
		yield return null; // this frame a probably new current action is created
						   // Now, families are informed if new current action was produced, we can check if no currentAction exists on players and if all players are on the end of the level
		if (!playerHasCurrentAction())
		{
			int nbEnd = 0;
			bool endDetected = false;
			// parse all exits
			for (int e = 0; e < f_exit.Count && !endDetected; e++)
			{
				GameObject exit = f_exit.getAt(e);
				// parse all players
				for (int p = 0; p < f_player.Count && !endDetected; p++)
				{
					GameObject player = f_player.getAt(p);
					// check if positions are equals
					if (player.GetComponent<Position>().x == exit.GetComponent<Position>().x && player.GetComponent<Position>().y == exit.GetComponent<Position>().y)
					{
						nbEnd++;
						// if all players reached end position
						if (nbEnd >= f_exit.Count)
							// trigger end
							GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.Win });
					}
				}
			}
		}
	}

	private bool playerHasCurrentAction()
	{
		foreach (GameObject go in f_newCurrentAction)
		{
			if (go.GetComponent<CurrentAction>().agent.CompareTag("Player"))
				return true;
		}
		return false;
	}

	// Display panel with appropriate content depending on end
	private void displayEndPanel(GameObject unused)
	{
		// display end panel (we need immediate enabling)
		endPanel.transform.parent.gameObject.SetActive(true);
		// Get the first end that occurs
		if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Detected)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Vous avez été repéré !";
			GameObjectManager.setGameObjectState(Restart, true);
			GameObjectManager.setGameObjectState(Rewind, true);
			GameObjectManager.setGameObjectState(Menu, true);
			GameObjectManager.setGameObjectState(Next, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Win)
		{
			int score = (10000 / (Global.GD.totalActionBlocUsed + 1) + 5000 / (Global.GD.totalStep + 1) + 6000 / (Global.GD.totalExecute + 1) + 5000 * Global.GD.totalCoin);
			SendStatements.instance.WinLevel(score);
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, true);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text =
				"Code length" + Global.GD.totalActionBlocUsed + "/" + Global.GD.level.bestCode
				+ "\nExecution length" + Global.GD.totalStep + "/" + Global.GD.level.bestExec;
			setScoreStars(score, verticalCanvas.Find("ScoreCanvas"));

			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/VictorySound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = false;
			endPanel.GetComponent<AudioSource>().Play();
			GameObjectManager.setGameObjectState(Restart, true);
			GameObjectManager.setGameObjectState(Rewind, false);
			GameObjectManager.setGameObjectState(Menu, true);
			GameObjectManager.setGameObjectState(Next, true);
			//Check if next level exists in campaign
			if (Global.GD.level.next != null)
			{
				GameObjectManager.setGameObjectState(Next, false);
			}
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.BadCondition)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Une condition est mal remplie !";
			GameObjectManager.setGameObjectState(Restart, false);
			GameObjectManager.setGameObjectState(Rewind, true);
			GameObjectManager.setGameObjectState(Menu, false);
			GameObjectManager.setGameObjectState(Next, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		} else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.NoMoreAttempt)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Vous n'avez pas réussi à atteindre le téléporteur et vous n'avez plus d'exécution disponible.\nEssayez de résoudre ce niveau en moins de coup !";
			GameObjectManager.setGameObjectState(Restart, true);
			GameObjectManager.setGameObjectState(Rewind, false);
			GameObjectManager.setGameObjectState(Menu, true);
			GameObjectManager.setGameObjectState(Next, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.NoAction)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Aucune action ne peut être exécutée !";
			GameObjectManager.setGameObjectState(Restart, false);
			GameObjectManager.setGameObjectState(Rewind, true);
			GameObjectManager.setGameObjectState(Menu, false);
			GameObjectManager.setGameObjectState(Next, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.InfiniteLoop)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "ATTENTION, boucle infinie détectée...\nRisque de surchauffe du processeur du robot, interuption du programme d'urgence !";
			GameObjectManager.setGameObjectState(Restart, false);
			GameObjectManager.setGameObjectState(Rewind, true);
			GameObjectManager.setGameObjectState(Menu, false);
			GameObjectManager.setGameObjectState(Next, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
	}

	// Gére le nombre d'étoile à afficher selon le score obtenue
	private async void setScoreStars(int score, Transform scoreCanvas)
	{
		bool bestCode = false;
		bool bestExec = false;
		Image[] starList = stars.GetComponentsInChildren<Image>();
		starList[0].color = gold;
		if (Global.GD.totalActionBlocUsed <= Global.GD.level.bestCode)
		{
			bestCode = true;
			starList[1].color = red;
		}

		if (Global.GD.totalStep <= Global.GD.level.bestExec)
		{
			bestExec = true;
			starList[2].color = blue;
		}

		if (!bestCode && !bestExec)
			Global.GD.level.score = Star.Done;
		else if (bestCode && !bestExec)
			Global.GD.level.score = Star.Code;
		else if (!bestCode && bestExec)
			Global.GD.level.score = Star.Exec;
		else
			Global.GD.level.score = Star.All;

		await GameStateManager.SaveGD();
		
		// Détermine le nombre d'étoile à afficher
		int scoredStars = 0;
		// if (Global.GD.levelScore != null)
		// {
		// 	//check 0, 1, 2 or 3 stars
		// 	if (score >= Global.GD.levelScore[0])
		// 	{
		// 		scoredStars = 3;
		// 	}
		// 	else if (score >= Global.GD.levelScore[1])
		// 	{
		// 		scoredStars = 2;
		// 	}
		// 	else
		// 	{
		// 		scoredStars = 1;
		// 	}
		// }
		//
		// // Affiche le nombre d'étoile désiré
		// for (int nbStar = 0; nbStar < 4; nbStar++)
		// {
		// 	if (nbStar == scoredStars)
		// 		GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, true);
		// 	else
		// 		GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, false);
		// }

		//save score only if better score
		int savedScore = PlayerPrefs.GetInt(Global.GD.mode + Path.DirectorySeparatorChar + Global.GD.level.name + Global.GD.scoreKey, 0);
		if (savedScore < scoredStars)
		{
			PlayerPrefs.SetInt(Global.GD.mode + Path.DirectorySeparatorChar + Global.GD.level.name + Global.GD.scoreKey, scoredStars);
			PlayerPrefs.Save();
		}
	}

	// Cancel End (see ReloadState button in editor)
	public void cancelEnd()
	{
		foreach (GameObject endGO in f_requireEndPanel)
			// in case of several ends pop in the same time (for instance exit reached and detected)
			foreach (NewEnd end in endGO.GetComponents<NewEnd>())
				GameObjectManager.removeComponent(end);
	}

	private IEnumerator delayNoMoreAttemptDetection()
	{
		// wait three frames in case win will be detected (win is priority with noMoreAttempt)
		yield return null;
		yield return null;
		yield return null;
		if (f_requireEndPanel.Count <= 0 && playButtonAmount.activeSelf && playButtonAmount.GetComponentInChildren<TMP_Text>().text == "0")
			GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.NoMoreAttempt });
	}
}

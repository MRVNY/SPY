using UnityEngine;
using FYFY;
using DIG.GBLXAPI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;



public class SendStatements : FSystem {

    public static class Globals
    {
        // global int
        public static string lv;
        public static String st_lv;
        public static DateTime start;
        public static int nb_lv_completed;
    }

    private Family f_actionForLRS = FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformedForLRS)));

    public static SendStatements instance;

    public SendStatements()
    {
        instance = this;
    }

	protected override void onStart()
    {
		initGBLXAPI();
    }

    public void initGBLXAPI()
    {
        if (!GBLXAPI.IsInit)
            GBLXAPI.Init(GBL_Interface.lrsAddresses);

        GBLXAPI.debugMode = false;

        string sessionID = Environment.MachineName + "-" + DateTime.Now.ToString("yyyy.MM.dd.hh.mm.ss");
        //Generate player name unique to each playing session (computer name + date + hour)
        GBL_Interface.playerName = Global.GD.player;//String.Format("{0:X}", sessionID.GetHashCode());
        GBL_Interface.userUUID = GBL_Interface.playerName;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        // Do not use callbacks because in case in the same frame actions are removed on a GO and another component is added in another system, family will not trigger again callback because component will not be processed
        foreach (GameObject go in f_actionForLRS)
        {
            if (Global.GD == null) GBL_Interface.playerName = "Studennt";
            else GBL_Interface.playerName = Global.GD.player;
            ActionPerformedForLRS[] listAP = go.GetComponents<ActionPerformedForLRS>();
            int nb = listAP.Length;
            ActionPerformedForLRS ap;
            if (!this.Pause)
            {
                for (int i = 0; i < nb; i++)
                {
                    ap = listAP[i];
                    //If no result info filled
                    if (!ap.result)
                    {
                        GBL_Interface.SendStatement(ap.verb, ap.objectType, ap.activityExtensions);
                    }
                    else
                    {
                        bool? completed = null, success = null;

                        if (ap.completed > 0)
                            completed = true;
                        else if (ap.completed < 0)
                            completed = false;

                        if (ap.success > 0)
                            success = true;
                        else if (ap.success < 0)
                            success = false;

                        GBL_Interface.SendStatementWithResult(ap.verb, ap.objectType, ap.activityExtensions, ap.resultExtensions, completed, success, ap.response, ap.score, ap.duration);
                    }
                }
            }
            for (int i = nb - 1; i > -1; i--)
            {
                GameObjectManager.removeComponent(listAP[i]);
            }
        }
	}

    public void testSendStatement()
    {
        Debug.Log(GBL_Interface.playerName + " asks to send statement...");
        GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
        {
            verb = "interacted",
            objectType = "menu"
        });
    }
		public void SendLevel(string lv)//int level)
		{
        //String st_lv="";

        // Debug.Log(lv);
        // if (lv==0)
        //   lv=Globals.lv+1;
        Globals.lv=lv;
        // Debug.Log(GBL_Interface.playerName + " try level " +lv.ToString());
        Globals.st_lv="level "+lv.ToString();
        SendStatements.Globals.start = DateTime.Now;
        GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
        {
            verb = "commence",
            objectType = "level",
            activityExtensions = new Dictionary<string, string>()
            {
                { "lv", Globals.lv }
            }
        });
        }

    public void WinLevel(int score, int nb, int code_length, int execution_length)//, int duration)
    {
      TimeSpan duration = DateTime.Now-Globals.start;

      GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
      {
          verb = "reussit",
          objectType = "level",
          activityExtensions = new Dictionary<string, string>()
          {
            { "lv", Globals.lv},
            { "score", score.ToString() },
            { "duration", duration.TotalSeconds.ToString()},
            { "nb_lv_completed", nb.ToString()},
            { "code_length", code_length.ToString()},
            { "execution_length", execution_length.ToString()}
          }
      });
      // Debug.Log(Global.GD.score);
      if (Global.GD.score[Globals.st_lv]==null){
        Global.GD.score[Globals.st_lv]=0;
      }
      if (score>(int)Global.GD.score[Globals.st_lv]){
        Global.GD.score[Globals.st_lv]=score;
        //Debug.Log(Global.GD.score[Globals.st_lv]);
      }
    }

    public void SendRestart()
    {
      GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
      {
          verb = "recommence",
          objectType = "level",//#Globals.st_lv,
      });
    }

    public void SendBackMenu()
    {
      GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
      {
          verb = "revien sur",
          objectType = "Menu",
      });
    }

    public void SendActions(String actions)
    {
      GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
      {
          verb = "essaie",
          objectType = "level",
          activityExtensions = new Dictionary<string, string>()
          {
            { "actions", actions.ToString() },
            {"lv", Globals.st_lv.ToString()}
          }
      });
    }

    public void SendBeginGame()
		{
				Debug.Log(GBL_Interface.playerName + " Demarre serious-game");

				GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
				{
						verb = "demarre",
						objectType = "serious-game"

				});
		}
    public void SendQuitGame()
		{
				Debug.Log(GBL_Interface.playerName + " Quitte serious-game");

				GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
				{
						verb = "quitte",
						objectType = "serious-game"
				});
		}
    public void ResetData()
		{
				Debug.Log(GBL_Interface.playerName + "Reinitialise donnees personnels");

				GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
				{
						verb = "reinitialise",
						objectType = "donnee_personnel"
				});
		}

}

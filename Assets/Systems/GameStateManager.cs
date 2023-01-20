using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FYFY;
using TMPro;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

/// <summary>
/// This manager enables to save the game state and to restore it on demand for instance when the player is detected by drones, he can reset the game on a state just before the previous execution
/// </summary>
public class GameStateManager : FSystem {

    private Family f_coins = FamilyManager.getFamily(new AnyOfTags("Coin"));
    private Family f_doors = FamilyManager.getFamily(new AnyOfTags("Door"));
    private Family f_directions = FamilyManager.getFamily(new AllOfComponents(typeof(Direction)), new NoneOfComponents(typeof(Detector)));
    private Family f_positions = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new NoneOfComponents(typeof(Detector)));
    private Family f_activables = FamilyManager.getFamily(new AllOfComponents(typeof(Activable)));
    private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));
    private Family f_forControls = FamilyManager.getFamily(new AllOfComponents(typeof(ForControl)));

    private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));

    private SaveContent save;

    private string currentContent;

    public GameObject playButtonAmount;

    GameStateManager instance;

    private static string savePath;

    public GameStateManager()
	{
		instance = this;
        savePath = Application.persistentDataPath;
	}
    
    public static bool SaveExists(string key)
    {
        string path = savePath + "/saves/" + key + ".txt";
        return File.Exists(path);
    }
    
    public static void Save<T>(T objectToSave, string key)
    {
        // chemin du fichier de sauvegarde
        string path = savePath + "/saves/";

        //création du fichier si il n'existe pas
        Directory.CreateDirectory(path);
        
        // convertion des paramètres de sauvegarde en fichier binaires
        BinaryFormatter formatter = new BinaryFormatter();

        using(FileStream fileStream = new FileStream(path + key + ".txt", FileMode.Create))
        {
            formatter.Serialize(fileStream, objectToSave);
        }
    }
    
    public static T Load<T>(string key)
    {
        if (SaveExists(key))
        {
            string path = savePath + "/saves/";
            BinaryFormatter formatter = new BinaryFormatter();
            T returnValue = default(T);
            using (FileStream fileStream = new FileStream(path + key + ".txt", FileMode.Open))
            {
                returnValue = (T)formatter.Deserialize(fileStream);
            }

            return returnValue;
        }
        else return default(T);
    }

    public async static Task SaveGD()
    {
        Save(GameData.mode, "mode");
        Save(GameData.levelList, "levelList");
        Save(GameData.levelToLoad, "levelToLoad");
        Save(GameData.homemadeLevelToLoad, "homemadeLevelToLoad");
        Save(GameData.levelToLoadScore, "levelToLoadScore");
        Save(GameData.dialogMessage, "dialogMessage");
        Save(GameData.actionBlockLimit, "actionBlockLimit");
        Save(GameData.scoreKey, "scoreKey");
        Save(GameData.totalStep, "totalStep");
        Save(GameData.totalActionBlocUsed, "totalActionBlocUsed");
        Save(GameData.totalExecute, "totalExecute");
        Save(GameData.totalCoin, "totalCoin");
        Save(GameData.gameSpeed_default, "gameSpeed_default");
        Save(GameData.gameSpeed_current, "gameSpeed_current");
        Save(GameData.dragDropEnabled, "dragDropEnabled");
        Save(GameData.gameLanguage, "gameLanguage");
        Save(GameData.convoNode, "convoNode");
    }

    public static async Task LoadGD()
    {
        GameData.mode = Load<string>("mode");
        GameData.levelList = Load<Hashtable>("levelList");
        GameData.levelToLoad = Load<(string, int)>("levelToLoad");
        GameData.homemadeLevelToLoad = Load<string>("homemadeLevelToLoad");
        GameData.levelToLoadScore = Load<int[]>("levelToLoadScore");
        GameData.dialogMessage = Load<List<(string,float,string,float, int, int)>>("dialogMessage");
        GameData.actionBlockLimit = Load<Hashtable>("actionBlockLimit");
        GameData.scoreKey = Load<string>("scoreKey");
        GameData.totalStep = Load<int>("totalStep");
        GameData.totalActionBlocUsed = Load<int>("totalActionBlocUsed");
        GameData.totalExecute = Load<int>("totalExecute");
        GameData.totalCoin = Load<int>("totalCoin");
        GameData.gameSpeed_default = Load<float>("gameSpeed_default");
        GameData.gameSpeed_current = Load<float>("gameSpeed_current");
        GameData.dragDropEnabled = Load<bool>("dragDropEnabled");
        GameData.gameLanguage = Load<string>("gameLanguage");
        GameData.convoNode = Load<string>("convoNode");
    }

    protected override void onStart()
    {
        save = new SaveContent();
        f_playingMode.addEntryCallback(delegate { SaveState(); });
    }

    // Save data of all interactable objects in scene
    private void SaveState()
	{
        //reset save
        save.rawSave.coinsState.Clear();
        foreach (GameObject coin in f_coins)
            save.rawSave.coinsState.Add(coin.activeSelf);
        save.rawSave.doorsState.Clear();
        foreach (GameObject door in f_doors)
            save.rawSave.doorsState.Add(door.activeSelf);
        save.rawSave.directions.Clear();
        foreach (GameObject dir in f_directions)
            save.rawSave.directions.Add(dir.GetComponent<Direction>().direction);
        save.rawSave.positions.Clear();
        foreach (GameObject pos in f_positions)
            save.rawSave.positions.Add(new SaveContent.RawPosition(pos.GetComponent<Position>()));
        save.rawSave.activables.Clear();
        foreach (GameObject act in f_activables)
            save.rawSave.activables.Add(new SaveContent.RawActivable(act.GetComponent<Activable>()));
        save.rawSave.currentDroneActions.Clear();    
        foreach(GameObject go in f_currentActions)
            if(go.GetComponent<CurrentAction>().agent.CompareTag("Drone"))
                save.rawSave.currentDroneActions.Add(new SaveContent.RawCurrentAction(go));
        save.rawSave.currentLoopParams.Clear();
        foreach (GameObject go in f_forControls)
            save.rawSave.currentLoopParams.Add(new SaveContent.RawLoop(go.GetComponent<ForControl>()));

        currentContent = JsonUtility.ToJson(save.rawSave);

        // If amount enabled, reduce by 1
        if (playButtonAmount.activeSelf)
        {
            TMP_Text amountText = playButtonAmount.GetComponentInChildren<TMP_Text>();
            amountText.text = "" + (int.Parse(amountText.text) - 1);
        }
    }

    // Used in StopButton and ReloadState buttons in editor
    // Load saved state an restore data on interactable game objects
    public void LoadState()
    {
        save.rawSave = JsonUtility.FromJson<SaveContent.RawSave>(currentContent);
        for (int i = 0; i < f_coins.Count && i < save.rawSave.coinsState.Count ; i++)
        {
            GameObjectManager.setGameObjectState(f_coins.getAt(i), save.rawSave.coinsState[i]);
            f_coins.getAt(i).GetComponent<Renderer>().enabled = save.rawSave.coinsState[i];
        }
        for (int i = 0; i < f_doors.Count && i < save.rawSave.doorsState.Count ; i++)
        {
            GameObjectManager.setGameObjectState(f_doors.getAt(i), save.rawSave.doorsState[i]);
            f_doors.getAt(i).GetComponent<Renderer>().enabled = save.rawSave.doorsState[i];
        }
        for (int i = 0; i < f_directions.Count && i < save.rawSave.directions.Count ; i++)
            f_directions.getAt(i).GetComponent<Direction>().direction = save.rawSave.directions[i];
        for (int i = 0; i < f_positions.Count && i < save.rawSave.positions.Count ; i++)
        {
            Position pos = f_positions.getAt(i).GetComponent<Position>();
            pos.x = save.rawSave.positions[i].x;
            pos.y = save.rawSave.positions[i].y;
        }
        for (int i = 0; i < f_activables.Count && i < save.rawSave.activables.Count; i++)
        {
            Activable act = f_activables.getAt(i).GetComponent<Activable>();
            act.slotID = save.rawSave.activables[i].slotID;
        }
        foreach(GameObject go in f_currentActions)
            if(go.GetComponent<CurrentAction>().agent.CompareTag("Drone"))
                GameObjectManager.removeComponent<CurrentAction>(go);
        foreach(SaveContent.RawCurrentAction act in save.rawSave.currentDroneActions)
            GameObjectManager.addComponent<CurrentAction>(act.action, new{agent = act.agent});
        for (int i = 0; i < f_forControls.Count && i < save.rawSave.currentLoopParams.Count; i++)
        {
            ForControl fc = f_forControls.getAt(i).GetComponent<ForControl>();
            fc.currentFor = save.rawSave.currentLoopParams[i].currentFor;
            fc.nbFor = save.rawSave.currentLoopParams[i].nbFor;
            fc.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = fc.nbFor.ToString();
        }

        // If amount enabled, reduce by 1
        if (playButtonAmount.activeSelf)
        {
            TMP_Text amountText = playButtonAmount.GetComponentInChildren<TMP_Text>();
            amountText.text = "" + (int.Parse(amountText.text) + 1);
        }
    }
}

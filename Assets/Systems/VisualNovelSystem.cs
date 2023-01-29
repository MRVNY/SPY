using System;
using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.Accessibility;
using UnityEngine.SceneManagement;

/// <summary>
/// Manage dialogs at the begining of the level
/// </summary>
public class VisualNovelSystem : FSystem
{
	private Family f_VN = FamilyManager.getFamily(new AllOfComponents(typeof(VisualNovel)));
	private VisualNovel VN;
	
	//convoTree & paths
	private string node;
	private string next;
	private JObject convoTree;
	private string treePath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ConvoTree" + Path.DirectorySeparatorChar;
	private string imgPath;
	
	//Async Typewrtier & Skip
	private List<string> toWrite = new List<string>();
	private Task writing;
	public Button skipButton;
	bool skipped = false;
	
	private GameObject optionPanel;
	public static VisualNovelSystem Instance;

	protected override async void onStart()
	{
<<<<<<< HEAD
		//if(LevelGenerator.loadingGD != null) await LevelGenerator.loadingGD;
=======
		Instance = this;
		if(LevelGenerator.loadingGD != null) await LevelGenerator.loadingGD;
>>>>>>> origin/LevelEditor
		
		//if (Global.GD == null)
		//	return;

		VN = f_VN.First().GetComponent<VisualNovel>();
<<<<<<< HEAD
		//skipButton = VN.dialog.transform.parent.GetComponent<Button>();
		//skipButton.onClick.AddListener(() => { Skip(); });
        optionPanel = VN.options[0].transform.parent.parent.gameObject;
		toggleUI("VN_Off");
		
		//if (Global.GD.convoNode != null)
		//{
  //          //Debug.Log(Global.GD.level.node.name);
		//	if(SceneManager.GetActiveScene().name == "LevelMap")
		//		convoTree = JObject.Parse(File.ReadAllText(treePath + "LevelMap.json"));
		//    else convoTree = JObject.Parse(File.ReadAllText(treePath + Global.GD.level.node.name + ".json"));
		//	node = Global.GD.convoNode;
=======
		skipButton = VN.dialog.transform.parent.GetComponent<Button>();
		skipButton.onClick.AddListener(() => { Next(); });
		optionPanel = VN.options[0].transform.parent.parent.gameObject;
		toggleUI("VN_Off");
		
		if (Global.GD.convoNode != null)
		{
			if(SceneManager.GetActiveScene().name == "LevelMap")
				convoTree = JObject.Parse(File.ReadAllText(treePath + "LevelMap.json"));
			else convoTree = JObject.Parse(File.ReadAllText(treePath + "Intro.json"));
			//else convoTree = JObject.Parse(File.ReadAllText(treePath + Global.GD.level.node.name + ".json"));
			
			if(Global.GD.level != null && !Global.GD.convoNode.Contains("ask"))
				Global.GD.convoNode = Global.GD.level.name + ".0";
			node = Global.GD.convoNode;
>>>>>>> origin/LevelEditor

		//	imgPath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
		//	          Path.DirectorySeparatorChar + Global.GD.mode + Path.DirectorySeparatorChar +
		//	          "Images" + Path.DirectorySeparatorChar;

		//	//Global.GD.gameLanguage = "en";
			
		//	if (convoTree[node] != null)
		//	{
		//		toggleUI("VN_On");
		//		setVN();
		//	}
		//}
	}
	
	public void setVN()
	{
		node = Global.GD.convoNode;
        JToken jNode = convoTree[node];
        if (jNode == null)
        {
	        toggleUI("VN_Off");
	        return;
        }

		for (int i = 0; i < VN.options.Count(); i++)
			VN.options[i].transform.parent.gameObject.SetActive(false);

		// set text
		skipButton.enabled = true;
		if (toWrite.Count == 0)
		{
			string text = jNode[Global.GD.gameLanguage].ToString();
			if (text.Contains("{name}")) text = text.Replace("{name}", Global.GD.player);
			toWrite = text.Split('\n').ToList();
		}

		writing = TypeWriter(toWrite[0]);

		if (toWrite.Count == 1)
		{
			// set image
			if (jNode["img"] != null)
				setImageSprite(VN.img, imgPath + jNode["img"].ToString());

			// set camera pos
			if (jNode["camX"] != null && jNode["camY"] != null)
				GameObjectManager.addComponent<FocusCamOn>(MainLoop.instance.gameObject, new { camX = jNode["camX"], camY = jNode["camY"] });

			// set options
			else if (jNode["options"] != null) setOptions();
			
			// set NextNode
			next = null;
			if (node[^1].ToString().All(char.IsDigit))
			{
				string guessNext = node.Substring(0,node.Length-1) + (int.Parse(node[^1].ToString()) + 1);
				if(convoTree[guessNext]!=null) next = guessNext;
			}
			
			// execute 
			if (jNode["action"] != null && !jNode["action"].ToString().Contains("ending")) setActions();
		}
	}

	private async void setOptions()
	{
		toggleUI("options");
		skipButton.enabled = false;

		var objs = convoTree[node]["options"][Global.GD.gameLanguage].OfType<JProperty>();
		for (int i = 0; i < VN.options.Count(); i++)
		{
			if (i < objs.Count())
			{
				var button = VN.options[i].transform.parent.gameObject;
				VN.options[i].text = (string)objs.ElementAt(i).Value;
				string nextNode = objs.ElementAt(i).Name;

				//Wait for typewirtter and show option
				await writing;
				button.SetActive(true);
				VN.options[i].transform.parent.GetComponent<Button>().onClick.RemoveAllListeners();
				VN.options[i].transform.parent.GetComponent<Button>().onClick.AddListener(
					() => {
						Global.GD.convoNode = nextNode;
						toWrite.Clear();
						setVN();
					});
			}
		}
	}
	
	private void setActions()
	{
		string[] action = convoTree[node]["action"].ToString().Split(',');
		switch (action[0])
		{
			case "end":
				Global.GD.convoNode = null;
				break;
			case "value": break;
			case "mood": break;
			case "ending1":
				Global.GD.ending = 1;
				SceneManager.LoadScene("LevelMap");
				break;
			case "ending2":
				Global.GD.ending = 2;
				SceneManager.LoadScene("LevelMap");
				break;
			case "askName":
				toggleUI("askName");
				skipButton.enabled = false;
				VN.askName.GetComponentInChildren<Button>().onClick.AddListener( () =>
				{
					Global.GD.player = VN.askName.GetComponentInChildren<TMP_InputField>().text;
					Global.GD.convoNode = "gotName";
					toWrite.Clear();
					toggleUI("VN_On");
					GameStateManager.SaveGD();
					setVN();
				});
				break;
			case "next":
				next = action[1]; break;
			case "changeDiff":
				int tmp = int.Parse(action[1]);
				Global.GD.difficulty = Math.Min(Math.Max(0,tmp),2); break;
		}
	}

	//Show text with typewriter effect
	async Task TypeWriter(string toType)
	{
		skipped = false;
		skipButton.enabled = true;
		string story = toType;
		VN.dialog.text = "";
		foreach (char c in story) 
		{
			if(skipped) break;
			VN.dialog.text += c;
			await Task.Delay(10);
		}
		skipped = true;
	}

	//Skip Typewriter when it's not finished
	public async void Next()
	{
		if (convoTree[node]["options"] == null && skipped && toWrite.Count == 1 && next == null)
		{
			toggleUI("VN_Off");
			toWrite.Clear();
		}

		else if (skipped)
		{
			toWrite.RemoveAt(0);
			skipButton.enabled = false;
			if (toWrite.Count == 0)
			{
				// execute 
				if (convoTree[node]["action"] != null && convoTree[node]["action"].ToString().Contains("ending")) setActions();
				
				if (next != null)
				{
					Global.GD.convoNode = next;
					setVN();
				}
				else
				{
					toggleUI("VN_Off");
				}
			}
			else setVN();
		}
		
		else if (writing != null)
		{
			skipped = true;
			await writing;
			VN.dialog.text = toWrite[0];
			
			if (convoTree[node]["options"] != null && toWrite.Count==1) skipButton.enabled = false;
		}
	}

	// Affiche l'image associ�e au dialogue
	private void setImageSprite(Image img, string path)
	{
		toggleUI("img");
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			MainLoop.instance.StartCoroutine(GetTextureWebRequest(img, path));
		}
		else
		{
			Texture2D tex2D = new Texture2D(2, 2); //create new "empty" texture
			byte[] fileData = File.ReadAllBytes(path); //load image from SPY/path
			if (tex2D.LoadImage(fileData))
			{
				img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100.0f);
				img.type = Image.Type.Simple;
			}
		}
	}

	private void toggleUI(string ui)
	{
		optionPanel.SetActive(false);
		VN.img.gameObject.SetActive(false);
		VN.askName.SetActive(false);
		
		switch (ui)
		{
			case "options":
				optionPanel.SetActive(true); break;
			case "img":
				VN.img.gameObject.SetActive(true); break;
			case "askName":
				VN.askName.SetActive(true); break;
			case "VN_On":
				VN.gameObject.SetActive(true); break;
			case "VN_Off":
				VN.gameObject.SetActive(false);
				GameStateManager.SaveGD();
				break;
		}
	}

	private IEnumerator GetTextureWebRequest(Image img, string path)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
		}
		else
		{
			Texture2D tex2D = ((DownloadHandlerTexture)www.downloadHandler).texture;
			img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100.0f);
		}
	}

	public void endLevelConvo()
	{
		if (convoTree[Global.GD.level.name + ".end.0"] != null)
		{
			Global.GD.convoNode = Global.GD.level.name + ".end.0";
			toggleUI("VN_On");
			setVN();
		}
	}
}
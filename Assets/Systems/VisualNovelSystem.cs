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

/// <summary>
/// Manage dialogs at the begining of the level
/// </summary>
public class VisualNovelSystem : FSystem
{
	private Family f_VN = FamilyManager.getFamily(new AllOfComponents(typeof(VisualNovel)));
	private VisualNovel VN;

	private GameData gameData;

	//convoTree & paths
	public string node;
	private JObject convoTree;
	private string treePath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ConvoTree";
	private string imgPath;
	
	//Async Typewrtier & Skip
	private Task writing;
	public Button skipButton;
	bool skipped = false;

	protected override void onStart()
	{
		VN = f_VN.First().GetComponent<VisualNovel>();
		skipButton = VN.dialog.transform.parent.GetComponent<Button>();
		VN.gameObject.SetActive(false);
		gameData = GameData.Instance;
		
		if (gameData != null)
		{
			convoTree = JObject.Parse(File.ReadAllText(treePath + Path.DirectorySeparatorChar + "1.json"));
			node = gameData.convoNode;

			imgPath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
			          Path.DirectorySeparatorChar + gameData.levelToLoad.Item1 + Path.DirectorySeparatorChar +
			          "Images" + Path.DirectorySeparatorChar;

			gameData.gameLanguage = "en";
			
			if (convoTree[node] != null)
			{
				VN.gameObject.SetActive(true);
				setVN();
			}
		}
	}
	
	private async void setVN()
	{
		if (convoTree[node] == null) VN.gameObject.SetActive(false);

		for (int i = 0; i < VN.options.Count(); i++)
		{
			VN.options[i].transform.parent.gameObject.SetActive(false);
		}
		node = gameData.convoNode;

		// set text
	    writing = TypeWriter(convoTree[node][gameData.gameLanguage].ToString());
		
		// set image
		if(convoTree[node]["img"] != null)
		{
			setImageSprite(VN.img, imgPath + convoTree[node]["img"].ToString());
		}

		// set camera pos
		// if (gameData.dialogMessage[nDialog].Item5 != -1 && gameData.dialogMessage[nDialog].Item6 != -1)
  //       {
		// 	GameObjectManager.addComponent<FocusCamOn>(MainLoop.instance.gameObject, new { camX = gameData.dialogMessage[nDialog].Item5, camY = gameData.dialogMessage[nDialog].Item6 });
  //       }
  
  		// set options
        if (convoTree[node]["options"] != null)
        {
	        var objs = convoTree[node]["options"][gameData.gameLanguage].OfType<JProperty>();
	        for (int i = 0; i < VN.options.Count(); i++)
	        {
		        if (objs.Count() > i - 1)
		        {
			        var button = VN.options[i].transform.parent.gameObject;
			        VN.options[i].text = (string)objs.ElementAt(i).Value;
			        string nextNode = objs.ElementAt(i).Name;

			        await writing;
			        button.SetActive(true);
			        VN.options[i].transform.parent.GetComponent<Button>().onClick.AddListener(
				        () =>
				        {
					        gameData.convoNode = nextNode;
					        setVN();
				        });
		        }
	        }
	        skipButton.enabled = false;
        }
        else skipButton.enabled = true;

        // execute action
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
	public async void Skip()
	{
		if (convoTree[node]["options"] == null && skipped) VN.gameObject.SetActive(false);
		
		if (writing != null)
		{
			skipped = true;
			await writing;
			VN.dialog.text = convoTree[node][gameData.gameLanguage].ToString();
			
			if (convoTree[node]["options"] != null) skipButton.enabled = false;
		}
	}

	// Affiche l'image associ�e au dialogue
	private void setImageSprite(Image img, string path)
	{
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
}
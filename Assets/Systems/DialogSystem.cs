using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Manage dialogs at the begining of the level
/// </summary>
public class DialogSystem : FSystem
{
	public GameObject dialogPanel;
	private int nDialog = 0;
	private string sepa = Path.DirectorySeparatorChar.ToString();

	protected override void onStart()
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		//Activate DialogPanel if there is a message
		if (Global.GD.dialogMessage != null && nDialog < Global.GD.dialogMessage.Count && !dialogPanel.transform.parent.gameObject.activeSelf)
		{
			showDialogPanel();
		}
	}


	// Affiche le panneau de dialoge au d�but de niveau (si besoin)
	public void showDialogPanel()
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, true);
		nDialog = 0;

		configureDialog();

		if (Global.GD.dialogMessage.Count > 1)
		{
			setActiveOKButton(false);
			setActiveNextButton(true);
		}
		else
		{
			setActiveOKButton(true);
			setActiveNextButton(false);
		}
	}

	// See NextButton in editor
	// Permet d'afficher la suite du dialogue
	public void nextDialog()
	{
		nDialog++; // On incr�mente le nombre de dialogue

		configureDialog();

		// Si il reste des dialogues � afficher ensuite
		if (nDialog + 1 < Global.GD.dialogMessage.Count)
		{
			setActiveOKButton(false);
			setActiveNextButton(true);
		}
		else
		{
			setActiveOKButton(true);
			setActiveNextButton(false);
		}
	}

	private void configureDialog()
    {
		// set text
		GameObject textGO = dialogPanel.transform.Find("Text").gameObject;
		if (Global.GD.dialogMessage[nDialog].Item1 != null)
		{
			GameObjectManager.setGameObjectState(textGO, true);
			textGO.GetComponent<TextMeshProUGUI>().text = Global.GD.dialogMessage[nDialog].Item1;
			if (Global.GD.dialogMessage[nDialog].Item2 != -1)
				((RectTransform)textGO.transform).sizeDelta = new Vector2(((RectTransform)textGO.transform).sizeDelta.x, Global.GD.dialogMessage[nDialog].Item2);
			else
				((RectTransform)textGO.transform).sizeDelta = new Vector2(((RectTransform)textGO.transform).sizeDelta.x, textGO.GetComponent<LayoutElement>().preferredHeight);
		}
		else
			GameObjectManager.setGameObjectState(textGO, false);
		// set image
		GameObject imageGO = dialogPanel.transform.Find("Image").gameObject;
		if (Global.GD.dialogMessage[nDialog].Item3 != null)
		{
			GameObjectManager.setGameObjectState(imageGO, true);
			setImageSprite(imageGO.GetComponent<Image>(), Global.GD.path + Global.GD.mode + sepa + "Images" + sepa + Global.GD.dialogMessage[nDialog].Item3);
			if (Global.GD.dialogMessage[nDialog].Item4 != -1)
				((RectTransform)imageGO.transform).sizeDelta = new Vector2(((RectTransform)imageGO.transform).sizeDelta.x, Global.GD.dialogMessage[nDialog].Item4);
			else
				((RectTransform)imageGO.transform).sizeDelta = new Vector2(((RectTransform)imageGO.transform).sizeDelta.x, imageGO.GetComponent<LayoutElement>().preferredHeight);
		}
		else
			GameObjectManager.setGameObjectState(imageGO, false);
		// set camera pos
		if (Global.GD.dialogMessage[nDialog].Item5 != -1 && Global.GD.dialogMessage[nDialog].Item6 != -1)
        {
			GameObjectManager.addComponent<FocusCamOn>(MainLoop.instance.gameObject, new { camX = Global.GD.dialogMessage[nDialog].Item5, camY = Global.GD.dialogMessage[nDialog].Item6 });
        }
	}


	// Active ou non le bouton Ok du panel dialogue
	public void setActiveOKButton(bool active)
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.Find("Buttons").Find("OKButton").gameObject, active);
	}


	// Active ou non le bouton next du panel dialogue
	public void setActiveNextButton(bool active)
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.Find("Buttons").Find("NextButton").gameObject, active);
	}


	// See OKButton in editor
	// D�sactive le panel de dialogue
	public void closeDialogPanel()
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
		nDialog = Global.GD.dialogMessage.Count;
	}

	// Affiche l'image associ�e au dialogue
	public void setImageSprite(Image img, string path)
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
				img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100.0f);
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
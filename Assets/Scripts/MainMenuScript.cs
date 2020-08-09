/* Name: Larry Y.
 * Date: May 27, 2019
 * Desc: Functions for the main menu. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
	public string[] mapList;
	public Canvas mapPickerCanvas;

	public void LoadMap(string mapName)
	{
		SceneManager.LoadScene(mapName);
	}

	public void ToggleMapPicker()
	{
		if (mapPickerCanvas.enabled)
		{
			mapPickerCanvas.enabled = false;
			GetComponent<Canvas>().enabled = true;
		}
		else
		{
			mapPickerCanvas.enabled = true;
			GetComponent<Canvas>().enabled = false;
		}
	}

	public void StartGame()
	{
		SceneManager.LoadScene(mapList[0]);
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}

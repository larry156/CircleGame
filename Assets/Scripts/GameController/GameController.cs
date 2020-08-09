/* Name: Larry Y.
 * Date: May 27, 2019
 * Desc: Script for game controller. Handles stuff like spawning players and whatnot */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
	public GameObject playerPrefab;
	public GameObject[] aiPrefabs;
	public GameObject winScreen;
	public GameObject loseScreen;

	public float minZoom, maxZoom, absoluteMaxZoom; // maxZoom can increase, up to absoluteMaxZoom

	private GameObject[] spawnPoints;
	public int circlesRemaining;
	//private List<GameObject> nonPropCircles;
	private bool gameEnded = false;

	// Start is called before the first frame update
	private void Start()
	{
		Spawn();
	}

	// Spawn AIs and a player using all spawnpoints in the scene
	private void Spawn()
	{
		spawnPoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
		circlesRemaining = 0;
		bool playerHasBeenSpawned = false;
		foreach (GameObject spawn in spawnPoints)
		{
			Spawnpoint script = spawn.GetComponent<Spawnpoint>();
			if (script.canSpawnAI && script.canSpawnPlayer) // Some spawnpoints are allowed to spawn AIs and players...
			{
				int AIorPlayer = Random.Range(0, 2);
				if (playerHasBeenSpawned || AIorPlayer == 0)
				{
					int prefabToSpawn = Random.Range(0, aiPrefabs.Length);
					GameObject.Instantiate(aiPrefabs[prefabToSpawn], spawn.transform.position, spawn.transform.rotation);
				}
				else
				{
					GameObject.Instantiate(playerPrefab, spawn.transform.position, spawn.transform.rotation);
					playerHasBeenSpawned = true;
				}
			}
			else if (script.canSpawnAI) // ...some can only spawn AIs...
			{
				int prefabToSpawn = Random.Range(0, aiPrefabs.Length);
				GameObject.Instantiate(aiPrefabs[prefabToSpawn], spawn.transform.position, spawn.transform.rotation);
			}
			else if (script.canSpawnPlayer) // ...and some can only spawn players...
			{
				GameObject.Instantiate(playerPrefab, spawn.transform.position, spawn.transform.rotation);
			}
			circlesRemaining++;
		}
	}

	// Return to the main menu
	public void ReturnToMenu()
	{
		SceneManager.LoadScene("MainMenu");
	}

	// On game over
	private void GameOver(bool playerWon)
	{
		gameEnded = true;
		if (playerWon)
		{
			winScreen.SetActive(true);
		}
		else
		{
			loseScreen.SetActive(true);
		}
	}

	// Called by circles whenever one is consumed
	public void OnConsume(GameObject consumedCircle)
	{
		circlesRemaining--;
		// If the player is consumed, that's a game over
		if (consumedCircle.GetComponent<CircleScript>().isPlayer)
		{
			GameOver(false);
		}
		// If there's only one circle left, and the player is still alive, that circle is the player, who has just won.
		else if (circlesRemaining <= 1 && !gameEnded)
		{
			GameOver(true);
		}
	}

	// Update is called once per frame
	private void Update()
	{

	}
}

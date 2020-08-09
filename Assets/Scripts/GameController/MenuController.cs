/* Name: Larry Y.
 * Date: March 9, 2020
 * Desc: Modified GameController script for the main menu */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
	//public GameObject playerPrefab;
	public GameObject[] aiPrefabs;
	public int circlesRemaining, preferredCircles;
	public float spawnCheckInterval, maxMass;
	//public GameObject winScreen;
	//public GameObject loseScreen;

	//public float minZoom, maxZoom, absoluteMaxZoom; // maxZoom can increase, up to absoluteMaxZoom

	private GameObject[] spawnPoints;
	
	private float lastSpawnAttempt;
	//private List<GameObject> nonPropCircles;
	//private bool gameEnded = false;

	// Start is called before the first frame update
	private void Start()
	{
		Spawn(true);
		lastSpawnAttempt = Time.time;
	}

	// Spawn AIs and a player using all spawnpoints in the scene
	private void Spawn(bool initial = false)
	{
		spawnPoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
		//circlesRemaining = 0;
		if (initial)
		{
			foreach (GameObject spawn in spawnPoints)
			{
				//Spawnpoint script = spawn.GetComponent<Spawnpoint>();
				int prefabToSpawn = Random.Range(0, aiPrefabs.Length);
				GameObject.Instantiate(aiPrefabs[prefabToSpawn], spawn.transform.position, spawn.transform.rotation);
				circlesRemaining++;
			}
		}
		else
		{
			GameObject spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
			int prefabToSpawn = Random.Range(0, aiPrefabs.Length);
			GameObject.Instantiate(aiPrefabs[prefabToSpawn], spawn.transform.position, spawn.transform.rotation);
			circlesRemaining++;
		}
	}

	// Called by circles whenever one is consumed
	public void OnConsume(GameObject consumedCircle)
	{
		circlesRemaining--;
	}

	// Update is called once per frame
	private void Update()
	{
		if (Time.time - spawnCheckInterval >= lastSpawnAttempt)
		{
			if (circlesRemaining < preferredCircles)
			{
				Spawn();
			}
			lastSpawnAttempt = Time.time;
			GameObject[] circles = GameObject.FindGameObjectsWithTag("Circle");
			List<GameObject> toRemove = new List<GameObject>();
			foreach (GameObject circ in circles)
			{
				if (circ.GetComponent<Rigidbody2D>().mass >= maxMass)
				{
					toRemove.Add(circ);
				}
			}
			for (int i = 0; i < toRemove.Count; i++)
			{
				Destroy(toRemove[0]);
				circlesRemaining--;
			}
		}
	}
}

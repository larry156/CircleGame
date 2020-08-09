// Literally just contains two bools so the GameController can determine where to spawn AIs and players.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
	public bool canSpawnPlayer, canSpawnAI;
}

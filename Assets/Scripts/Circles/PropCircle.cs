/* Name: Larry Y.
 * Date: May 2, 2019
 * Desc: 
 * PropCircles are basically just circles that aren't a player or AI, whose only purpose is giving the other circles something to consume.
 * This script really only exists to randomize their colour and size, without having to put a full CircleScript on each prop. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropCircle : MonoBehaviour
{
	public float minSize, maxSize;

	void Start()
    {
		GetComponent<SpriteRenderer>().color = Random.ColorHSV();
		GetComponent<Transform>().localScale = GetComponent<Transform>().localScale * Random.Range(minSize, maxSize);
	}
}

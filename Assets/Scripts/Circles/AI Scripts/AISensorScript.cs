/* Name: Larry Y.
 * Date: May 25, 2019
 * Desc: Stuff to help the AI survive longer. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISensorScript : MonoBehaviour
{
	private AIScript myAI;
	private CircleScript myCircScript;
	private List<GameObject> dangers;

	private void Start()
	{
		myAI = GetComponentInParent<AIScript>();
		myCircScript = GetComponentInParent<CircleScript>();
		dangers = new List<GameObject>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		// If the sensor detects a circle larger than this one
		if (collision.GetComponent<CircleScript>() != null && // GetComponent will return null if the other object is, say, a prop circle
			collision.GetComponent<CircleScript>().myMagnitude - myCircScript.myMagnitude >= myCircScript.absorptionThreshold &&
			!myAI.isFleeing)
		{
			if (myAI.debugLogging)
			{
				Debug.Log("Their magnitude: " + collision.GetComponent<CircleScript>().myMagnitude);
				Debug.Log("My magnitude: " + myCircScript.myMagnitude);
				Debug.Log("Difference in magnitude is " + (collision.GetComponent<CircleScript>().myMagnitude - myCircScript.myMagnitude));
			}

			// Go to a point away from the other circle until there's no danger.
			Vector3 targetPoint = (transform.position - collision.transform.position).normalized * 1000;
			myAI.Flee(targetPoint);
			// Keep track of the danger
			if (!dangers.Contains(collision.gameObject))
			{
				dangers.Add(collision.gameObject);
			}
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		OnTriggerEnter2D(collision);
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (dangers.Contains(collision.gameObject))
		{
			dangers.Remove(collision.gameObject);
			if (myAI.debugLogging)
			{
				Debug.Log("There are " + dangers.Count + " dangers nearby.");
			}
		}
		if (dangers.Count < 1)
		{
			myAI.Invoke("StopFleeing", 1f);
		}
	}
}

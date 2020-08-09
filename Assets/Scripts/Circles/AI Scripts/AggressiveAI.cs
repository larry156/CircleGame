/* Name: Larry Y.
 * Date: May 18, 2019
 * Desc: An AI based off of the IntermediateAI that is more likely to go after players. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressiveAI : AIScript
{
	public float maxDistanceFactor;
	public float deathCheckFrequency; // How often the AI checks to see if it's target is alive
	public int baseWeight, enemyWeight;
	public float distanceModifier; // Each unit of distance will subtract this much from a possible target's weight

	private float lastDeathCheck = 0.0f;

	// Get a target that this circle will go after. It will favour circles that are closer, but won't necessarily go for the closest one.
	public override void GetTarget()
	{
		targetDead = false;
		lastSearchTime = Time.time;
		Dictionary<GameObject, int> circles = new Dictionary<GameObject, int>();
		int weightSum = 0;
		// Find valid possible targets and give them a weight based on distance from this circle
		foreach (GameObject circ in GameObject.FindGameObjectsWithTag("Circle"))
		{
			if (transform.localScale.magnitude - circ.transform.localScale.magnitude >= myCircScript.absorptionThreshold &&
				circ != this)
			{
				int weight = baseWeight - Mathf.RoundToInt((circ.transform.position - transform.position).magnitude * distanceModifier);
				if (circ.GetComponent<CircleScript>() != null)
				{
					weight += enemyWeight;
				}
				circles[circ] = weight;
				weightSum += weight;
				if (debugLogging)
				{
					Debug.DrawRay(circ.transform.position, (circ.transform.position - transform.position).normalized * weight, Color.yellow, 2f, false);
				}
			}
		}

		// Pick a target to go after.
		bool targetFound = false;
		foreach(KeyValuePair<GameObject, int> circ in circles)
		{
			if (Random.Range(0, weightSum) < circ.Value)
			{
				curTarget = circ.Key;
				targetTransform = curTarget.transform;
				targetPos = targetTransform.position;
				lastDeathCheck = Time.time;
				targetFound = true;
				if (debugLogging)
				{
					Debug.Log(name + " is now targeting " + curTarget.name);
					Debug.Log("Will search again at " + (Time.time + searchFrequency));
				}
				break;
			}
		}

		if (!targetFound && curTarget == null)
		{
			// Find a spawner and stay near it, preferring closer ones.
			GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
			float closestValidDistance = Mathf.Infinity;
			foreach (GameObject spawner in spawners)
			{
				if ((spawner.GetComponent<Transform>().position - transform.position).magnitude <= closestValidDistance)
				{
					closestValidDistance = (spawner.GetComponent<Transform>().position - transform.position).magnitude;
					curTarget = spawner;
					targetTransform = spawner.transform;
					targetPos = targetTransform.position;
				}
			}
		}
	}

	// Unchanged from base
	public override void Flee(Vector3 goHere)
	{
		isFleeing = true;
		fleePos = goHere;
		targetPos = goHere;
		fledAt = Time.time;
		if (debugLogging)
		{
			Debug.Log(name + " is fleeing");
		}
	}

	// Now it will only find a new target if its current one is dead
	public override void StopFleeing()
	{
		if (Time.time - fledAt > fleeDuration)
		{
			isFleeing = false;
			if (targetDead || curTarget == null)
			{
				GetTarget();
			}
			else
			{
				targetPos = targetTransform.position;
			}
		}
	}

	protected override void FixedUpdate()
	{
		if (Time.time - lastDeathCheck > deathCheckFrequency && curTarget == null)
		{
			targetDead = true;
			lastDeathCheck = Time.time;
		}
		if (Time.time < startDelay) // Give it time to actually find a target.
		{
			return;
		}
		if ((Time.time - lastSearchTime > searchFrequency || targetDead) && !isFleeing)
		{
			GetTarget();
		}
		if ((targetPos - transform.position).magnitude < targetDistanceThreshold)
		{
			targetReached = true;
		}
		else if (targetReached)
		{
			targetReached = false;
		}
		else if (!targetReached)
		{
			Debug.DrawLine(transform.position, targetPos, Color.green, Time.fixedDeltaTime, false);
			Debug.DrawRay(targetPos, -GetDirection().normalized * targetDistanceThreshold, Color.red, Time.fixedDeltaTime, false);
		}

		if (isFleeing)
		{
			targetPos = fleePos;
			StopFleeing();
		}
	}
}

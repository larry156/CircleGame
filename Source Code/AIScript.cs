﻿/* Name: Larry Y.
 * Date: May 18, 2019
 * Desc: Script for AI, contains functions called by other scripts as well. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIScript : MonoBehaviour
{
	public GameObject curTarget;
	public float searchFrequency, lastSearchTime, targetDistanceThreshold, startDelay, fleeDuration;
	public bool debugLogging, targetReached, targetDead, isFleeing;

	private Transform targetTransform;
	private Vector3 targetPos, fleePos;
	private CircleScript myCircScript;
	private float fledAt;

    // Start is called before the first frame update
    void Start()
    {
		myCircScript = GetComponent<CircleScript>();
		GetTarget();
    }

	// Get the closest circle that is able to be consumed by this one.
	public void GetTarget()
	{
		GameObject[] circles = GameObject.FindGameObjectsWithTag("Circle");
		float closestValidDistance = Mathf.Infinity;
		bool foundATarget = false;
		lastSearchTime = Time.time;
		foreach (GameObject circ in circles)
		{
			if ((circ.GetComponent<Transform>().position - transform.position).magnitude <= closestValidDistance &&
				transform.localScale.magnitude - circ.GetComponent<Transform>().localScale.magnitude >= myCircScript.absorptionThreshold)
			{
				closestValidDistance = (circ.GetComponent<Transform>().position - transform.position).magnitude;
				curTarget = circ;
				targetTransform = circ.transform;
				targetPos = targetTransform.position;
				foundATarget = true;
			}
		}
		if (!foundATarget)
		{
			// Find a spawner and stay near it, preferring closer ones.
			// TODO
		}
		if (debugLogging && foundATarget)
		{
			Debug.Log(name + " is now targeting " + curTarget.name);
		}
		targetDead = false;
	}

	// Get a unit vector in the direction of the current target
	public Vector2 GetDirection()
	{
		return (targetPos - transform.position).normalized;
	}

	// Get the angle that points towards the current target
	public float GetTargetAngle()
	{
		// Rotate to face target
		Vector3 targetPosRelative = targetPos;
		targetPosRelative.x -= transform.position.x;
		targetPosRelative.y -= transform.position.y;
		return -Mathf.Atan2(targetPosRelative.x, targetPosRelative.y) * Mathf.Rad2Deg;
	}

	// Flee to goHere
	public void Flee(Vector3 goHere)
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

	// Called by AISensorScript, will tell AI to stop fleeing if it is "safe" and fleeDuration has elaspsed
	public void StopFleeing()
	{
		if (Time.time - fledAt > fleeDuration)
		{
			isFleeing = false;
			GetTarget();
		}
	}

    void FixedUpdate()
    {
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

		if (isFleeing)
		{
			targetPos = fleePos;
		}
    }
}

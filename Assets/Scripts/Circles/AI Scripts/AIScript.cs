/* Name: Larry Y.
 * Date: May 18, 2019
 * Desc: Script for AI, contains functions called by other scripts as well. Also serves as a template for other AI types. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIScript : MonoBehaviour
{
	public GameObject curTarget;
	public float searchFrequency, lastSearchTime, targetDistanceThreshold, startDelay, fleeDuration;
	public bool debugLogging, targetReached, targetDead, isFleeing, goingSomewhere;

	protected Transform targetTransform;
	protected Vector3 targetPos, fleePos;
	protected CircleScript myCircScript;
	protected float fledAt;

	// Start is called before the first frame update
	protected void Start()
    {
		myCircScript = GetComponent<CircleScript>();
		GetTarget();
    }

	// Get the closest circle that is able to be consumed by this one.
	public virtual void GetTarget()
	{
		targetDead = false;
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
			GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
			foreach (GameObject spawner in spawners)
			{
				if ((spawner.GetComponent<Transform>().position - transform.position).magnitude <= closestValidDistance)
				{
					closestValidDistance = (spawner.GetComponent<Transform>().position - transform.position).magnitude;
					curTarget = spawner;
					targetTransform = spawner.transform;
					targetPos = targetTransform.position;
					foundATarget = true;
				}
			}
		}
		if (debugLogging && foundATarget)
		{
			Debug.Log(name + " is now targeting " + curTarget.name);
		}
	}

	// Get a unit vector in the direction of the current target
	public Vector2 GetDirection()
	{
		return (targetPos - transform.position).normalized;
	}

	// Get the angle that points towards the current target (values should be from 0 to 360)
	// Note: This works differently from Vector2.Angle() and should not be used in the same way
	public float GetRotationToTarget()
	{
		// Rotate to face target
		Vector3 targetPosRelative = targetPos;
		targetPosRelative.x -= transform.position.x;
		targetPosRelative.y -= transform.position.y;
		return -Mathf.Atan2(targetPosRelative.x, targetPosRelative.y) * Mathf.Rad2Deg;
	}

	// Get the signed angle between this circle and its target.
	// This gets its own function so that it's easier for it to be used in other scripts.
	public float GetAngleBetweenTarget()
	{
		return Vector2.SignedAngle(GetComponent<Rigidbody2D>().velocity.normalized, GetDirection());
	}

	// Flee to goHere
	public virtual void Flee(Vector3 goHere)
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

	// A less prioritized version of flee
	public virtual void GoHere(Vector3 goHere)
	{
		goingSomewhere = true;
		fleePos = goHere;
		targetPos = goHere;
		fledAt = Time.time;
		if (debugLogging)
		{
			Debug.Log(name + " is going to " + goHere);
		}
	}

	// Called by AISensorScript, will tell AI to stop fleeing if it is "safe" and fleeDuration has elaspsed
	public virtual void StopFleeing()
	{
		if (Time.time - fledAt > fleeDuration)
		{
			isFleeing = false;
			goingSomewhere = false;
			GetTarget();
		}
	}

	protected virtual void FixedUpdate()
    {
		if (Time.time < startDelay) // Give it time to actually find a target.
		{
			return;
		}
        if ((Time.time - lastSearchTime > searchFrequency || targetDead) && !isFleeing)
		{
			GetTarget();
			goingSomewhere = false;
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

		if (isFleeing || goingSomewhere)
		{
			targetPos = fleePos;
			StopFleeing();
		}
    }
}

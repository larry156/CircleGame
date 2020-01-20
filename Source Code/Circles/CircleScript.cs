﻿/* Name: Larry Y.
 * Date: April 30, 2019
 * Desc: The primary script for circles. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType { Basic, Rocket }

public class CircleScript : MonoBehaviour
{
	public MovementType movementType;
	public bool isPlayer, debugLogging;
	public float minZoom, maxZoom, absoluteMaxZoom; // maxZoom can increase, up to absoluteMaxZoom
	public float speed, rotateSpeed, absorptionThreshold, myMagnitude;
	// myMagnitude exists because calling transform.localScale.magnitude in other scripts always returns what the magnitude was at the start for some reason.

	private Rigidbody2D rb;
	private Transform myTransform;
	private CircleCollider2D myCollider;
	private Camera myCamera, deathCam;
	private float cameraToCircleRatio, zoomRatio;
	private AIScript myAI;

	// Start is called before the first frame update
	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		myTransform = GetComponent<Transform>();
		myCollider = GetComponent<CircleCollider2D>();
		myMagnitude = myTransform.localScale.magnitude;
		if (isPlayer)
		{
			myCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			cameraToCircleRatio = myTransform.localScale.magnitude / myCamera.orthographicSize;
			zoomRatio = maxZoom / minZoom;
			deathCam = GameObject.FindGameObjectWithTag("DeathCamera").GetComponent<Camera>();
			deathCam.gameObject.SetActive(false);
			//Debug.Log(cameraToCircleRatio);
		}
		if (!isPlayer) // Players are always green (maybe I'll make a colour picker at some point);
		{
			GetComponent<SpriteRenderer>().color = Random.ColorHSV();
			myAI = GetComponent<AIScript>();
		}
	}

	private void OnDestroy()
	{
		if (isPlayer)
		{
			myCamera.gameObject.SetActive(false);
			deathCam.gameObject.SetActive(true);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Transform otherTransform = collision.gameObject.GetComponent<Transform>();
		//Debug.Log(collision.collider.gameObject.name);
		// If this circle is large enough to consume the other
		if (collision.gameObject.tag == "Circle" && myTransform.localScale.magnitude - otherTransform.localScale.magnitude >= absorptionThreshold)
		{
			if (collision.gameObject.GetComponent<CircleScript>() != null)
			{
				GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().OnConsume(collision.gameObject);
			}

			// Increase the size of the circle based on the mass added from the one being consumed. 
			Debug.Log("Another circle was absorbed.");
			float targetMass = rb.mass + collision.gameObject.GetComponent<Rigidbody2D>().mass;
			// This should calculate how much bigger this circle needs to be in order to reach targetMass.
			float newRadius = Mathf.Sqrt(targetMass / Mathf.PI);
			// (myCollider.radius * myTransform.localScale.x) should get the true radius, rather than it always being 1.925 or whatever it is set to at the beginning.
			float scale = newRadius / (myCollider.radius * myTransform.localScale.x);
			myTransform.localScale = myTransform.localScale * scale;
			myMagnitude = transform.localScale.magnitude;
			Destroy(collision.gameObject);
			StartCoroutine(OnConsume(targetMass, 0.05f)); // Log mass after 50ms

			if (isPlayer)
			{
				// Adjust camera and zoom values for the player when consuming another circle.
				float originalCameraSize = myCamera.orthographicSize;
				myCamera.orthographicSize = myTransform.localScale.magnitude / cameraToCircleRatio; 
				float camSizeDifference = myCamera.orthographicSize - originalCameraSize;
				if (maxZoom < absoluteMaxZoom)
				{
					minZoom += camSizeDifference;
					maxZoom = minZoom * zoomRatio;
					if (maxZoom > absoluteMaxZoom)
					{
						maxZoom = absoluteMaxZoom;
					}
				}
			}
			if (!isPlayer)
			{
				// This tells the AI script to find a new target since the old one was just destroyed.
				// For some reason calling GetTarget() directly here doesn't work properly. 
				myAI.targetDead = true;
			}
		}
		else if (collision.gameObject.tag == "Circle" && myTransform.localScale.magnitude - otherTransform.localScale.magnitude < absorptionThreshold)
		{
			Debug.Log("This circle isn't big enough to absorb the other one.");
			if (debugLogging)
			{
				Debug.Log(myTransform.localScale.magnitude - otherTransform.localScale.magnitude);
			}
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		// If two circles of similar size touch each other and one of them becomes big enough to consume the other afterwards,
		// then this should cause them to be consumed properly.
		OnCollisionEnter2D(collision);
	}

	// Not included in the OnCollisionEnter because it wasn't logging an accurate actualMass.
	// As you might expect, this runs after another circle is consumed by this circle.
	private IEnumerator OnConsume(float targetMass, float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		if (debugLogging)
		{
			Debug.Log("targetMass: " + targetMass + " actualMass: " + rb.mass);
		}
	}

	private void FixedUpdate()
	{
		if (!isPlayer && Time.time < myAI.startDelay) // Give it time to actually find a target.
		{
			return;
		}

		if (isPlayer) // Allow only the circle controlled by the player to move based on input. Everything else should run off of an AI.
		{
			if (movementType == MovementType.Basic)
			{
				rb.velocity += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * speed;
			}
			else if (movementType == MovementType.Rocket)
			{
				Vector2 forwardSpeed = Vector2.up * Input.GetAxis("Vertical") * speed * Mathf.Pow(rb.mass, 1.1f);
				if (forwardSpeed.y < 0) // Rockets cannot go backwards, that would be silly.
				{
					forwardSpeed.y = 0f;
				}
				rb.AddRelativeForce(forwardSpeed);
				//if (Input.GetAxis("Vertical") < 0f) // Pressing the down key should slow the player down.
				//{
				//	rb.AddRelativeForce(new Vector2(0, Input.GetAxis("Vertical") / 2) * speed * Mathf.Pow(rb.mass, 1.1f));
				//}
				if (Input.GetAxis("Horizontal") != 0f) // Only rotate if player is pressing the keys
				{
					rb.AddTorque(-Mathf.Sign(Input.GetAxis("Horizontal")) * rotateSpeed * Mathf.Pow(rb.mass, 1.1f));
				}
			}
		}
		else if (!myAI.targetReached)
		{
			if (movementType == MovementType.Basic)
			{
				rb.velocity += myAI.GetDirection() * speed;
				rb.rotation = myAI.GetTargetAngle();
			}
			else if (movementType == MovementType.Rocket)
			{
				rb.AddRelativeForce(Vector2.up * speed * Mathf.Pow(rb.mass, 1.1f));
				if (rb.rotation > myAI.GetTargetAngle())
				{
					rb.AddTorque(-rotateSpeed * Mathf.Pow(rb.mass, 1.1f));
				}
				else if (rb.rotation < myAI.GetTargetAngle())
				{
					rb.AddTorque(rotateSpeed * Mathf.Pow(rb.mass, 1.1f));
				}
			}
		}
		else if (myAI.targetReached)
		{
			myAI.GetTarget();
		}
	}

	private void Update()
	{
		// Camera zoom for player
		if (isPlayer)
		{
			// Clamp the camera size between min and max zoom levels
			if (myCamera.orthographicSize < minZoom || myCamera.orthographicSize > maxZoom)
			{
				myCamera.orthographicSize = Mathf.Clamp(myCamera.orthographicSize, minZoom, maxZoom);
			}
			// Zoom in or out with the scroll wheel
			if (myCamera.orthographicSize > minZoom && Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				myCamera.orthographicSize -= 0.5f;
			}
			if (myCamera.orthographicSize < maxZoom && Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				myCamera.orthographicSize += 0.5f;
			}
			// Adjust ratios if the player zoomed in or out
			if (Input.GetAxis("Mouse ScrollWheel") != 0f) // There is an analog dead zone setting which should deal with floating point error here.
			{
				cameraToCircleRatio = myTransform.localScale.magnitude / myCamera.orthographicSize;
			}
		}
	}
}

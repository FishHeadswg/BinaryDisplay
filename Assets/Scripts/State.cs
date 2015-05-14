/*
Author: Trevor Richardson
State.cs
03-23-2015

	Tracks the state of the peg, moves it into position, and colors it.
	
 */

using UnityEngine;
using System.Collections;

public class State : MonoBehaviour
{
	// States
	public enum PegState
	{
		Off,
		On
	}
	public PegState currentState;

	// track previous states to prevent anomalous behavior
	public bool onTracker;
	public bool offTracker;

	// Lerp stuff
	float startTime;
	float duration = 2.0f;

	// Set the peg's state (from the GameController)
	public void SetState (PegState newState)
	{
		currentState = newState;
	}

	// Init and enter FSM coroutine
	void Start ()
	{
		renderer.material.color = Color.red;
		offTracker = true;
		StartCoroutine (StateSequence ());
	}

	// main body, tracks the peg's on/off state
	IEnumerator StateSequence ()
	{
		while (true) {
			if (currentState == PegState.Off && !offTracker) {
				yield return StartCoroutine (TransitionOff ());
			} else if (currentState == PegState.On && !onTracker) {
				yield return StartCoroutine (TransitionOn ());
			}
			yield return null;
		}
	}

	// Moves peg to ON position and changes to green
	IEnumerator TransitionOn ()
	{
		renderer.material.color = Color.green;
		startTime = Time.time;
		onTracker = true;
		offTracker = false;
		Vector3 targetPos = new Vector3 (transform.localPosition.x, transform.localPosition.y + 1.0f, transform.localPosition.z);
		while (Vector3.Distance(transform.localPosition, targetPos) > .05f) {
			transform.localPosition = Vector3.Lerp (transform.localPosition, targetPos, (Time.time - startTime)/duration);
			yield return null;
		}
		yield break;
	}

	// Moves peg to OFF position and changes to red
	IEnumerator TransitionOff ()
	{
		renderer.material.color = Color.red;
		startTime = Time.time;
		offTracker = true;
		onTracker = false;
		Vector3 targetPos = new Vector3 (transform.localPosition.x, transform.localPosition.y - 1.0f, transform.localPosition.z);
		while (Vector3.Distance(transform.localPosition, targetPos) > .05f) {
			transform.localPosition = Vector3.Lerp (transform.localPosition, targetPos, (Time.time - startTime)/duration);
			yield return null;
		}
		yield break;
	}

}

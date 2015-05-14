/*
Author: Trevor Richardson
gameController.cs
03-23-2015

	Tracks the state of the peg, moves it into position, and colors it.
	
 */

using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{

	// Pegs
	public GameObject peg1;
	public GameObject peg2;
	public GameObject peg4;
	public GameObject peg8;

	public GUIText text;

	// Tick/Ding audio
	public AudioSource tick;
	public AudioSource ding;

	// track game status
	bool playing;

	// Individual states/events of the game
	public enum GameState
	{
		Init,
		Count,
		Wave,
		End
	}
	GameState currentState;

	// Binary counter
	public int count;

	// Set the game's state
	void SetState (GameState newState)
	{
		currentState = newState;
	}

	// Init and enter main coroutine
	void Start ()
	{
		playing = true;
		SetState (GameState.Init);
		StartCoroutine (GameSequence ());
	}
	
	// Main body, transitions between each event
	IEnumerator GameSequence ()
	{
		while (playing) {
			switch (currentState) {
			case GameState.Init:
				yield return StartCoroutine (InitState ());
				break;
			case GameState.Count:
				yield return StartCoroutine (CountState ());
				break;
			case GameState.Wave:
				yield return StartCoroutine (WaveState ());
				break;
			case GameState.End:
				yield return StartCoroutine (EndState ());
				break;
			}
		}
	}

	// Initialize peg positions/camera
	IEnumerator InitState ()
	{
		// Set counter to 15
		count = 15;
		yield return StartCoroutine (Counter ());

		text.text = "Initializing...";

		// Set up camera
		float targetFOV = camera.fieldOfView - 20.0f;
		float zoomTime = 10.0f;
		float startTime = Time.time;
		while (camera.fieldOfView - targetFOV > 0.01f) {
			camera.fieldOfView = Mathf.Lerp (camera.fieldOfView, targetFOV, (Time.time - startTime) / zoomTime);
			yield return null;
		}
		// Enter next state
		SetState (GameState.Count);
	}

	// Counts from 0 to 15 in binary
	IEnumerator CountState ()
	{
		count = 0;
		// Begin ticker and wait for 4 ticks / 2sec before beginning
		StartCoroutine (Ticker ());
		yield return new WaitForSeconds (2.0f);

		// Count and ding
		while (count <= 15) {
			text.text = "Counting: " + count;
			ding.Play ();
			yield return StartCoroutine (Counter ());
			++count;
		}
		// Enter next state
		SetState (GameState.Wave);
		yield return null;
	}

	// Simple ticker that ticks every 0.5s
	IEnumerator Ticker ()
	{
		while (count < 15) {
			tick.Play ();
			yield return new WaitForSeconds (0.5f);
		}
	}

	// Perform a wave with the counter, incrementing/decrenting the pitch 
	// along the way
	IEnumerator WaveState ()
	{
		text.text = "Wave!!!";

		// reset counter
		count = 0;
		yield return StartCoroutine (Counter ());

		// array of each peg's script component
		State[] states = new State[4];
		states [0] = peg1.GetComponent<State> ();
		states [1] = peg2.GetComponent<State> ();
		states [2] = peg4.GetComponent<State> ();
		states [3] = peg8.GetComponent<State> ();

		// iterate through the array, dinging on every ON transition
		for (int i = 3; i >= 0; --i) {
			// no previous peg for first iteration
			if (i == 3) {
				ding.Play ();
				// sets peg to ON and waits for the transition to finish before bringing the next ON
				states [i].SetState (State.PegState.On);
				yield return states [i].StartCoroutine ("TransitionOn");
				// increment pitch
				ding.pitch += 0.25f;
			} else {
				ding.Play ();
				states [i].SetState (State.PegState.On);
				yield return states [i].StartCoroutine ("TransitionOn");
				// flip previous peg to OFF after the current peg has moved into ON position
				states [i + 1].SetState (State.PegState.Off);
				if (i == 0)
					yield return states [i + 1].StartCoroutine ("TransitionOff");
				ding.pitch += 0.25f;
			}
		}
		// wave in reverse
		for (int i = 0; i <= 3; ++i) {
			if (i == 0) {
				// offset extra pitch increment from previous loop
				ding.pitch -= 0.25f;
				ding.Play ();
				ding.pitch -= 0.25f;
			}
			else {
				ding.Play ();
				states [i].SetState (State.PegState.On);
				yield return states [i].StartCoroutine ("TransitionOn");
				states [i - 1].SetState (State.PegState.Off);
				if (i == 3)
					yield return states [i - 1].StartCoroutine ("TransitionOff");
				ding.pitch -= 0.25f;
			}
		}
		// set final peg to OFF and transition to last event
		states [3].SetState (State.PegState.Off);
		SetState (GameState.End);
	}

	// final state, simply displays Goodbye text
	IEnumerator EndState ()
	{
		playing = false;
		text.text = "Goodbye!";
		yield break;
	}

	// Sets each peg into the correct position, once every second / 2 ticks
	IEnumerator Counter ()
	{
		if ((count & 1) == 1)
			peg1.SendMessage ("SetState", State.PegState.On);
		else
			peg1.SendMessage ("SetState", State.PegState.Off);
		if ((count & 2) == 2)
			peg2.SendMessage ("SetState", State.PegState.On);
		else
			peg2.SendMessage ("SetState", State.PegState.Off);
		if ((count & 4) == 4)
			peg4.SendMessage ("SetState", State.PegState.On);
		else
			peg4.SendMessage ("SetState", State.PegState.Off);
		if ((count & 8) == 8)
			peg8.SendMessage ("SetState", State.PegState.On);
		else
			peg8.SendMessage ("SetState", State.PegState.Off);
		yield return new WaitForSeconds (1.0f);
	}

}
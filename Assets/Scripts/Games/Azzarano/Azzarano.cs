using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// In this game, Azzarano, we want to display a stimulus (rectangle) for a defined duration.
/// During that duration, the player needs to respond as quickly as possible.
/// Each Trial also has a defined delay to keep the player from guessing.
/// Some appropriate visual feedback is also displayed according to the player's response.
/// </summary>
public class Azzarano : GameBase
{
	const string INSTRUCTIONS = "Press <color=cyan>Spacebar</color> as soon as you see the square.";
	const string FINISHED = "FINISHED!";
	const string RESPONSE_GUESS = "No Guessing!";
	const string RESPONSE_CORRECT = "Good!";
    const string RESPONSE_HITRED = "Don't hit RED!";
	const string RESPONSE_TIMEOUT = "Missed it!";
	const string RESPONSE_SLOW = "Too Slow!";
	Color RESPONSE_COLOR_GOOD = Color.green;
	Color RESPONSE_COLOR_BAD = Color.red;

    int trials = 0;
    int score = 0;

	/// <summary>
	/// A reference to the UI canvas so we can instantiate the feedback text.
	/// </summary>
	public GameObject uiCanvas;
	/// <summary>
	/// The object that will be displayed briefly to the player.
	/// </summary>
	public GameObject stimulus;
	/// <summary>
	/// A prefab for an animated text label that appears when a trial fails/succeeds.
	/// </summary>
	public GameObject feedbackTextPrefab;
	/// <summary>
	/// The instructions text label.
	/// </summary>
	public Text instructionsText;


	/// <summary>
	/// Called when the game session has started.
	/// </summary>
	public override GameBase StartSession(TextAsset sessionFile)
	{
		base.StartSession(sessionFile);

		instructionsText.text = INSTRUCTIONS;
		StartCoroutine(RunTrials(SessionData));

		return this;
	}


	/// <summary>
	/// Iterates through all the trials, and calls the appropriate Start/End/Finished events.
	/// </summary>
	protected virtual IEnumerator RunTrials(SessionData data)
	{
		foreach (Trial t in data.trials)
		{
			StartTrial(t);
			yield return StartCoroutine(DisplayStimulus(t));
			EndTrial(t);
		}
        trials = data.trials.Count;
		FinishedSession();
		yield break;
	}


	/// <summary>
	/// Displays the Stimulus for a specified duration.
	/// During that duration the player needs to respond as quickly as possible.
	/// </summary>
	protected virtual IEnumerator DisplayStimulus(Trial t)
	{
		GameObject stim = stimulus;
        
		stim.SetActive(false);

		yield return new WaitForSeconds(t.delay);

        if(((AzzaranoTrial)t).position == "predefined")
        {
            stim.GetComponent<RectTransform>().localPosition = new Vector3(((AzzaranoTrial)t).positionX * 50, ((AzzaranoTrial)t).positionY * 50, 0);
        }
        else
        {
            float randX = Random.Range(-2, 2);
            float randY = Random.Range(-2, 2);
            stim.GetComponent<RectTransform>().localPosition = new Vector3(randX * 50, randY * 50, 0);
        }

        if (((AzzaranoTrial)t).red == "true")
        {
            stim.GetComponent<Image>().color = new Color(255, 0, 0);
            
        }
        else
        {
            stim.GetComponent<Image>().color = new Color(255, 255, 255);
        }

        StartInput();
		stim.SetActive(true);

		yield return new WaitForSeconds(((AzzaranoTrial)t).duration);
		stim.SetActive(false);
		EndInput();

		yield break;
	}


	/// <summary>
	/// Called when the game session is finished.
	/// e.g. All session trials have been completed.
	/// </summary>
	protected override void FinishedSession()
	{
        GUILog.Log("Score: {0} / {1}", score, trials);
        base.FinishedSession();
		instructionsText.text = FINISHED;
        instructionsText.text += "\nScore: " + score + "/" + trials;
            
	}


	/// <summary>
	/// Called when the player makes a response during a Trial.
	/// StartInput needs to be called for this to execute, or override the function.
	/// </summary>
	public override void PlayerResponded(KeyCode key, float time)
	{
		if (!listenForInput)
		{
			return;
		}
		base.PlayerResponded(key, time);
		if (key == KeyCode.Space)
		{
			EndInput();
			AddResult(CurrentTrial, time);
		}
	}


	/// <summary>
	/// Adds a result to the SessionData for the given trial.
	/// </summary>
	protected override void AddResult(Trial t, float time)
	{
		TrialResult r = new TrialResult(t);
		r.responseTime = time;
		if (time == 0)
		{
            if (((AzzaranoTrial)t).red == "false")
            {
                // No response.
                DisplayFeedback(RESPONSE_TIMEOUT, RESPONSE_COLOR_BAD);
                GUILog.Log("Fail! No response!");
            }
            else
            {
                // Responded correctly.
                DisplayFeedback(RESPONSE_CORRECT, RESPONSE_COLOR_GOOD);
                r.success = true;
                r.accuracy = GetAccuracy(t, time);
                GUILog.Log("Success! responseTime = {0}", time);
                score += 1;
            }
                
		}
		else
		{
			if (IsGuessResponse(time))
			{
				// Responded before the guess limit, aka guessed.
				DisplayFeedback(RESPONSE_GUESS, RESPONSE_COLOR_BAD);
				GUILog.Log("Fail! Guess response! responseTime = {0}", time);
			}
			else if (IsValidResponse(time))
			{
                if(((AzzaranoTrial)t).red == "false")
                {
                    // Responded correctly.
                    DisplayFeedback(RESPONSE_CORRECT, RESPONSE_COLOR_GOOD);
                    r.success = true;
                    r.accuracy = GetAccuracy(t, time);
                    GUILog.Log("Success! responseTime = {0}", time);
                    score += 1;
                }
                else
                {
                    // Responded correctly.
                    DisplayFeedback(RESPONSE_HITRED, RESPONSE_COLOR_BAD);
                    //r.success = true;
                    //r.accuracy = GetAccuracy(t, time);
                    GUILog.Log("Fail! Not supposed to respond to RED");
                }
				
			}
			else
			{
                if (((AzzaranoTrial)t).red == "false")
                {
                    // Responded too slow.
                    DisplayFeedback(RESPONSE_SLOW, RESPONSE_COLOR_BAD);
                    GUILog.Log("Fail! Slow response! responseTime = {0}", time);
                }
                else
                {
                    // Responded correctly.
                    DisplayFeedback(RESPONSE_CORRECT, RESPONSE_COLOR_GOOD);
                    r.success = true;
                    r.accuracy = GetAccuracy(t, time);
                    GUILog.Log("Success! responseTime = {0}", time);
                    score += 1;
                }
                    
			}
		}
		sessionData.results.Add(r);
	}


	/// <summary>
	/// Display visual feedback on whether the trial has been responded to correctly or incorrectly.
	/// </summary>
	private void DisplayFeedback(string text, Color color)
	{
		GameObject g = Instantiate(feedbackTextPrefab);
		g.transform.SetParent(uiCanvas.transform);
		g.transform.localPosition = feedbackTextPrefab.transform.localPosition;
		Text t = g.GetComponent<Text>();
		t.text = text;
		t.color = color;
	}


	/// <summary>
	/// Returns the players response accuracy.
	/// The perfect accuracy would be 1, most inaccuracy is 0.
	/// </summary>
	protected float GetAccuracy(Trial t, float time)
	{
		AzzaranoData data = sessionData.gameData as AzzaranoData;
		bool hasResponseTimeLimit =  data.ResponseTimeLimit > 0;

		float rTime = time - data.GuessTimeLimit;
		float totalTimeWindow = hasResponseTimeLimit ? 
			data.ResponseTimeLimit : (t as AzzaranoTrial).duration;

		return 1f - (rTime / (totalTimeWindow - data.GuessTimeLimit));
	}


	/// <summary>
	/// Returns True if the given response time is considered a guess.
	/// </summary>
	protected bool IsGuessResponse(float time)
	{
		AzzaranoData data = sessionData.gameData as AzzaranoData;
		return data.GuessTimeLimit > 0 && time < data.GuessTimeLimit;
	}


	/// <summary>
	/// Returns True if the given response time is considered valid.
	/// </summary>
	protected bool IsValidResponse(float time)
	{
		AzzaranoData data = sessionData.gameData as AzzaranoData;
		return data.ResponseTimeLimit <= 0 || time < data.ResponseTimeLimit;
	}
}

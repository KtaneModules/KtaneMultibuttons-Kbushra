using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;

public class SimpleModuleScript : MonoBehaviour {

	public KMAudio audio;
	public KMBombInfo info;
	public KMBombModule module;
	public KMSelectable[] cylinders;
	public KMSelectable cylinderSubmit;
    static int ModuleIdCounter = 1;
    int ModuleId;

	public int ans = 0;
	public int InputAns = 0;
	public int StageCur;
	public int StageLim;

	bool _isSolved = false;
	bool incorrect = false;

	void Awake() {
		ModuleId = ModuleIdCounter++;

		foreach (KMSelectable button in cylinders)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate () { pressedCylinder(pressedButton); return false; };
        }
		cylinderSubmit.OnInteract += delegate () { submit(); return false; };
	}

	void Start ()
	{
		//module.HandlePass ();
		if (info.GetSerialNumberLetters().Any ("BROKE".Contains))
		{
			ans++;
			Log ("Serial number shares a letter with BROKE");
		}  
		if (info.GetSerialNumberLetters().Any ("HELLO".Contains)) 
		{
			ans++;
			Log ("Serial number shares a letter with HELLO");
		}
		if (info.GetPortCount () > 0) 
		{
			ans++;
			Log ("There are more than 0 ports");
		}
		if (info.GetBatteryCount() > 2) 
		{
			ans++;
			Log ("There are more than 2 batteries");
		}
		Log ("Answer set!");
	}

	void pressedCylinder(KMSelectable pressedButton)
	{
		int buttonPosition = new int();
		for(int i = 0; i < cylinders.Length; i++)
		{
			if (pressedButton == cylinders[i])
			{
				buttonPosition = i;
				break;
			}
		}

		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, cylinders[buttonPosition].transform);
		cylinders [buttonPosition].AddInteractionPunch ();
		switch (buttonPosition)
		{
			case 0:
			if(info.GetBatteryCount() < 2)
			{
				incorrect = true;
				Log ("Strike! There are less than 2 batteries.");
			}
			break;
			case 1:
			if(info.GetBatteryCount() < 1)
			{
				incorrect = true;
				Log ("Strike! There are no batteries.");
			}
			break;
		    case 2:
			if (info.GetBatteryCount () > 0) {
				incorrect = true;
				Log ("Strike! There are batteries");
			}
			break;
		}

		if(incorrect)
		{
			incorrect = false;
			module.HandleStrike ();
			InputAns = 0;
		}
		else
		{
			InputAns++;
			Log ("would like an answer");
			Log ("Input increased. Current input: " + InputAns);
		}
	}

	void submit()
	{
		Log ("Submitted: " + InputAns + ", Expecting: " + ans);
		if (InputAns == ans) {
			module.HandlePass ();
			Log ("Solved!");
		}
		else 
		{
			module.HandleStrike ();
			Log ("Striked!");
			InputAns = 0;
		}
	}

	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press <1/2/3> <#> [Presses the specified button '#' times] | !{0} submit [Presses the submit button]";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		if (command.EqualsIgnoreCase("submit"))
		{
			yield return null;
			cylinderSubmit.OnInteract();
			yield break;
		}
		string[] parameters = command.Split(' ');
		if (parameters[0].ToLowerInvariant().StartsWith("press"))
		{
			if (parameters.Length == 1)
			{
				yield return "sendtochaterror Please specify a button and an amount of times to press the button!";
				yield break;
			}
			if (parameters.Length == 2 && parameters[1].EqualsAny("1", "2", "3"))
			{
				yield return "sendtochaterror Please specify an amount of times to press the button!";
				yield break;
			}
			if (parameters.Length == 2 && !parameters[1].EqualsAny("1", "2", "3"))
			{
				yield return "sendtochaterror!f The specified button '" + parameters[1] + "' is invalid!";
				yield break;
			}
			if (parameters.Length > 3)
			{
				yield return "sendtochaterror Too many parameters!";
				yield break;
			}
			if (!parameters[1].EqualsAny("1", "2", "3"))
			{
				yield return "sendtochaterror!f The specified button '" + parameters[1] + "' is invalid!";
				yield break;
			}
			int times = -1;
			if (!int.TryParse(parameters[2], out times))
			{
				yield return "sendtochaterror!f The specified amount '" + parameters[2] + "' is invalid!";
				yield break;
			}
			if (times <= 0)
			{
				yield return "sendtochaterror A button cannot be pressed '" + times + "' times!";
				yield break;
			}
			yield return null;
			for (int i = 0; i < times; i++)
			{
				cylinders[int.Parse(parameters[1]) - 1].OnInteract();
				yield return new WaitForSeconds(.1f);
			}
		}
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		if (InputAns > ans)
		{
			module.HandlePass();
			yield break;
		}
		int validBtn = -1;
		if (info.GetBatteryCount() >= 2)
			validBtn = 0;
		else if (info.GetBatteryCount() > 0)
			validBtn = 1;
		else
			validBtn = 2;
		while (InputAns != ans)
		{
			cylinders[validBtn].OnInteract();
			yield return new WaitForSeconds(.1f);
		}
		cylinderSubmit.OnInteract();
	}

	void Log(string message)
	{
		Debug.LogFormat("[Multi-Buttons #{0}] {1}", ModuleId, message);
	}
}

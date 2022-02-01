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

	public AudioSource playsound;

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
	}

	void pressedCylinder(KMSelectable pressedButton)
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);
		int buttonPosition = new int();
		for(int i = 0; i < cylinders.Length; i++)
		{
			if (pressedButton == cylinders[i])
			{
				buttonPosition = i;
				break;
			}
		}

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
			playsound.Play ();
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
			playsound.Play ();
			module.HandleStrike ();
			Log ("Striked!");
			InputAns = 0;
		}
	}

	/*public void Button1()
	{
		if (info.GetBatteryCount () < 2) 
		{
			module.HandleStrike ();
			Log ("WRONG!");
		}
		else 
		{
			InputAns++;
			Log ("would like an answer");
		}
	}
	public void Button2()
	{
		if (info.GetBatteryCount () < 1) {
			module.HandleStrike ();
			Log ("WRONG!");
		}
		else 
		{
			InputAns++;
			Log ("would like an answer");
		}
	}
	public void Button3()
	{
		if (info.GetBatteryCount () < 1) {
			InputAns++;
			Log ("would like an answer");
		} 
		else 
		{
			module.HandleStrike ();
			Log ("WRONG!");
		}
	}*/

	void Log(string message)
	{
		Debug.LogFormat("[Multi-buttons #{0}] {1}", ModuleId, message);
	}
}

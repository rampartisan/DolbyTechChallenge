using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

// Microphone input script to be attached to an empty game object, written by Alex Baldwin March 2016
// http://www.alexbaldwin.audio/
// It is a combination/update of methods from these two sources.
// http://wiki.unity3d.com/index.php/Mic_Input
// http://answers.unity3d.com/questions/1113690/microphone-input-in-unity-5x.html

// In unity we cannot access the microphone data stream directly, we need to
// write it to an AudioSource (and we therefore need an AudioSource component!)

[RequireComponent(typeof(AudioSource))]

public class MicrophoneInput : MonoBehaviour {

// --- VARIABLE DECLERATIONS --- //

	// We are using an AudioMixer, the microphone needs to be unmutued in order to 
	// get data - however we also don't want to hear it (feedback if through speakers,
	// it is also uneccesary, you can hear your voice!). By using an audio mixer we can keep the mic on but mute the 
	// master output. Create an audiomixer, create a subgroup under master and call it microphone.
	// within the AudioSource component set the output to microphone. Then expose the volume
	// parameter of the master volume group to scripting (click on it then right click volume in inspector)
	// rename it to masterVol

	// Create AudioMixer public field
	public AudioMixer outputMixer;

	// Flags to start/stop microphone
	public bool enable = false;
	public bool disable = false;

	// String to store the device we are using
	public string selectedDevice { get; private set; }

	// Storgae f or microphone capabilities
	private int minFreq,maxFreq;

	// Flag to check that we have chosen a device
	private bool deviceSet = false;

	// The audio source component that we will write microphone data to
	AudioSource input;

	// --- MAIN --- //
	void Start() {
		// Set master volume of AudioMixer to minimum (for this application we never want to hear
		// the output, so more control is not needed)
		outputMixer.SetFloat ("masterVol", -80.0f);
		//Make sure AudioSource is assigned to component and zero its data
		input = GetComponent<AudioSource>();
		input.clip = null;
		input.loop = true;
	}

	void Update() {
		// On each update check to see if microphone should be on/off and set accordingly
		if(enable) {
			micOn ();
		}
		if (disable) {
			micOff ();
		}

		// reset flags
		enable = false;
		disable = false;

	}

	// --- MICROPHONE CONTROL --- //

	// Get capabilties of the microphone we are using
	void getDeviceCaps() {
		Microphone.GetDeviceCaps (selectedDevice, out minFreq, out maxFreq);
		// According to documentation if min-max = 0 then the device supports
		// 44.1khz samp freq.
		if ((minFreq - maxFreq) == 0) {
			maxFreq = 44100;
		}

		print (maxFreq.ToString ());
	}

	// Writes the data from the microphone to the AudioSource
	void micOn() {
		// Make sure that the device isnt allready recording (shouldnt happen, just for safety)
		if (!Microphone.IsRecording (selectedDevice)) {
			// Set AudioSource clip to data from the microphone,looping on, 1 second
			input.clip = Microphone.Start (selectedDevice, true, 1, maxFreq);
			// Empty while to wait for recording to start
			while (!(Microphone.GetPosition (selectedDevice) > 0)) {
			}
			// Play the audio
			input.Play ();
		}
	}

	// Stop playback,stop recording,set audio clip to null
	void micOff() {
		input.Stop ();
		Microphone.End (selectedDevice);
		input.clip = null;

	}

	// --- GUI --- // 

	// Unity inbuilt immediate mode GUI, usually used for debug control but works fine
	// for a simple selector.
	void OnGUI() {
		micSelectGUI((Screen.width/2)-150, (Screen.height/2)-75, 300, 100, 10, -300);
	}

	public void micSelectGUI(float left,float top, float width, float height, float buttontop, float buttonLeft) {
		// if we have more than 1 device and we have not set it yet provide a GUI for choosing
		if (Microphone.devices.Length > 1 && deviceSet == false)
			for (int devIdx = 0; devIdx < Microphone.devices.Length; ++devIdx)
				if (GUI.Button (new Rect (left + ((width + buttonLeft) * devIdx), top + ((height + buttontop) * devIdx), width, height), Microphone.devices [devIdx].ToString ())) {			
					selectedDevice = Microphone.devices [devIdx].ToString ();
					deviceSet = true;
					getDeviceCaps ();
					micOn ();
				}
		// If there is only 1 device, default to it
		if (Microphone.devices.Length < 2 && deviceSet == false) {
			print ("using default device");
			selectedDevice = Microphone.devices [0].ToString ();
			deviceSet = true;
			getDeviceCaps ();
			micOn ();

		}
			
	}

	// --- DATA ACCESS --- //

	// Returns array[blocksize] of samples from the microphone, this is the core
	// output of this script and is called by the analysis scripts.
	public float[] getSamps(int blockSize) {
		float[] data = new float[blockSize];
		input.GetOutputData (data, 0);
		return data;
	}
}
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

// Microphone input script to be attached to an empty game object
// It is a combination/update of methods from these two sources. All comments my own.
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
	// The audio source component that we will write microphone data to
	AudioSource micInput;
	// Flags to start/stop microphone and mute
	public bool enable = false;
	public bool disable = false;
	public bool muteAudio = true;
	// Field to set the length of the recording (audioSource) in seconds
	public int recSize = 1;

	// String to store the device we are using
	private string selectedDevice;
	// Storgae of microphone capabilities
	private int minFreq,maxFreq;
	// Flag to check that we have chosen a device
	private bool deviceSet = false;
	// Flag to indicate current state of the microphpone
	private bool currentState = false;

	// --- MAIN --- //
	void Start() {
		// Set mixer volume dependent on public feild muteOutput
		setMixerMute();
		//Make sure AudioSource is assigned to component, make sure we have an empty audio clip and set it to loop
		micInput = GetComponent<AudioSource>();
		micInput.clip = null;
		micInput.loop = true;
	}

	void Update() {
		// On each update check to see if microphone should be on/off or output should be mutedand set accordingly
		if(enable) {
			micOn ();
		}
		if (disable) {
			micOff ();
		}
		// We only need to set on/off once per action so reset the flags
		enable = false;
		disable = false;
		setMixerMute ();
	}

	// --- MICROPHONE CONTROL --- //

	// Get capabilties of the microphone we are using, its name and min/max sample rates
	void getDeviceCaps() {
		Microphone.GetDeviceCaps (selectedDevice, out minFreq, out maxFreq);
		// According to documentation if min-max = 0 then the device supports
		// 44.1khz samp freq. (this is a windows sepecific thing)
		if ((minFreq - maxFreq) == 0) {
			maxFreq = 44100;
		}
	}

	// Writes the data from the microphone to the AudioSource (essentialy use it as a circular buffer)
	void micOn() {
		// Make sure that the device isnt allready recording (shouldnt happen, just for safety)
		if (!Microphone.IsRecording (selectedDevice)) {
			// Set AudioSource clip to data from the microphone,looping on
			micInput.clip = Microphone.Start (selectedDevice, true, recSize, maxFreq);
			// Empty while to wait for recording to start
			while (!(Microphone.GetPosition (selectedDevice) > 0)) {
			}
			// Play the audio (needs to be playing in order to get data)
			micInput.Play ();
			currentState = true;
		}
	}

	// Stop playback,stop recording,set audio clip to null
	void micOff() {
		micInput.Stop ();
		Microphone.End (selectedDevice);
		micInput.clip = null;
		currentState = false;

	}

	// Method to set the AudioMixer volume (default is off, we dont really need to hear our voices)
	void setMixerMute() {
		if (muteAudio) {
			outputMixer.SetFloat ("masterVol", -80.0f);
		} else {
			outputMixer.SetFloat ("masterVol", 0.0f);
		}
	}

	// --- GUI --- // 

	// Unity inbuilt immediate mode GUI, usually used for debug control but works fine
	// for a simple selector.
	void OnGUI() {
		micSelectGUI((Screen.width/2)-150, (Screen.height/2)-75, 300, 100, 10, -300);
	}
		
	public void micSelectGUI(float left,float top, float width, float height, float buttontop, float buttonLeft) {
		// if we have more than 1 device and we have not set it yet provide a GUI for choosing
		if (Microphone.devices.Length > 1 && deviceSet == false) {
			// Iterate over each device, creating button to allow it to be selected
			for (int devIdx = 0; devIdx < Microphone.devices.Length; ++devIdx) {
				// When a device is chosed set it as the selected device,set the selected flag, get its capabilities and then start the microphone
				if (GUI.Button (new Rect (left + ((width + buttonLeft) * devIdx), top + ((height + buttontop) * devIdx), width, height), Microphone.devices [devIdx].ToString ())) {			
					selectedDevice = Microphone.devices [devIdx].ToString ();
					deviceSet = true;
					getDeviceCaps ();
					micOn ();
				}
			}
		}
		// If there is only 1 device, default to it - set it as selected device, set chosen flag, get its capabilities and start the microphone
		if (Microphone.devices.Length < 2 && deviceSet == false) {
			selectedDevice = Microphone.devices [0].ToString ();
			deviceSet = true;
			getDeviceCaps ();
			micOn ();
		}
	}
		
	// --- ACCESSORS --- //

	// Returns array[blocksize] of samples from the microphone
	public float[] getSamps(int blockSize, int offset) {
		float[] data = new float[blockSize];
		micInput.GetOutputData (data, offset);
		return data;
	}

	// Returns the position in the buffer the microphone is currently recording at, needed to only reference
	// the last n samples of audio
	public int micPos() {
		return Microphone.GetPosition (selectedDevice);
	}

	// Returns the sample rate of the device we are currently using
	public int sampleRate() {
		return maxFreq;
	}

	// Check if device has been set (otherwise variables such as sample rate will be invalid)
	public bool deviceReady() {
		return deviceSet;
	}

	// Get current state of device
	public bool getState() {
		return currentState;
	}

	// Get name of currently used device
	public string getName() {
		return selectedDevice;
	}

// --- END ALL --- //
}
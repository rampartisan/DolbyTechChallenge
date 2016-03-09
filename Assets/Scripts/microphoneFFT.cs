using UnityEngine;
using System.Collections;

// Calculate the spectral magnitudes of the microphone input, Accessor getFFT() is the main return of this code

public class microphoneFFT : MonoBehaviour {
	// Public feild to set size of FFT window
	public int windowSize = 1024;

	// Handle onto mic input
	private MicrophoneInput micInput;

	// --- MAIN --- //
	void Start () {
		// MicrophoneInput component on the game object
		micInput = GetComponent<MicrophoneInput> ();	
	}
	
	void Update () {
	}

	// --- FFT --- //
	private float[] getSamples() {
		float[] samps = new float[windowSize];
		int offset = micInput.micPos();
		samps = micInput.getSamps (windowSize, offset);
		micInput.get
		return samps;
	}

	// Returns array, FFT of the last <WindowSize> samples. Luckily for this application we are only concerned 
	// with the magnitude of each freqreqncy bin and dont need the phase, so we can save a bit of computation time and
	// do not have to deal with storing complex numbers!
	public float[] getFFT() {
		// Get the most recent samples from the microphone
		float[] samps = getSamples ();
		// Empty array to store our magnitudes
		float[] FFT = new float[windowSize];	
		// itterate over each frame and each sample within the frame.
		for (int frameIdx = 0; frameIdx < windowSize; frameIdx++) {
			for (int sampleIdx = 0; sampleIdx < windowSize; sampleIdx++) {	
				// frequency of the component in radians 
				float theta = -2 * Mathf.PI * frameIdx * (sampleIdx / windowSize);
				// sin theta for real component (if at somepoint we want phase, use Cos for the imaginary part)
				FFT [frameIdx] += samps [sampleIdx] * Mathf.Exp (Mathf.Sin (theta));
			}
		}
		// Return the array of magnitudes
		return FFT;
	}

// --- END --- //
}

using UnityEngine;
using System.Collections;
using ComplexMath;

// Calculate the spectral magnitudes of the microphone input, Accessor getFFT() is the main return of this code
// Dependent on Complex.cs, a script that 
public class microphoneFFT : MonoBehaviour {
	// Public feild to set size of FFT window
	public int windowSize = 1024;

	// Handle onto MicrophoneInput gameobject
	private MicrophoneInput micInput;
	// Length of buffer in samples, set to 44100 (or anything) for safety, avoiding divide by 0
	private int buffLen = 44100;

	// --- MAIN --- //
	void Start () {
		// MicrophoneInput component on the game object
		micInput = GetComponent<MicrophoneInput> ();
		// Start a routine that waits until the device has been set then calculates the buffer size in samples
		StartCoroutine(waitForDevice());
	}

	IEnumerator waitForDevice(){
		// If the device has been selected / is ready, work out the length in samples
		// of the recording (sample rate (Hz) * length in seconds)
		if (micInput.deviceReady()) {
			buffLen = micInput.sampleRate () * micInput.recSize;
		} else {
			// If the device hasnt been set, wait one second and check again (recursive)
			yield return new WaitForSeconds (1.0f);
			StartCoroutine(waitForDevice());
		}
	}

	// --- FFT --- //
	private float[] getSamples() {
		float[] samps = new float[windowSize];
		int offset = 0;
		samps = micInput.getSamps (windowSize, offset);
		return samps;
	}

	// Returns spectral magnitude array, FFT of the last <WindowSize> samples. 
	public float[] getFFT() {
		// Get the most recent samples from the microphone
		float[] samps = getSamples ();
		// Empty complex array to store our FFT data
		Complex[] FFT = new Complex[windowSize];
		// Empty double array to store magnitudes
		float[] FFTMags = new float[windowSize];
		// itterate over each bin and each sample within the frame.
		for (int binIdx = 0; binIdx < windowSize; binIdx++) {
			FFT [binIdx] = new Complex (0, 0);
			for (int sampleIdx = 0; sampleIdx < windowSize; sampleIdx++) {	
				Complex temp = new Complex (1,-2 * Mathf.PI * binIdx * sampleIdx/windowSize);
				FFT [binIdx] += (samps [sampleIdx] * Complex.Exp(temp));
			}
			// fill the magnitude array

			FFTMags [binIdx] = (float)FFT[binIdx].Modulus;
		}

		// Return magnitude	
		return FFTMags;
	}
// --- END ALL --- //
}

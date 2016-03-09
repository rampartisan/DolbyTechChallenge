using UnityEngine;
using System.Collections;


public class terrainHeightModification : MonoBehaviour {

// --- VARIABLE DECLERATIONS --- //

	// Handle onto the MicrophoneListener gameobject (public feild, set via inspector)
	public GameObject MicrophoneObject;
	// Handle onto mic input component
	private MicrophoneInput micInput;
	// Handle onto FFT component
	private microphoneFFT micFFT;
	// Handle onto terrain collider
	private Terrain terrain;
	// Array to store FFT
	private float[] spectralMag;
	// Store fft windowsize
	private int windowSize;
	private int xRes,zRes;
	private int frameCount = 0;

	// --- MAIN --- //

	void Start () {
		// Get components
		micInput = MicrophoneObject.GetComponent<MicrophoneInput> (); 
		micFFT = MicrophoneObject.GetComponent<microphoneFFT> (); 
		terrain = GetComponent<Terrain> ();
		windowSize = micFFT.windowSize;
		xRes = terrain.terrainData.heightmapWidth;
		zRes = terrain.terrainData.heightmapHeight;
	}
	
	void Update () {
		editTerrain ();
	}

	// --- TERRAIN MODIFICATIONS --- //

	void editTerrain() {
		// Get most recent FFT data
		float[] mags = micFFT.getFFT ();
		// Get current heights
		float[,] heights = terrain.terrainData.GetHeights (0, 0, xRes, zRes);
		// itterate over terrain and set the heights of all x for current z (frameCount)
		for (int xIdx = 0; xIdx < xRes; xIdx++) {
			heights [xIdx, frameCount] = mags[xIdx];
		}

		terrain.terrainData.SetHeightsDelayLOD (0, 0, heights);
		frameCount = (frameCount+1)%(xRes-1);
}
// --- END ALL --- //
}

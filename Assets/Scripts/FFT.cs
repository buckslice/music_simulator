using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioListener))]
public class FFT : MonoBehaviour
{
	private float[] freqData = new float[8192];
	
	public float[] band;

	private float prevTime;
	
    public static FFT thi; //hackathon linking workaround

	void Start()
	{
        thi = this;
		prevTime = Time.time;
		int n = freqData.Length;
		int k = 3;
		band = new float[k];    
	}

	void Update()
	{
			check ();
	}
	
	void check()
	{

        freqData = AudioListener.GetSpectrumData(8192, 0, FFTWindow.Rectangular);

        for (int i = 2; i < 10; i++) {
            band[0] += freqData[i];
        }
        band[0] /= 8;

        for (int i = 11; i < 28; i++) {
            band[1] += freqData[i];
        }
        band[1] /= 17;

        for (int i = 30; i < 150; i++) {
            band[2] += freqData[i];
        }
        band[2] /= 120;

        band[0] *= 4;   // low freq
        band[1] *= 4;   // main beat
        band[2] *= 8;   // high freq
    }
}
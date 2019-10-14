using UnityEngine;
using System.IO;

namespace Obel.SpeechKitTool
{
    public class AudioConverter : MonoBehaviour
    {
        public AudioSource audioSource;

        public bool showDebugMessages;

        private float[] PCM2Floats(byte[] bytes)
        {
            float max = -(float) System.Int16.MinValue;
            float[] samples = new float[bytes.Length / 2];

            for (int i = 0; i < samples.Length; i++)
            {
                short int16sample = System.BitConverter.ToInt16(bytes, i * 2);
                samples[i] = (float) int16sample / max;
            }

            return samples;
        }

        public void Convert(string fullFileName)
        {
            if (showDebugMessages) Debug.Log("[AudioConverter] starting: " + fullFileName);

            byte[] pcmBinary = File.ReadAllBytes(fullFileName);
            float[] audioBynary = PCM2Floats(pcmBinary);

            audioSource.clip = AudioClip.Create("MyPlayback", pcmBinary.Length / 2, 1, 48000, false);
            audioSource.clip.SetData(audioBynary, 0);
            audioSource.Play();
        }
    }
}
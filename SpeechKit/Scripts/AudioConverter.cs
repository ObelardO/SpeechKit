using UnityEngine;
using System.IO;

namespace Obel.SpeechKitTool
{
    public class AudioConverter : MonoBehaviour
    {
        #region Public properties

        public AudioSource audioSource;

        #endregion

        #region Debug properties

        public bool showDebugMessages;

        #endregion

        #region Public methods

        public void Convert(byte[] pcmBinary)
        {
            if (showDebugMessages) Debug.Log("[AudioConverter] starting: " + pcmBinary.Length);

            CreateAudiClip(pcmBinary);
        }

        public void Convert(string fullFileName)
        {
            if (showDebugMessages) Debug.Log("[AudioConverter] starting: " + fullFileName);

            CreateAudiClip(File.ReadAllBytes(fullFileName));
        }

        #endregion

        #region Private methods

        private void CreateAudiClip(byte[] pcmBinary)
        {
            float[] audioBynary = PCM2Floats(pcmBinary);

            audioSource.clip = AudioClip.Create("MyPlayback", pcmBinary.Length / 2, 1, 48000, false);
            audioSource.clip.SetData(audioBynary, 0);
            audioSource.Play();
        }

        private float[] PCM2Floats(byte[] bytes)
        {
            float max = -(float)System.Int16.MinValue;
            float[] samples = new float[bytes.Length / 2];

            for (int i = 0; i < samples.Length; i++)
            {
                short int16sample = System.BitConverter.ToInt16(bytes, i * 2);
                samples[i] = (float)int16sample / max;
            }

            return samples;
        }

        #endregion
    }
}
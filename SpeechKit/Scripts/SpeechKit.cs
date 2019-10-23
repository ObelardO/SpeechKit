using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Events;

namespace Obel.SpeechKitTool
{
    public class SpeechKit : MonoBehaviour
    {
        #region Synthesis properties

        public enum SPLang { ru, en, tr }
        [SerializeField, Header("Voice")] private SPLang lang;
        private string[] languages = new string[] { "ru-RU", "en-US", "tr-TR" };
        public string Lang => languages[(int)lang];

        public enum SPVoice { oksana, alyss, jane, zahar, ermil }
        [SerializeField] private SPVoice voice;
        private string[] voices = new string[] { "oksana", "alyss", "jane", "zahar", "ermil" };
        public string Voice => voices[(int)voice];

        public enum SPEmotion { neutral, evil, good }
        [SerializeField] private SPEmotion emotion;
        private string[] emotions = new string[] { "neutral", "evil", "good" };
        public string Emotion => emotions[(int)emotion];

        public enum SPSpeed { x1, x1_15, x1_25, x1_5, x2_0, x0_9, x0_8, x0_7, x0_6, x0_5  }
        [SerializeField] private SPSpeed speed;
        private string[] speeds = new string[] { "1.0", "1.15", "1.25", "1.5", "2.0", "0.9", "0.8", "0.7", "0.6", "0.5" };
        public string Speed => speeds[(int)speed];

        public enum SPCodec { PCM, opus }
        [SerializeField] private SPCodec codec;
        private string[] codecs = new string[] { "lpcm", "oggopus" };
        public string Codec => codecs[(int)codec];

        #endregion

        #region Input and Output properties

        [SerializeField, Space, TextArea] private string textToVoice;
        public string TextToVoice { set => textToVoice = value; get => textToVoice; }

        public enum StorageMode { files, memory }
        [Tooltip("Store files and use onDoneFiles event or pass received bytes to onDoneBytes event")]
        public StorageMode storageMode;

        #endregion

        #region API connection properties

        public enum AuthMode { IAMToken, APIKey }
        [Header("API")] public AuthMode authMode;

        [SerializeField, Tooltip("Paste here your IAMToken or API-Key if you using service account")]
        private string authCode;
        public string AuthCode { set => authCode = value; get => authCode; }

        [SerializeField, Tooltip("Paste here folder ID if you using IAMToken for authorization. Keep clear if not")]
        private string folderID;
        public string FolderID { set => folderID = value; get => folderID; }

        #endregion

        #region Result events

        [Serializable] public class UnityEventString : UnityEvent<string> { }
        [Serializable] public class UnityEventBytes : UnityEvent<byte[]> { }

        [Header("Events")]
        public UnityEventString onDoneFile;
        public UnityEventBytes onDoneBytes;
        public UnityEvent onError;

        #endregion

        #region Debug properties

        [Space]
        public bool showDebugMessages;

        #endregion

        #region Public methods

        public void TryToSpeak(string fileName) => Speech(fileName);

        public void TryToSpeak() => Speech(null);

        #endregion

        #region Private methods

        private async void Speech(string fileName)
        {
            if (showDebugMessages) Debug.Log("[SpeechKit] Try to speak: " + TextToVoice + " File: " + fileName);

            string loadedFileName = "";

            HttpClient client = new HttpClient();

            try
            {
                if (showDebugMessages) Debug.Log("[SpeechKit] Authorization");

                client.DefaultRequestHeaders.Add("Authorization", (authMode == AuthMode.APIKey ? "Api-Key " : "Bearer ") + AuthCode);

                var values = new Dictionary<string, string>
                {
                    { "text", TextToVoice },
                    { "lang", Lang },
                    { "speed", Speed },
                    { "emotion", Emotion },
                    { "voice", Voice },
                    { "format", Codec },
                };

                if (authMode == AuthMode.IAMToken) values.Add("folderId", FolderID);
                if (codec == SPCodec.PCM) values.Add("sampleRateHertz", "48000");

                var content = new FormUrlEncodedContent(values);

                if (showDebugMessages) Debug.Log("[SpeechKit] Response...");
                var response = await client.PostAsync("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize", content);

                if (showDebugMessages) Debug.Log("[SpeechKit] Content...");
                var responseBytes = await response.Content.ReadAsByteArrayAsync();

                if (showDebugMessages) Debug.Log("[SpeechKit] Precess data...");

                if (storageMode == StorageMode.files)
                {
                    string path = Path.Combine(Application.streamingAssetsPath, "SpeechKit");

                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    if (string.IsNullOrEmpty(fileName)) fileName = $"[{DateTime.Now.ToString().Replace(":", ".").Replace(" ", "_")}][{lang}][{voice}][{emotion}][{speed}].ogg";

                    try
                    {
                        loadedFileName = Path.Combine(path, fileName);
                        if (showDebugMessages) Debug.Log("[SpeechKit] FILE: " + loadedFileName);
                        File.WriteAllBytes(loadedFileName, responseBytes);
                        onDoneFile.Invoke(loadedFileName);
                    }
                    catch
                    {
                        if (showDebugMessages) Debug.LogWarning("[SpeechKit] something wrong with file writing!");
                        onError.Invoke();
                    }
                }
                else
                {
                    onDoneBytes.Invoke(responseBytes);
                }
            }
            catch
            {
                if (showDebugMessages) Debug.LogWarning("[SpeechKit] something wrong with API!");
                onError.Invoke();
            }
            finally
            {
                client.Dispose();
                if (showDebugMessages) Debug.Log("[SpeechKit] Done.");
            }
        }

        #endregion
    }
}

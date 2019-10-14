using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Obel.SpeechKitTool
{
    public class SpeechKit : MonoBehaviour
    {
        // https://cloud.yandex.ru/docs/iam/operations/iam-token/create - get IAM TOKEN //

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

        public enum SPSpeed { normal, fast, fastest, slow, slowest }
        [SerializeField] private SPSpeed speed;
        private string[] speeds = new string[] { "1.0", "1.25", "1.5", "0.75", "0.5" };
        public string Speed => speeds[(int)speed];

        [SerializeField, TextArea] private string textToVoice;
        public string TextToVoice { set => textToVoice = value; get => textToVoice; }

        [Header("API"), SerializeField] private string folderID;
        public string FolderID { set => folderID = value; get => folderID; }

        [SerializeField, TextArea] private string iamToken;
        public string IamToken { set => iamToken = value; get => iamToken; }

        [Serializable] public class UnityEventString : UnityEvent<string> { }
        [Header("Events")]
        public UnityEventString onDone;
        public UnityEvent onError;
        private string doneResult = string.Empty;
        private string errorResult = string.Empty;

        [Space]
        public bool showDebugMessages; 

        public void TryToSpeak() => Speech();

        private async void Speech()
        {
            if (showDebugMessages) Debug.Log("[SpeatchKit] Try to speak: " + TextToVoice);

            string loadedFileName = "";

            await Task.Run(async () =>
            {
                HttpClient client = new HttpClient();

                try
                {
                    if (showDebugMessages) Debug.Log("[SpeatchKit] Authorization");
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + iamToken);
                    var values = new Dictionary<string, string>
                    {
                        { "text", TextToVoice },
                        { "lang", Lang },
                        { "folderId", FolderID },
                        { "speed", Speed },
                        { "emotion", Emotion },
                        { "voice", Voice },
                        { "format", "lpcm" },
                        { "sampleRateHertz", "48000" }
                    };
                    var content = new FormUrlEncodedContent(values);
                    if (showDebugMessages) Debug.Log("[SpeatchKit] Response...");
                    var response = await client.PostAsync("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize", content);
                    if (showDebugMessages) Debug.Log("[SpeatchKit] Content...");
                    var responseBytes = await response.Content.ReadAsByteArrayAsync();
                    if (showDebugMessages) Debug.Log("[SpeatchKit] Write file...");

                    string path = Path.Combine(Application.streamingAssetsPath, "SpeatchKit");

                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    loadedFileName = Path.Combine(path, $"[{DateTime.Now.ToString().Replace(":", ".").Replace(" ", "_")}][{lang}][{voice}][{emotion}][{speed}].ogg");

                    try
                    {
                        File.WriteAllBytes(loadedFileName, responseBytes);
                    }
                    catch
                    {
                        if (showDebugMessages) Debug.LogWarning("[SpeatchKit] something wrong file writing!");
                        errorResult = "file error";
                    }
                }
                catch
                {
                    if (showDebugMessages) Debug.LogWarning("[SpeatchKit] something wrong with API!");
                    errorResult = "API error";
                }
                finally
                {
                    client.Dispose();
                    if (showDebugMessages) Debug.Log("[SpeatchKit] Done.");
                    doneResult = loadedFileName;
                }
            });
        }


        private void Update() => processResults();

        private void processResults()
        {
            if (!string.IsNullOrEmpty(doneResult))
            {
                onDone.Invoke(doneResult);
                doneResult = string.Empty;
            }

            if (!string.IsNullOrEmpty(doneResult))
            {
                onError.Invoke();
                errorResult = string.Empty;
            }
        }

    }
}

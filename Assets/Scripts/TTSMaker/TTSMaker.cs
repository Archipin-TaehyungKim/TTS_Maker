using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Ookii.Dialogs;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Application = UnityEngine.Application;
using Button = UnityEngine.UI.Button;
using Screen = UnityEngine.Screen;

public class TTSMaker : MonoBehaviour
{
    [SerializeField] private Dropdown dropDown;
    [SerializeField] private Text curSpeaker;
    [SerializeField] private Text LogTxt;
    [SerializeField] private InputField _inputField;

    [SerializeField] private Image backgroundImg;
    [SerializeField] private Button makeButton;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Image[] images;
    
    
    private void Start()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        dropDown.options.Clear();
        using (var sr = new StreamReader(Application.dataPath + "/Resources/speaker.txt"))
        {
            while (true)
            {
                Dropdown.OptionData option = new Dropdown.OptionData();
                var speaker = sr.ReadLine();
                if (speaker != null)
                {
                    option.text = speaker;
                    dropDown.options.Add(option);
                }
                else
                {
                    break;
                }
            }
        }
    }
    
    private void Update()
    {
        Change_Image();
        
        if (_audioSource.isPlaying)
        {
            backgroundImg.color = new Color(224.0f/255.0f, 66.0f/255.0f, 26.0f/255.0f, 1);
        }
        else
        {
            backgroundImg.color = new Color(224.0f/255.0f, 148.0f/255.0f, 26.0f/255.0f, 1);
            makeButton.interactable = true;
        }
    }

    private IEnumerator MakeTTS(System.Action<AudioClip> lastVoice)
    {
        if (curSpeaker.text == "Vivian" || curSpeaker.text == "Zeppelin") yield return Archipin_TTS(lastVoice, curSpeaker.text);
        else yield return Mindslab_TTS(lastVoice);
    }

    private IEnumerator Mindslab_TTS(System.Action<AudioClip> lastVoice)
    {
        var textAnim = logTextAnim();
        StartCoroutine(textAnim);
        
        const string host = "https://norchestra.maum.ai/harmonize/dosmart";
        var requestData = new NewTTSModel(_inputField.text, curSpeaker.text);
        var requestJson = JsonUtility.ToJson(requestData);
        var bytes = Encoding.UTF8.GetBytes(requestJson);

        var webRequest = new UnityWebRequest(host, UnityWebRequest.kHttpVerbPOST);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("cache-control", "no-cache");
        webRequest.uploadHandler = new UploadHandlerRaw(bytes);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
            webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            StopCoroutine(textAnim);
            lastVoice(null);
            LogTxt.text = webRequest.error.ToString();
        }
        else
        {
            var response = webRequest.downloadHandler.data;
            StopCoroutine(textAnim);
            var wav = new WAV(response);
            var audioClip = AudioClip.Create(_inputField.text.Replace(' ', '_'), wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            lastVoice(audioClip);
        }
    }

    private IEnumerator Archipin_TTS(System.Action<AudioClip> lastVoice, string voiceName)
    {
        var textAnim = logTextAnim();
        StartCoroutine(textAnim);
        
        var host = voiceName == "Vivian" ? "https://wgi02.archipindev.com/voice-generator/" : "https://wgi02.archipindev.com/tts/";
        // var requestDict = new Dictionary<string, string> { {"input_text", _inputField.text} };
        var requestJson = JsonUtility.ToJson(new ArchipinTTSModel()
        {
            input_text = _inputField.text
        });
        var bytes = Encoding.UTF8.GetBytes(requestJson);
        
        var webRequest = new UnityWebRequest(host, UnityWebRequest.kHttpVerbPOST);
        webRequest.uploadHandler = new UploadHandlerRaw(bytes);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        yield return webRequest.SendWebRequest();
        
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
            webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            StopCoroutine(textAnim);
            lastVoice(null);
            LogTxt.text = webRequest.error.ToString();
        }
        else
        {
            var response = webRequest.downloadHandler.data;
            StopCoroutine(textAnim);
            var wav = new WAV(response);
            Debug.Log($"{wav.Frequency}/{wav.SampleCount}/{wav.ChannelCount}");
            var audioClip = AudioClip.Create(_inputField.text.Replace(' ', '_'), wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            lastVoice(audioClip);
        }
    }

    private IEnumerator logTextAnim()
    {
        var count = 0;
        while (true)
        {
            LogTxt.text = count switch
            {
                0 => "Log: Making tts.",
                1 => "Log: Making tts..",
                2 => "Log: Making tts...",
                _ => LogTxt.text
            };

            count += 1;
            if (count >= 3) count = 0;
            
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    public void Toggle_Make()
    {
        makeButton.interactable = false;
        StartCoroutine(MakeTTS(clip =>
        {
            LogTxt.text = "Log:\tSpeaker-> " + curSpeaker.text + "\n\t\tLast Text-> " + _inputField.text;
            _audioSource.clip = clip;
            _audioSource.Play();
        }));
    }

    public void Toggle_Play()
    {
        if (_audioSource.clip != null)  _audioSource.Play();
    }

    public void Toggle_Save()
    {
        if (_audioSource.clip == null) return;

        var filename = Save(curSpeaker.text, _audioSource.clip);
        SavWav.Save(filename, _audioSource.clip);
        LogTxt.text = "Log: " + $"Save complete! -> {filename}.wav";
    }

    private static string Save(string npc, Object audioClip)
    {
        var dialog = new VistaSaveFileDialog();
        
        dialog.Filter = "wav files (*.wav)|*.wav|All files (*.*)|*.*";
        dialog.FilterIndex = 1;
        dialog.Title = "Save audio";
        dialog.FileName = $"{npc}_{audioClip.name}.wav";

        return dialog.ShowDialog() != DialogResult.OK ? "" : dialog.FileName;
    }

    private void Change_Image()
    {
        foreach (var t in images)
        {
            t.gameObject.SetActive(t.gameObject.name == curSpeaker.text);
        }
    }
}

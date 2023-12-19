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

/// <summary>
/// This script makes tts one by one.
/// </summary>
public class TTSMaker : MonoBehaviour
{
    [SerializeField] private Dropdown dropDown;
    [SerializeField] private Text server;
    [SerializeField] private Text curSpeaker;
    [SerializeField] private InputField _input_text;

    [SerializeField] private Image backgroundImg;
    [SerializeField] private Button makeButton;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Image[] images;
    [SerializeField] private InputField _speed;
    
    [SerializeField] private LogManager _logManager;

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

        _speed.text = "1.0";
    }
    
    private void Update()
    {
        // NPC image
        Change_Image();
        
        // for announcement
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
        if (server.text == "Archipin")
        {
            yield return TTSManager.ArchipinTTS(
                input_text:_input_text.text, 
                voiceName:curSpeaker.text, 
                lastVoice:lastVoice,
                logMgr:_logManager,
                speed:float.Parse(_speed.text));
                
        }
        else
            yield return TTSManager.MindslabTTS(
                input_text:_input_text.text, 
                voiceName:curSpeaker.text, 
                lastVoice:lastVoice,
                logMgr:_logManager);
    }

    public void Toggle_Make()
    {
        makeButton.interactable = false;
        StartCoroutine(MakeTTS(clip =>
        {
            _logManager.ChangeLogText(
                "Log:\tSpeaker-> " + curSpeaker.text + "\n\t\tLast Text-> " + _input_text.text
                );
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
        _logManager.ChangeLogText("Log: " + $"Save complete! -> {filename}.wav");
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

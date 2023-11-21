using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UI;
using Ookii.Dialogs;
using Application = UnityEngine.Application;
using Screen = UnityEngine.Screen;

public abstract class CSVReader
{
    public static List<Tuple<string, string>> Read(string fileUrl)
    {
        var sr = new StreamReader(fileUrl);
        var sources = sr.ReadToEnd();
        sr.Close();

        var lines = sources.Split('\n');
        var readData = new List<Tuple<string, string>>();

        for (var i = 1; i < lines.Length; i++)
        {
            var data = lines[i].Split('\t');
            var filepath = data[0];
            var sentence = data[1].Replace("\n", "");

            if (sentence == "" || filepath == "")
                continue;
            
            readData.Add(new Tuple<string, string>(sentence, filepath));
        }
        
        return readData;
    }
}


public class AutoTTSMaker : MonoBehaviour
{
    [SerializeField] private Text curSpeaker;
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private Text contentPrefab;
    [SerializeField] private Dropdown dropDown;

    private Vector2 initPos = new Vector2(80, -15);
    private static int createCount = 0;

    private List<Tuple<string, string>> data;

    private void Start()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        dropDown.options.Clear();
        using var sr = new StreamReader(Application.dataPath + "/Resources/speaker.txt");
        while (true)
        {
            var option = new Dropdown.OptionData();
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

    private void LoadCSV()
    {
        var dialog = new VistaOpenFileDialog();
        
        dialog.Filter = "tsv files (*.tsv) |*.tsv|csv files (*.csv)|*.csv|All files (*.*)|*.*";
        dialog.FilterIndex = 3;
        dialog.Title = "Select tsv file";
        
        if (dialog.ShowDialog() != DialogResult.OK) return;
        if (dialog.OpenFile() == null) return;
        
        data = CSVReader.Read(dialog.FileName);
        CreateLog("\"" + dialog.FileName + "\" data loaded!, Total: " + data.Count);
    }

    public void Toggled_Load()
    {
        LoadCSV();
    }

    private IEnumerator MAKE_TTS()
    {
        CreateLog("TTS making start!");

        for (var i = 0; i < data.Count; i++)
        {
            CreateLog($"{i + 1}/{data.Count} making... speaker: {curSpeaker.text}, sentence: {data[i].Item1}");
            if (curSpeaker.text == "Vivian" || curSpeaker.text == "Zeppelin")
                yield return StartCoroutine(TTSManager.Archipin_TTS(
                    data[i].Item1, curSpeaker.text, audioClip =>
                    {
                        SavWav.Save(data[i].Item2 + ".wav", audioClip);
                    }, null, log =>
                    {
                        if (log == "-> Save path:\"")
                            CreateLog("-> Save path:\"" + data[i].Item2 + "\"");
                        else
                            CreateLog(log);
                    }));
            
            else 
                yield return StartCoroutine(TTSManager.Mindslab_TTS(
                    data[i].Item1, curSpeaker.text, audioClip =>
                    {
                        SavWav.Save(data[i].Item2 + ".wav", audioClip);
                    }, null, log =>
                    {
                        if (log == "-> Save path:\"")
                            CreateLog("-> Save path:\"" + data[i].Item2 + "\"");
                        else
                            CreateLog(log);
                    }));
        }
    }

    public void Toggled_Make()
    {
        if (data.Count == 0)
        {
            CreateLog("you need to load data first!");
            return;
        }
        
        StartCoroutine(MAKE_TTS());
    }

    private void CreateLog(string sentence)
    {
        var text = Text.Instantiate(contentPrefab);
        text.transform.SetParent(scrollViewContent.transform);
        text.transform.localPosition = new Vector3(initPos.x, initPos.y + 30 * createCount, 0);
        text.transform.localScale = Vector3.one;
        text.text = sentence;
        createCount += 1;
    }
}

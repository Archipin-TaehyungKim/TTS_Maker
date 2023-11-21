using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Application = UnityEngine.Application;
using Ookii.Dialogs;
using Screen = UnityEngine.Screen;

public class GoogleTTSMakerManager: MonoBehaviour
{
    public List<DataStruct> data;
    [SerializeField] private Text statusText;
    [SerializeField] private Config _config;
    [SerializeField] private Text version;
    
    private void Start()
    {
        _config.SetConfigure();
        version.text = Application.version;

        statusText.text = "Please load tsv file";
        Screen.SetResolution(800, 600, FullScreenMode.Windowed);
    }

    private string LoadCSV()
    {
        var dialog = new VistaOpenFileDialog();
        
        dialog.Filter = "tsv files (*.tsv) |*.tsv|csv files (*.csv)|*.csv|All files (*.*)|*.*";
        dialog.FilterIndex = 3;
        dialog.Title = "Open file";
        
        if (dialog.ShowDialog() != DialogResult.OK) return "";
        if (dialog.OpenFile() == null) return "";
        FileIOUtility.LoadFile(dialog.FileName, d => { data = d; });
        return dialog.FileName;
    }

    public void OnClick_LoadCSV()
    {
        var url = LoadCSV();
        Debug.Log(url);
        if (url != "")
            statusText.text = $"{url} Loaded!";
    }

    public void OnClick_MakeTTS()
    {
        StartCoroutine(Make());
    }

    private IEnumerator Make()
    {
        for (var i = 0; i < data.Count; i++)
        {
            yield return new WaitForSeconds(1.0f);
            statusText.text = $"'{i + 1}/{data.Count}\n{data[i].fileName}' is processing...";
            yield return StartCoroutine(MakeTTS(data[i].fileName, data[i].contents));
        }
    }

    private IEnumerator MakeTTS(string fileName, string context)
    {
        const string host = "https://texttospeech.googleapis.com/v1beta1/text:synthesize?key=AIzaSyC8U86hBZmF3tS-HWhUsWQmgeNTRNNcYws";
        var requestData = new GoogleTTSRequestDTO(
            _config.GetSpeaker(), 
            float.Parse(_config.GetSpeed()),
            float.Parse(_config.GetPitch()),
            context);
        
        Debug.Log(requestData);
        
        var requestJson = JsonUtility.ToJson(requestData);
        var bytes = Encoding.UTF8.GetBytes(requestJson);
    
        var webRequest = new UnityWebRequest(host, UnityWebRequest.kHttpVerbPOST);
        webRequest.uploadHandler = new UploadHandlerRaw(bytes);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
    
        yield return webRequest.SendWebRequest();
    
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
            webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            yield return null;
        }
        else
        {
            var response = webRequest.downloadHandler.text;
            var responseDTO = JsonUtility.FromJson<GoogleTTSResponseDTO>(response);
            // Debug.Log(responseDTO.audioContent.Length);
            var byteArr = Convert.FromBase64String(responseDTO.audioContent);
            // Debug.Log(byteArr.Length);

            var floatArr = new float[byteArr.Length / 2];
            
            for (var i = 0; i < floatArr.Length; i++)
            {
                floatArr[i] = BitConverter.ToInt16(byteArr, i * 2) / 32768.0f;
            }
            
            var audioClip = AudioClip.Create("tts", floatArr.Length, 1, 48000, false);
            audioClip.SetData(floatArr, 0);
            
            SavWav.Save($"{fileName}.wav", audioClip);
            statusText.text += $"\nSuccess!";
        }
    }
}
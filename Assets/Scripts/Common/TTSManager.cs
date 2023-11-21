using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TTSManager
{
    public static IEnumerator Archipin_TTS(string input_text, string voiceName, 
        System.Action<AudioClip> lastVoice=null, LogManager logMgr=null, Action<string> logMsg=null)
    {
        logMgr?.StartLogAnim();

        const string host = "https://wgi02.archipindev.com/tts/";
        var requestJson = JsonUtility.ToJson(new ArchipinTTSModel()
        {
            input_text = input_text,
            npc = voiceName,
            student_id = "tts_maker"
        });
        var bytes = Encoding.UTF8.GetBytes(requestJson);
        
        var webRequest = new UnityWebRequest(host, UnityWebRequest.kHttpVerbPOST);
        webRequest.uploadHandler = new UploadHandlerRaw(bytes);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        yield return webRequest.SendWebRequest();
        
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
            webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            logMgr?.StopLogAnim();
            logMgr?.ChangeLogText(webRequest.error);
            lastVoice?.Invoke(null);
            logMsg?.Invoke("-> ERROR: web request connection or protocol error");
        }
        else
        {
            try
            {
                var response = webRequest.downloadHandler.data;
                logMgr?.StopLogAnim();
                var wav = new WAV(response);
                var audioClip = AudioClip.Create(input_text.Replace(' ', '_'), wav.SampleCount, 1, wav.Frequency, false);
                audioClip.SetData(wav.LeftChannel, 0);
                lastVoice?.Invoke(audioClip);
                logMsg?.Invoke("-> Save path:\"");
            }
            catch (Exception e)
            {
                logMsg?.Invoke($"-> Error: {e}");
            }
        }
    }
    
    public static IEnumerator Mindslab_TTS(string input_text, string voiceName, 
        System.Action<AudioClip> lastVoice=null, LogManager logMgr=null, Action<string> logMsg=null)
    {
        logMgr?.StartLogAnim();
        
        const string host = "https://norchestra.maum.ai/harmonize/dosmart";
        var requestData = new NewTTSModel(input_text, voiceName);
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
            logMgr?.StopLogAnim();
            logMgr?.ChangeLogText(webRequest.error);
            lastVoice?.Invoke(null);
            logMsg?.Invoke("-> ERROR: web request connection or protocol error");
        }
        else
        {
            try
            {
                var response = webRequest.downloadHandler.data;
                logMgr?.StopLogAnim();
                var wav = new WAV(response);
                var audioClip = AudioClip.Create(input_text.Replace(' ', '_'), wav.SampleCount, 1, wav.Frequency, false);
                audioClip.SetData(wav.LeftChannel, 0);
                lastVoice?.Invoke(audioClip);
                logMsg?.Invoke("-> Save path:\"");
            }
            catch (Exception e)
            {
                logMsg?.Invoke($"-> Error: {e}");
            }
        }
    }
}
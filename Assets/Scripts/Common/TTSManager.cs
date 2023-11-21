using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TTSManager
{
    public static IEnumerator ArchipinTTS(string input_text, string voiceName, 
        System.Action<AudioClip> lastVoice=null, LogManager logMgr=null, Action<string> logMsg=null)
    {
        logMgr?.StartLogAnim();

        // dev server's host
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
    
    public static IEnumerator MindslabTTS(string input_text, string voiceName, 
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
    
    public static IEnumerator GoogleTTS(string fileName, string input_text, Config _config, Action<string> msg)
    {
        const string host = "https://texttospeech.googleapis.com/v1beta1/text:synthesize?key=AIzaSyC8U86hBZmF3tS-HWhUsWQmgeNTRNNcYws";
        var requestData = new GoogleTTSRequestDTO(
            _config.GetSpeaker(), 
            float.Parse(_config.GetSpeed()),
            float.Parse(_config.GetPitch()),
            input_text);

        var requestJson = JsonUtility.ToJson(requestData);
        var bytes = Encoding.UTF8.GetBytes(requestJson);
    
        var webRequest = new UnityWebRequest(host, UnityWebRequest.kHttpVerbPOST);
        webRequest.uploadHandler = new UploadHandlerRaw(bytes);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
    
        yield return webRequest.SendWebRequest();
    
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
            webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            msg?.Invoke("");
            yield return null;
        }
        else
        {
            var response = webRequest.downloadHandler.text;
            var responseDTO = JsonUtility.FromJson<GoogleTTSResponseDTO>(response);
            var byteArr = Convert.FromBase64String(responseDTO.audioContent);
            var floatArr = new float[byteArr.Length / 2];
            
            for (var i = 0; i < floatArr.Length; i++)
            {
                floatArr[i] = BitConverter.ToInt16(byteArr, i * 2) / 32768.0f;
            }
            
            var audioClip = AudioClip.Create("tts", floatArr.Length, 1, 48000, false);
            audioClip.SetData(floatArr, 0);
            
            SavWav.Save($"{fileName}.wav", audioClip);
            // statusText.text += $"\nSuccess!";
            msg?.Invoke($"\nSuccess!");
        }
    }
}
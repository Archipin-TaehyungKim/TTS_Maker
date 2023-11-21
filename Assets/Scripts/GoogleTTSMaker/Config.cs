using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Config : MonoBehaviour
{
    [SerializeField] private InputField speaker;
    [SerializeField] private InputField speed;
    [SerializeField] private InputField pitch;
    [SerializeField] private GameObject status;
    [SerializeField] private Text status_text;

    public void OnEnable()
    {
        status.SetActive(false);
    }

    public void SetConfigure()
    {
        try
        {
            var filePath = Application.dataPath + "/config.txt";
            var lines = File.ReadAllLines(filePath);
            
            SetSpeaker(lines[0].Split(':')[1]);
            SetSpeed(lines[1].Split(':')[1]);
            SetPitch(lines[2].Split(':')[1]);
        }
        catch
        {
            SetSpeaker("en-US-Standard-A");
            SetSpeed("1");
            SetPitch("1");
        }
    }

    private void SetSpeaker(string text) { speaker.text = text; }
    public string GetSpeaker() { return speaker.text; }

    private void SetSpeed(string text) { speed.text = text; }
    public string GetSpeed() { return speed.text; }

    private void SetPitch(string text) { pitch.text = text; }
    public string GetPitch() { return pitch.text; }

    private IEnumerator Activate_Status_Bar(string text)
    {
        status.SetActive(true);
        status_text.text = text;

        yield return new WaitForSeconds(3.0f);
        
        status.SetActive(false);
    }
    
    public void OnClick_Save()
    {
        var savePath = Application.dataPath + "/config.txt";
        File.WriteAllText(savePath, 
            $"Speaker:{GetSpeaker()}\n" +
            $"Speed:{GetSpeed()}\n" +
            $"Pitch:{GetPitch()}", Encoding.UTF8);
        
        StartCoroutine(Activate_Status_Bar("Configure saved!"));
    }
}

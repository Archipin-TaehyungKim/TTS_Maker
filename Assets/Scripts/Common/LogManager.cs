using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    [SerializeField] private Text LogTxt;

    private IEnumerator textAnim;

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

    public void StartLogAnim()
    {
        textAnim = logTextAnim();
        StartCoroutine(textAnim);
    }

    public void StopLogAnim()
    {
        StopCoroutine(textAnim);
    }

    public void ChangeLogText(string msg)
    {
        LogTxt.text = msg;
    }
}
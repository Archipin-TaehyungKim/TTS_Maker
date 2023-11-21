using System;

[Serializable]
public class GoogleTTSRequestDTO
{
    public InputClass input = new InputClass();
    public VoiceClass voice = new VoiceClass();
    public AudioConfigClass audioConfig = new AudioConfigClass();

    public GoogleTTSRequestDTO(string speaker, float speed, float pitch, string contents)
    {
        input.text = contents;
        voice.languageCode = $"{speaker.Split('-')[0]}-{speaker.Split('-')[1]}";
        voice.name = speaker;
        audioConfig.pitch = pitch;
        audioConfig.speakingRate = speed;
    }

    public override string ToString()
    {
        return $"{voice.languageCode}/{voice.name}/{audioConfig.speakingRate}/{audioConfig.pitch}/{audioConfig.sampleRateHertz}";
    }
}

[Serializable]
public class InputClass
{
    public string text;
}

[Serializable]
public class VoiceClass
{
    public string languageCode;
    public string name;
}

[Serializable]
public class AudioConfigClass
{
    public string audioEncoding = "LINEAR16";
    public float pitch;
    public float speakingRate;
    public int sampleRateHertz = 48000;
}


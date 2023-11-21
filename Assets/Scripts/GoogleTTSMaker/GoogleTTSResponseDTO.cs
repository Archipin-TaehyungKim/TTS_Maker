using System;

[Serializable]
public class GoogleTTSResponseDTO
{
    public string audioContent;
    public GetAudioConfig audioConfig;
}

[Serializable]
public class GetAudioConfig
{
    public string[] timepoints;
    public string audioEncoding;
    public string speakingRate;
    public float pitch;
    public float volumeGainDb;
    public int sampleRateHertz;
    public string[] effectsProfileId;
}
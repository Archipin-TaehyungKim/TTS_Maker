[System.Serializable]
public class TTSModel
{
    public string apiId;
    public string apiKey;
    public string text;
    public string voiceName;

    public TTSModel(string data, string voiceName)
    {
        apiId = "mindslab-api-archipin_01";
        apiKey = "0e1a34083d1041d89f913061a37c2a9f";
        text = data;
        // voiceName = "Luna";
        this.voiceName = voiceName;
    }

    public override string ToString() 
        => $"[\'apiid\': {apiId}, " +
           $"\'apiKey\': {apiKey}, " +
           $"\'text\': {text}, " +
           $"\'voiceName\' {voiceName}";
}
[System.Serializable]
public class NewTTSModel
{
    public string app_id;
    public string name;
    public string[] item;
    public string[] param;
    public string sid;
    
    /// <summary>
    /// Will deprecated.
    /// Mindslab's TTS request dto
    /// </summary>
    /// <param name="data">input text</param>
    /// <param name="voiceName">npc name</param>
    public NewTTSModel(string data, string voiceName)
    {
        app_id = voiceName switch
        {
            "Luna" => "archipin_tts_36d67cf7-0196-54b5-9773-bd2d99235303",
            "Zeppelin" => "archipin_tts_c10bc8a4-c481-5459-89d2-36b0457b5950",
            "Sam" => "archipin_tts_a7990263-a404-5c5b-87e3-1590b3d192a8",
            "Carl" => "archipin_tts_c2e23bea-c41c-5ca2-b57a-c5f4e77c9219",
            _ => app_id
        };

        name = $"archipin_{voiceName.ToLower()}_supply";
        item = new[] { $"spw-{voiceName.ToLower()}-stream" };
        param = new[] { data };
        sid = "kib1BR8YzcqulPAOAAAJ";
    }
}
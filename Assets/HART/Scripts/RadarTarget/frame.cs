using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class webFrame
{
    public webFrame[] result;
}

    /// <summary>
    /// A frame has a tick counter and a list of targets
    /// </summary>
    /// 
[System.Serializable]
public class frame
{
    public int tick;
    public gpsData[] targets;
    public override string ToString()
    {
        string str = "";
        str += "Tick:" + tick + "\n";
        str += "Targets:\n";
        for (int i = 0; i < targets.Length; i++)
        {
            str += "\tID: " + targets[i].id + "\n";
            str += "\tTime: " + targets[i].Time + "\n";
            str += "\tLat: " + targets[i].Latitude + "\n";
            str += "\tLong: " + targets[i].Longitude + "\n";
            str += "\tElv: " + targets[i].Elevation + "\n";
            str += "---\n";
        } 
        return str;
    }
}

/// <summary>
/// The actual gps data
/// </summary>
[System.Serializable]
public class gpsData
{
    public void copy(gpsData a)
    {
        id = a.id;
        Type = a.Type;
        Latitude = a.Latitude;
        Longitude = a.Longitude;
        Time = a.Time;
        Elevation = a.Elevation;
        UTME = a.UTME;
        UTMN = a.UTMN;
        UTMZ = a.UTMZ;
        TrueTrack = a.TrueTrack;
    }
    public string id = "";
    public string Type = "";
    public float Latitude = 0;
    public float Longitude = 0;
    public string Time = "";
    public float Elevation = 0;
    public float UTME = 0;
    public float UTMN = 0;
    public string UTMZ = "";
    public float TrueTrack = 0;
}






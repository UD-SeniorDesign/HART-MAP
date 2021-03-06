﻿using System;
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
    private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
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
    public string getTextData()
    {
        string data = "";
        
        if (Type == "Aircraft")
        {
            data += Type + '\n' + id + '\n' + "Lat:" + Latitude + " Lon:" + Longitude+ '\n'+"Rotation " + TrueTrack+ "° from North";
        }
        else if (Type == "Train")
        {
            data += "Train station" + '\n' + id + "Lat:" + Latitude + " Lon:" + Longitude +'\n';
            foreach (station sta in stops)
            {
                foreach (stationUpdates su in sta.updates)
                {
                    data += string.Format("Train:{0} on line {1} arriving at {2}\n", su.train, su.line,epoch.AddSeconds(su.arriveDepart.arrive).TimeOfDay);
                }
            }
        }
        return data;
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
    public station[] stops;
}
[System.Serializable]
public class station
{
    public string stationId = "";
    public string stationName = "";
    public float Latitude = 0;
    public float Longitude = 0;
    public stationUpdates[] updates; 
}

[System.Serializable]
public class stationUpdates
{
    public string train = "";
    public string line = "";
    public trainTimings arriveDepart;
}
[System.Serializable]
public class trainTimings
{
    public long arrive = 0;
    public long depart = 0;
}





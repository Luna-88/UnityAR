using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GPSController : MonoBehaviour
{
    string message = "Initializing GPS...";
    float thisLat; // current locations
    float thisLong;
    float startingLat; // default value
    float startingLong;
    private bool toggleText = true;
    public GameObject sphere;
    public Text Textfield;
    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
    void OnGUI() 
    {
        GUI.skin.label.fontSize = 50;
        GUI.Label(new Rect(30, 30, 1000, 1000), message);
    }

    public void Reset()
    {
        startingLat = Input.location.lastData.latitude;
        startingLong = Input.location.lastData.longitude;
    }

    public void SetText() {
        toggleText = !toggleText;

        string text = "Current Lat: " + Input.location.lastData.latitude +
        "\nCurrent Long: " + Input.location.lastData.longitude;
    
        Textfield.text = text;

        if (toggleText)
        {
            Textfield.enabled = true;
        }
        else 
        {
            Textfield.enabled = false;
        }
    }

    IEnumerator StartGPS() {
        message = "Starting";

        //Check if location is enabled
        if (!Input.location.isEnabledByUser) 
        {
            message = "Location Services Not Enabled";
            yield break;
        }

        // Start service before querying location
        Input.location.Start(5, 0);
        
        // Wait until service initializes
        int maxWait = 5;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) 
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 secs
        if (maxWait < 1) 
        {
            message = "Time out";
            yield break;
        }

        // Connection failed
        if (Input.location.status == LocationServiceStatus.Failed) 
        {
            message= "Unabled to determine device location";
            yield break;
        } 
        else 
        {
            Input.compass.enabled = true;
            message = "Lat: " + Input.location.lastData.latitude + 
            "\nLong: " + Input.location.lastData.longitude +
            "\nAlt: " + Input.location.lastData.altitude +
            "\nHoriz Acc: " + Input.location.lastData.horizontalAccuracy +
            "\nVert Acc: " + Input.location.lastData.verticalAccuracy +
            "\n=======" +
            "\nHeading: " + Input.compass.trueHeading;

            startingLat = Input.location.lastData.latitude;
            startingLong = Input.location.lastData.longitude;
        }

        // Stop service if there is no need to query location updates continuously - comment out if you want values to be updated
        // Input.location.Stop();

    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartGPS());
    }

    // Update is called once per frame
    void Update()
    {
        DateTime lastUpdate = epoch.AddSeconds(Input.location.lastData.timestamp);
        DateTime rightNow = DateTime.Now;

        thisLat = Input.location.lastData.latitude;
        thisLong = Input.location.lastData.longitude;

        float distance = Haversine(thisLat, thisLong, startingLat, startingLong);

        message = "Distance Travelled: " + distance +
        "\nUpdate Time: " + lastUpdate.ToString("HH:mm:ss") +
        "\nNow: " + rightNow.ToString("HH:mm:ss");

        if (thisLat < 50.98345 && thisLong > -114.074)
            sphere.SetActive(true);
        else 
            sphere.SetActive(false);
    }

    float Haversine(float lat1, float long1, float lat2, float long2)
    {
        float earthRad = 6371000;
        float lRad1 = lat1 * Mathf.Deg2Rad;
        float lRad2 = lat2 * Mathf.Deg2Rad;
        float dLat = (lat2 - lat1) * Mathf.Deg2Rad;
        float dLong = (long2 - long1) * Mathf.Deg2Rad;
        float a = Mathf.Sin(dLat / 2.0f) * Mathf.Sin(dLat / 2.0f) + 
                        Mathf.Cos(lRad1) * Mathf.Cos(lRad2) *
                        Mathf.Sin(dLong / 2.0f) * Mathf.Sin(dLong / 2.0f);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        return earthRad * c; // in metres
    }
}

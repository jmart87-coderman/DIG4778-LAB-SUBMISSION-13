using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml;
using System.Globalization;

public class WeatherManager : MonoBehaviour { // needs monoBehaviour for the coroutine
    public string APIKey;

    [Header("City MUST be formatted as 'City,Country', for example: 'Orlando,US'")]  
    public string City;
    [SerializeField] private string currentCity;

    [Header("Skyboxes")]
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material nightSkybox;
    [SerializeField] private Material sunnySkybox;
    [SerializeField] private Material sunlessSkybox;
    [SerializeField] private Material rainySkybox;
    [SerializeField] private Material snowySkybox;

    [Header("Lighting")]
    [SerializeField] private Light sunLight;
    [SerializeField] private bool isDay; // bool that checks for sunlight 

    private void Start()
    {
        currentCity = City; //make the starting city = the city you input before loading the scene
        StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
    }
    private void Update()
    {
        if(currentCity != City)
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                currentCity = City;
                RefreshCity();
            }
            
        }
    }

    private IEnumerator CallAPI(string url, Action<string> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError) {
                Debug.LogError($"network problem: {request.error}");
            } else if (request.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"response error: {request.responseCode}");
            } else {
                callback(request.downloadHandler.text);
            }
        }
    }

    public IEnumerator GetWeatherXML(Action<string> callback) {
        Debug.Log("Requesting weather for: " + currentCity);
        
        string xmlApi = "http://api.openweathermap.org/data/2.5/weather?q=" + UnityWebRequest.EscapeURL(currentCity) + "&mode=xml&units=imperial&appid=" + APIKey; // imperial is added so that the temperature is in farenheit
        
        return CallAPI(xmlApi, callback);
    }

    public void OnXMLDataLoaded(string data)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(data);

        Debug.Log("XML Data Loaded");

        // Get city information
        XmlNode cityNode = doc.SelectSingleNode("//city");
        string cityName = cityNode.Attributes["name"].Value; //makes the city Name equal to the city Node

        // city time for day and night cycle
        XmlNode timezoneNode = doc.SelectSingleNode("//timezone");
        int timezoneOffset = int.Parse(timezoneNode.InnerText);
        DateTime cityTime = DateTime.UtcNow.AddSeconds(timezoneOffset);

        if (cityTime.Hour >= 6 && cityTime.Hour < 18)
        {
            isDay = true;
        }
        else
        {
            isDay = false;
        }

        // Temperature
        XmlNode temperatureNode = doc.SelectSingleNode("//temperature");
        float temperature = float.Parse(temperatureNode.Attributes["value"].Value, CultureInfo.InvariantCulture); // info will be displayed the same regardless of the user settings

        // humidity
        XmlNode humidityNode = doc.SelectSingleNode("//humidity");
        int humidity = int.Parse(humidityNode.Attributes["value"].Value);

        // weather
        XmlNode weatherNode = doc.SelectSingleNode("//weather");
        int weatherID = int.Parse(weatherNode.Attributes["number"].Value); 
        string weatherDescription = weatherNode.Attributes["value"].Value;

        // update skybox
        UpdateSkybox(weatherID);

        // Show the information in the console when testing
        Debug.Log($"City: {cityName}, Temperature: {temperature}, Humidity: {humidity}, Weather: {weatherDescription}");
    }

    private void UpdateSkybox(int weatherID)
    {
        // each if statement updates both the skybox and the directional light
        
        if(!isDay)
        {
            RenderSettings.skybox = nightSkybox;
            sunLight.intensity = 0.1f;
            sunLight.color = Color.blue;
        }
        else if (weatherID >= 200 && weatherID < 600)
        {
            // Thunderstorm, drizzle, and rain since there are no separate skyboxes for all 3
            RenderSettings.skybox = rainySkybox;

            sunLight.intensity = 0.8f;
            sunLight.color = Color.gray;
        }
        else if (weatherID >= 600 && weatherID < 700)
        {
            // Snow
            RenderSettings.skybox = snowySkybox;

            sunLight.intensity = 0.5f;
            sunLight.color = Color.gray;
        }
        else if (weatherID >= 700 && weatherID < 800)
        {
            // Sunless
            RenderSettings.skybox = sunlessSkybox;

            sunLight.intensity = 0.2f;
            sunLight.color = Color.gray;
        }
        else if (weatherID == 800)
        {
            // Clear
            RenderSettings.skybox = sunnySkybox;
            sunLight.intensity = 1.0f;
            sunLight.color = Color.white;
        }
        else if (weatherID >= 801 && weatherID < 803)
        {
            // Cloudy
            RenderSettings.skybox = daySkybox;
            sunLight.intensity = 0.5f;
            sunLight.color = Color.gray;
        }
        else if(weatherID >= 803)
        {
            RenderSettings.skybox = sunlessSkybox;
            sunLight.intensity = 1.0f;
            sunLight.color = Color.white;
        }
    }

    private void RefreshCity()
    {
        StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
    }
    
    
    
    
    
    

}
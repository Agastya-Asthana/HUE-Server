using System.Text.Json.Nodes;
using System.Text.Json;

class HUEAPI
{
    private static HttpClientHandler handler = new HttpClientHandler();
    HttpClient client = new HttpClient(handler);

    //API related variables
    private string bridgeIPAddress;
    private string appKey;

    //Store mapping from string api method to HttpMethod type api method
    // For example "GET" is HttpMethod.Get when performing requests
    private Dictionary<string, HttpMethod> apiMethodMapping = new Dictionary<string, HttpMethod>();

    public HUEAPI()
    {
        //Ignore HTTPS related certs for development only
        //TODO: figure out long-term solution for deployment
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        //Adding all API methods
        apiMethodMapping.Add("GET", HttpMethod.Get);
        apiMethodMapping.Add("POST", HttpMethod.Post);
        apiMethodMapping.Add("PUT", HttpMethod.Put);
        apiMethodMapping.Add("PATCH", HttpMethod.Patch);
        apiMethodMapping.Add("DELETE", HttpMethod.Delete);
        apiMethodMapping.Add("HEAD", HttpMethod.Head);
        apiMethodMapping.Add("OPTIONS", HttpMethod.Options);

        //Get the API of the bridge on local network

    }

    // Destructor to dispose of the HTTP Handler and Client
    ~HUEAPI()
    {
        handler.Dispose();
        client.Dispose();
    }

    private async Task<HttpResponseMessage> PerformHttpAction(string apiMethod, string apiURL, Dictionary<string, string>? headers = null, JsonDocument? body = null)
    {
        //Can't continue if we don't have an api method or url to send request to
        if (String.IsNullOrEmpty(apiMethod) || String.IsNullOrEmpty(apiURL)) return null;

        HttpRequestMessage request = new HttpRequestMessage(
            this.apiMethodMapping[apiMethod],
            apiURL
        );

        //If there are headers add in every single one of them.
        if (headers != null && headers.Count > 0)
        {
            foreach (string key in headers.Keys)
            {
                request.Headers.Add(key, headers[key]);
            }
        }

        //Check if the JSON body is valid and make it into a string and push into the request content
        if (body != null && body.RootElement.ValueKind != JsonValueKind.Undefined && body.RootElement.ValueKind != JsonValueKind.Null)
        {
            StringContent apiBody = new StringContent( body.ToString(), null, "application/json" );
            request.Content = apiBody;
        }

        HttpResponseMessage response = await client.SendAsync(request);

        return response;
    }

    
    public async Task<JsonNode> GetHUEDevice (string deviceName)
    {
        HttpResponseMessage testResponse = await PerformHttpAction("GET", "https://discovery.meethue.com");
        string responseString = await testResponse.Content.ReadAsStringAsync();
        Console.WriteLine(responseString);
        //JsonDocument document = JsonDocument.Parse(responseString);
        //string bridgeIPAddress = document.RootElement[0].GetProperty("internalipaddress").GetString();
        JsonNode document = JsonNode.Parse(responseString);
        string bridgeIPAddress = document[0]["internalipaddress"].GetValue<string>();
        Console.WriteLine(bridgeIPAddress);

        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("hue-application-key", "rQPBBpxlozXfBQ7B09ij5bQgkcg8R6-M5LwRc3Ej");
        headers.Add("Accept", "application/json");
        HttpResponseMessage deviceQuery = await PerformHttpAction("GET", $"https://{bridgeIPAddress}/clip/v2/resource/device", headers);
        string deviceResponseString = await deviceQuery.Content.ReadAsStringAsync();
        JsonNode responseDocument = JsonNode.Parse(deviceResponseString);
        JsonArray devices = responseDocument["data"].AsArray();
        for (int i = 0; i < devices.Count; i++)
        {
            JsonNode currentDevice = devices[i];
            if (currentDevice["metadata"]["name"].GetValue<string>() == deviceName)
            {
                return currentDevice;
            }
        }
        return null;
    }

    public string GetRIDFromRType(JsonNode device, string rType)
    {
        JsonArray services = device["services"].AsArray();
        for (int i = 0; i < services.Count; i++)
        {
            JsonNode currentService = services[i];
            if (currentService["rtype"].GetValue<string>() == rType)
            {
                return currentService["rid"].GetValue<string>();
            }
        }
        return null;
    }

    public static async void Main(String[] args)
    {
        HUEAPI api = new HUEAPI();
        JsonNode device = await api.GetHUEDevice("TV Stand");
        string rid = api.GetRIDFromRType(device, "light");
        Console.WriteLine(rid);

        //Dispose
        //document.Dispose();

    }
}
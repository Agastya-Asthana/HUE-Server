WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

/*HttpClientHandler handler = new HttpClientHandler();
//Ignore HTTPS related certs
handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
HttpClient client = new HttpClient(handler);

HttpRequestMessage request = new HttpRequestMessage(
    HttpMethod.Get,
    "https://192.168.1.232/clip/v2/resource/device"
);

request.Headers.Add(
    "hue-application-key",
    "rQPBBpxlozXfBQ7B09ij5bQgkcg8R6-M5LwRc3Ej"
);
request.Headers.Add("Accept", "application/json");

HttpResponseMessage response = await client.SendAsync(request);

string body = await response.Content.ReadAsStringAsync();

Console.WriteLine($"Status: {response.StatusCode}");
Console.WriteLine(body);

handler.Dispose();
client.Dispose();*/

HUEAPI.Main(null);

app.MapGet("/", () => "Hello World!");

app.Run();

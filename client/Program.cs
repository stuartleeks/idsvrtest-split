using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        MainAsync().Wait();
    }

    static async Task MainAsync()
    {
        try
        {
            Console.WriteLine("client starting. Press Enter to make call...");
            Console.ReadLine();

            Console.WriteLine("getting endpoint metadata...");

            var discoveryClient = await DiscoveryClient.GetAsync("http://localhost:5000");

            var tokenClient = new TokenClient(discoveryClient.TokenEndpoint, "client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);



            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:5000/api/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}
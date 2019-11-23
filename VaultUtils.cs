using System.IO;
using System.Net.Mime;
using Devies.Extensions.HashiCorpVault.Models;
using Newtonsoft.Json;
using RestSharp;

namespace Devies.Extensions.HashiCorpVault
{
    public static class VaultUtils
    {
        public static string GetVaultToken(string url, string role, string user, string pass)
        {
            IRestClient client = new RestClient($"{url}/v1");
            string clientToken = null;

            if (File.Exists("/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                var token = File.ReadAllLines("/run/secrets/kubernetes.io/serviceaccount/token")[0];

                IRestRequest request = new RestRequest("auth/kubernetes/login", Method.POST);
                var requestBody = JsonConvert.SerializeObject(new {jwt = token, role = role});

                request.AddParameter(MediaTypeNames.Application.Json, requestBody, ParameterType.RequestBody);
                var response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    var data = JsonConvert.DeserializeObject<VaultToken>(response.Content);
                    if (data?.Auth?.ClientToken?.Length > 1)
                    {
                        clientToken = data.Auth.ClientToken;
                    }
                }
            }

            if (clientToken == null && !string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
            {
                IRestRequest request = new RestRequest($"auth/userpass/login/{user}", Method.POST);
                var requestBody = JsonConvert.SerializeObject(new {password = pass});

                request.AddParameter(MediaTypeNames.Application.Json, requestBody, ParameterType.RequestBody);
                var response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    var data = JsonConvert.DeserializeObject<VaultToken>(response.Content);
                    if (data?.Auth?.ClientToken?.Length > 1)
                    {
                        clientToken = data.Auth.ClientToken;
                    }
                }
            }

            return clientToken;
        }
    }
}
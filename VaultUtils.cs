using System.IO;
using System.Net.Mime;
using Devies.Extensions.HashiCorpVault.Models;
using Newtonsoft.Json;
using RestSharp;

namespace Devies.Extensions.HashiCorpVault
{
    public static class VaultUtils
    {
        public static string LoginWithUserpass(string url, string user, string pass)
        {
            IRestClient client = new RestClient($"{url}/v1");
            
            if (string.IsNullOrWhiteSpace(user))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(pass))
            {
                return null;
            }
            
            IRestRequest request = new RestRequest($"auth/userpass/login/{user}", Method.POST);
            var requestBody = JsonConvert.SerializeObject(new {password = pass});

            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                var data = JsonConvert.DeserializeObject<VaultToken>(response.Content);
                if (data?.Auth?.ClientToken?.Length > 1)
                {
                    return data.Auth.ClientToken;
                }
            }

            return null;
        }


        public static string LoginWithK8SServiceAccount(string url, string role)
        {
            IRestClient client = new RestClient($"{url}/v1");

            if (!File.Exists("/run/secrets/kubernetes.io/serviceaccount/token"))
            {
                return null;
            }

            var token = File.ReadAllLines("/run/secrets/kubernetes.io/serviceaccount/token")[0];

            IRestRequest request = new RestRequest("auth/kubernetes/login", Method.POST);
            var requestBody = JsonConvert.SerializeObject(new {jwt = token, role = role});

            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                var data = JsonConvert.DeserializeObject<VaultToken>(response.Content);
                if (data?.Auth?.ClientToken?.Length > 1)
                {
                    return data.Auth.ClientToken;
                }
            }


            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Devies.Extensions.HashiCorpVault.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace Devies.Extensions.HashiCorpVault
{
    public static class VaultExtensions
    {
        public static IConfigurationBuilder AddVaultEnvironments(this IConfigurationBuilder builder, string url, string token)
        {
            if (string.IsNullOrWhiteSpace("token"))
            {
                throw new ArgumentException("token is null or empty");
            }

            if (url?.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) != true)
            {
                throw new ArgumentException("url is not set or doesn't start with http");
            }
            
            var secretLookups = new Dictionary<string, Dictionary<string, string>>();
            
            var vaultData = GetAllVaultValues(builder.Build().GetChildren());

            var dict = vaultData.ToDictionary(x => x.Key, x =>
            {
                var split = x.Value.Split("#");

                if (split.Length < 2)
                {
                    return null;
                }

                var secretPath = split[0];
                var secretKey = split[1];

                int? version = null;
                if (split.Length >= 3)
                {
                    int tempVersion = -1;
                    if (int.TryParse(split[2], out tempVersion))
                    {
                        version = tempVersion;
                    }
                }

                var lookupPath = secretPath + "#" + version ?? "";
                
                if (!secretLookups.ContainsKey(lookupPath))
                {
                    var secrets = GetSecrets(url, secretPath, token, version);
                    if (secrets == null)
                    {
                        throw new AccessViolationException($"Can't read secret path: {secretPath}");
                    }
                    secretLookups.Add(lookupPath, secrets);
                }

                var secretLookup = secretLookups[lookupPath];
                if (secretLookup.ContainsKey(secretKey))
                {
                    return secretLookup[secretKey];
                }
                
                return secretKey;
            }).Where(x => x.Value != null);

            builder.AddInMemoryCollection(dict);
            return builder;
        }

        private static Dictionary<string, string> GetSecrets(string url, string path, string token, int? version)
        {
            IRestClient client = new RestClient($"{url}/v1");

            IRestRequest request = new RestRequest(path, Method.GET);
            request.AddHeader("X-Vault-Token", token);

            if (version.HasValue)
            {
                request.AddQueryParameter("version", version.Value.ToString());
            }
            
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                var data = JsonConvert.DeserializeObject<VaultSecret>(response.Content);
                if (data?.Data?.Data?.Secrets.Any() == true)
                {
                    return data.Data.Data.Secrets.ToDictionary(x=> x.Key, x=> x.Value.ToString());
                }
            }

            return null;
        }

        private static List<KeyValuePair<string, string>> GetAllVaultValues(IEnumerable<IConfigurationSection> sections)
        {
            var results = new List<KeyValuePair<string, string>>();
            
            foreach (var section in sections)
            {
                if (section.Value != null)
                {
                    if (section.Value.StartsWith("vault:", StringComparison.Ordinal))
                    {
                        results.Add(new KeyValuePair<string, string>(section.Path, section.Value.Substring(6)));
                    }
                }
                else
                {
                    results.AddRange(GetAllVaultValues(section.GetChildren()));
                }
            }
            
            return results;
        }
    }
}
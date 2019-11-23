using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Devies.Extensions.HashiCorpVault.Models
{
    public class VaultSecret
    {
        [JsonProperty("data")]
        public VaultSecretData Data { get; set; }
    }

    public class VaultSecretData
    {
        [JsonProperty("data")]
        public VaultSecretDataList Data { get; set; }
    }

    public class VaultSecretDataList
    {
        [JsonExtensionData] 
        public IDictionary<string, JToken> Secrets { get; set; }
    }
}
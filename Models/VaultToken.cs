using System.Collections.Generic;
using Newtonsoft.Json;

namespace Devies.Extensions.HashiCorpVault.Models
{
    public class VaultToken
    {
        [JsonProperty("auth")]
        public Auth Auth { get; set; }
    }

    public class Auth
    {
        [JsonProperty("client_token")]
        public string ClientToken { get; set; }
    }

}
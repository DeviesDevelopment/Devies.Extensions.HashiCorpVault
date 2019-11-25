# Devies.Extensions.HashiCorpVault


## Requriments
- An eisting vault
- A client_token or any auth path in the vault
- Any IBuilderConfiguration with values set to vaild vault values

## Recommendations
- If you host in K8S, set up the kubernetes auth in the vault, read more; https://banzaicloud.com/blog/inject-secrets-into-pods-vault-revisited/

## How to

1. Install the nuget package `dotnet add package Devies.Extensions.HashiCorpVault`
2. Login and get a token with `VaultUtils.LoginWithUserpass` or `VaultUtils.LoginWithK8SServiceAccount`, or your own way.
3. Call `AddVaultEnvironments` on your `IConfigurationBuilder` with your vault-url and token
4. AddVaultEnvironments HAS TO BE AFTER all other settings that may involve vault variables

## Example
#### appsettings.Development.json
```json
{
    "ServiceOptions": {
        "Url": "https://service.org",
        "Username": "vault:secret/data/service/dev#username",
        "Password": "vault:secret/data/service/dev#userpass"
    }
}
```

#### Program.cs

```c#
private Func<IConfigurationBuilder, string, IConfigurationRoot> ConfigurationBuilder = 
(builder, vaultToken) => 
    builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.Development.json")
    .AddVaultEnvironments("https://vault.myorg.org", vaultToken)
    .Build();
```

## Sources
- https://banzaicloud.com/blog/inject-secrets-into-pods-vault-revisited/




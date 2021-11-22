namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    
    // List of secrets by name and by version
    public class SecretList : Dictionary<string, Dictionary<string, Secret>>
    {
        private const string EmptyVersion = "_null_";

        public SecretList()
        {
        }

        public SecretList(IList<Secret> secrets)
        {
            if (secrets != null)
            {
                foreach (var secret in secrets)
                {
                    this.SetSecret(secret);
                }
            }
        }

        public Secret GetSecret(string secretName, DateTime date)
        {
            Dictionary<string, Secret> secretVersions;
            if (this.TryGetValue(secretName, out secretVersions))
            {
                foreach(var secret in secretVersions.Values)
                {
                    if ( (date >= secret.ActivationDate) && (date < secret.ExpirationDate))
                    {
                        return secret;
                    }
                }
            }
            return null;
        }

        public void SetSecret(Secret secret)
        {
            string nameKey = secret.Name;
            string versionKey = secret.Version ?? EmptyVersion;
            Dictionary<string, Secret> secretVersions;
            if (this.TryGetValue(nameKey, out secretVersions))
            {
                if (secretVersions.ContainsKey(versionKey))
                {
                    secretVersions[versionKey] = secret;
                }
                else
                {
                    secretVersions.Add(versionKey, secret);
                }
            }
            else
            {
                secretVersions = new Dictionary<string, Secret>();
                secretVersions[versionKey] = secret;
                this[nameKey] = secretVersions;
            }
        }
    }
}

namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    
    // List of secrets by name and by version
    public class SecretList : Dictionary<string, Dictionary<string, Secret>>
    {
        private const string EmptyVersion = "_null_";

        /// <summary>
        /// Create a new list of secrets.
        /// </summary>
        public SecretList()
        {
        }

        /// <summary>
        /// Create a new list of secrets from the given list of secrets.
        /// Multiple versions of a secret will be stored indexed by version.
        /// </summary>
        /// <param name="secrets">List of secrets to copy into the new list. This list can contain multiple versions with the same name.</param>
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

        /// <summary>
        /// Get the secret by name and date.
        /// </summary>
        /// <param name="secretName">Name of the secret to search for.</param>
        /// <param name="date">If given, only secrets that are valid at the given date will be returned.</param>
        /// <returns>The secret if found, or null if not found by name or active at given date.</returns>
        public Secret GetSecret(string secretName, DateTime? date = null)
        {
            if (this.TryGetValue(secretName, out Dictionary<string, Secret> secretVersions))
            {
                foreach (var secret in secretVersions.Values)
                {
                    if ((date != null) && (date >= secret.ActivationDate) && (date < secret.ExpirationDate))
                    {
                        return secret;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Store secret in the secret list, indexed by name and version.
        /// </summary>
        /// <param name="secret">Secret to store</param>
        public void SetSecret(Secret secret)
        {
            string nameKey = secret.Name;
            string versionKey = secret.Version ?? EmptyVersion;
            if (this.TryGetValue(nameKey, out Dictionary<string, Secret> secretVersions))
            {
                // Secret already known, so update or add by version
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
                // Secret not known yet, so add by name
                secretVersions = new Dictionary<string, Secret>
                {
                    [versionKey] = secret
                };
                this[nameKey] = secretVersions;
            }
        }
    }
}

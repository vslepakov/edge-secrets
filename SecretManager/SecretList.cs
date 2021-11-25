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
        public SecretList(IList<Secret?>? secrets)
        {
            if (secrets != null)
            {
                foreach (var secret in secrets)
                {
                    if (secret != null)
                    {
                        this.SetSecret(secret);
                    }
                }
            }
        }

        /// <summary>
        /// Get the secret by name, version and date.
        /// </summary>
        /// <param name="secretName">Name of the secret to search for.</param>
        /// <param name="version">Version of the secret to search for, or null for any version.</param>
        /// <param name="date">Date to use for validating the secret, or null for any date.</param>
        /// <returns>The secret if found, the first secret if found more than one, or null if not found.</returns>
        public Secret? GetSecret(string secretName, string? version, DateTime? date = null)
        {
            if (TryGetValue(secretName, out var secretVersions))
            {
                if (version != null)
                {
                    if (secretVersions.TryGetValue(version, out var secret))
                    {
                        if ((date == null) || secret.DateIsActive(date))
                        {
                            return secret;
                        }
                    }
                }
                else
                {
                    foreach (var secret in secretVersions.Values)
                    {
                        if ((date == null) || secret.DateIsActive(date))
                        {
                            return secret;
                        }
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
            string? nameKey = secret.Name;
            if (nameKey != null)
            {
                string versionKey = secret.Version ?? EmptyVersion;
                if (TryGetValue(nameKey, out var secretVersions))
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
}

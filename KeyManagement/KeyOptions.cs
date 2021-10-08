namespace EdgeSecrets.KeyManagement
{
    public enum KeyType
    {
        ECC,
        RSA,
        Symmetric
    }

    public class KeyOptions
    {
        public KeyType KeyType { get; set; }

        public int KeySize { get; set; }

        public string KeyId { get; set; }
    }
}

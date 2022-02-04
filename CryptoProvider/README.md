# CryptoProvider
CryptoProviders are used to encrypt and decrypt secrets.
Below the providers currently supported.

## WorkloadApiCryptoProvider
pre-requisites:
* Azure IoT Edge 1.x

It uses the Workload API of the Azure IoT Edge [security manager](https://docs.microsoft.com/en-us/azure/iot-edge/iot-edge-security-manager?view=iotedge-2018-06).
```c#
string initializationVector = "<initializationVector>";
cryptoProvider = new WorkloadApiCryptoProvider(initializationVector);
```

## IdentityServiceCryptoProvider
pre-requisites:
* either Azure IoT Edge 1.2+
* ...or the [iot-identity-service](https://azure.github.io/iot-identity-service/) standalone

It uses the [Key Service API](https://azure.github.io/iot-identity-service/api/keys-service.html) of the Azure IoT Identity Service.
```c#
cryptoProvider = new IdentityServiceCryptoProvider();
```

## AzureKeyVaultCryptoProvider
It uses Azure Key Vault.
```c#
cryptoProvider = new AzureKeyVaultCryptoProvider();
```

It requires the following environment variables:
* AZURE_CLIENT_ID
* AZURE_TENANT_ID
* AZURE_CLIENT_SECRET
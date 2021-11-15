# How to deploy?

To deploy the whole solution with all the services:

```bash
./deployAll.sh [RG Name] [Image URI] [Tenant ID] [App Object ID] [App Client ID] [App Password] [WebHook API Key to use]
```

> Tenant ID, App Object ID, App Client ID and App Password are needed to give the container app access to Azure KeyVault.

To deploy the container app only:

```bash
./deployApp.sh [RG Name] [Image URI] [Tenant ID] [App Client ID] [App Password] [WebHook API Key to use]
```


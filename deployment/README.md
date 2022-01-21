# Deploy all
To deploy the whole solution with all the services:

```bash
./deployAll.sh [RG Name] [Container Registry] [Image URI] [Tenant ID] [App Object ID] [App Client ID] [App Password] [WebHook API Key to use]
```

That script will deploy all the services in the rectangle:
![alt](../images/deployment-all.png)

* **[RG]**: the resource group name. 

    NOTE: It will be created in the northeurope region.

* **[WebHook API Key]**: an arbitrary string to use as the webhook API key.


Prerequisites:
* **[Container Registry]** and **[Image URI]** of the SecretDeliveryApp. 

    Look [here](../SecretDeliveryApp) to build and upload the image.
* **[Tenant ID]**, **[App Object ID]**, **[App Client ID]** and **[App Password]**

    The SecretDeliveryApp uses a service principal to access Azure KeyVault and Azure Container Registry. To create a service principal and get those info:


# Deploy the container app only
To deploy the container app only (TODO: setup Continuous Deployment from GitHub):

```bash
./deployApp.sh [RG Name] [Container Registry] [Image URI] [Tenant ID] [App Client ID] [App Password] [WebHook API Key to use] [Azure KeyVault URL] [KubeEnvironment Resource ID]

```

> KubeEnvironment Resource ID looks similar to this:  
>
> /subscriptions/[SUB ID]/resourceGroups/[RG NAME]/providers/Microsoft.Web/kubeEnvironments/[K8s ENV NAME]


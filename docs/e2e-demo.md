# deploy the solution
```bash
RG="edge-secrets-rg"               
APP_SP_NAME="arlotito-mySecretDeliveryApp"                 
ACR_NAME="arlotito.azurecr.io"
IMAGE_URI="edge-secrets/secret-delivery-app:0.0.1"
WEB_API_KEY="anyString"

# build and push the SecretDeliveryApp (see SecretDeliveryApp/README.md)
# NOTE: make sure you are in the project's root folder
docker build -t $ACR_NAME/$IMAGE_URI -f ./SecretDeliveryApp/Dockerfile .
docker push $ACR_NAME/$IMAGE_URI

# Obtain the full acr ID 
ACR_REGISTRY_ID=$(az acr show --name $ACR_NAME --query "id" --output tsv)

# create the service principal with "acrpull" access to the ACR.
# NOTE: password is shown only once. Make sure you save it!
APP_PASSWORD=$(az ad sp create-for-rbac --name $APP_SP_NAME --scopes $ACR_REGISTRY_ID --role acrpull --query "password" --output tsv)

# retrieve other ids
APP_OBJECT_ID=$(az ad sp list --display-name $APP_SP_NAME --query [0].objectId -o tsv)
APP_CLIENT_ID=$(az ad sp list --display-name $APP_SP_NAME --query [0].appId -o tsv)
APP_TENANT_ID=$(az ad sp list --display-name $APP_SP_NAME --query [0].appOwnerTenantId -o tsv)

# deploy
cd deployment
./deployAll.sh "$RG" "$ACR_NAME" "$ACR_NAME/$IMAGE_URI" "$APP_TENANT_ID" "$APP_OBJECT_ID" "$APP_CLIENT_ID" "$APP_PASSWORD" "$WEB_API_KEY"
```

# set the secret "FabrikamConnectionString" in KV
```bash
# get KV name
KV_NAME=$(az keyvault list -g "$RG" --query [0].name -o tsv)

# assign permissions to the user
MY_OBJECT_ID=$(az ad user show --id "arlotito@microsoft.com" --query objectId -o tsv)
az keyvault set-policy --name $KV_NAME --object-id $MY_OBJECT_ID --secret-permissions delete get list set

# create the secret "FabrikamConnectionString"
az keyvault secret set --name "FabrikamConnectionString" --vault-name $KV_NAME --value "connection-string"
```

# provision an iot edge 
```bash
# get iot hubname
IOT_HUB_NAME=$(az iot hub list -g "$RG" --query [0].name -o tsv)

# provision vm with iot edge 1.2
curl -L https://raw.githubusercontent.com/arlotito/vm-iotedge-provision/dev/scripts/vmedge.sh | bash -s -- \
    -s Standard_DS2_v2 \
    -g $RG -l northeurope \
    -e 1.2 \
    -n $IOT_HUB_NAME \
    -u "azuser"
```

# deploy the Samples/SecretManager.Edge
edit the Samples/SecretManager.Edge/.env
```bash
ACR_ADDRESS=<myAcr>.azurecr.io
ACR_USER=<myuser>
ACR_PASSWORD=<mypassword>

EDGESECRET_KEYID="mysymmkey-1"
EDGESECRET_CRYPTO_PROVIDER="workload-api"
EDGESECRET_INIT_VECTOR="1234567890"
```

build and push the SecretManager.Edge solution
deploy the solution to the iot edge vm


# connect to the iot edge VM and get the logs 
```bash
# connect to the iot edge VM
VM_NAME=$(az vm list -g $RG --query [0].name -o tsv)
ssh azuser@$VM_NAME.northeurope.cloudapp.azure.com -i /home/arlotito/.ssh/vmedge.key
```

[connect to VM](../Docs/samples.md)

# view logs
ContainerAppConsoleLogs_CL 





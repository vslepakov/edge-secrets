# Edge Secrets project samples

The Edge Secrets project contains sample applications to demonstrate the use of the Secret Manager components.

## SecretManager.Edge

The [SecretManager.Edge](../Samples/SecretManager.Edge) sample is an IoT Edge Module that will use the SecretManager components to retrieve a connection string to a database and read product informtion.

After building and installing the module, and setting upo the cloud components, use the following steps to run demonstrate the working of the Secret Manager. On the Edge device, run the following commands.

1. start the edge device and wait for all modules to be running

2. get id of the docker container
```
containerid="$(sudo docker ps -aqf name=SecretManager)"
```

3. remove the container file cache
```
sudo docker exec $containerid rm /usr/local/cache/secrets.json
```

4. restart the SecretManager module
```
sudo iotedge restart SecretManager
```

5. show the SecretManager module logs
```
azureuser@mvsdev3vmedge1:~$ sudo iotedge logs SecretManager --since 5m
```
This will show logging information similar to:
```
IoT Hub module client initialized.
Get data from database.
Using initialization vector 0123456789
Using Crypto Provider EdgeSecrets.CryptoProvider.WorkloadApiCryptoProvider
Secret manager client created.
Send request for secrets with id 'b1429d06-4ad1-449a-b491-5b7130fd7db5'
Received update of secrets for RequestId 'b1429d06-4ad1-449a-b491-5b7130fd7db5' (1 secret(s) received)
Add secret to file /usr/local/cache/secrets.json, file now contains 1 secrets
Valid database connection string found.
680 HL Road Frame - Black, 58 FR-R92B-58
706 HL Road Frame - Red, 58 FR-R92R-58
707 Sport-100 Helmet, Red HL-U509-R
708 Sport-100 Helmet, Black HL-U509
709 Mountain Bike Socks, M SO-B909-M
...
```
From this log, you can see that a remote request is sent to retrieve the secret, and when retrieved it is stored in the container file.

6. show the container files
```
sudo docker exec $containerid ls -l /usr/local/cache
```
And show the content of the container file:
```
sudo docker exec $containerid cat /usr/local/cache/secrets.json
{"FabrikamConnectionString":{"6769a77965264fa9a41b6b6e5d64654e":{"Name":"FabrikamConnectionString","Value":"AiRdKfOVR5nrp4hC4X\u002BYr/CQMynmnQ76bipeQ4wJOP/vLPNYYj12dTAG3doSVT14GxAPCs08aD6v\u002BTkUK5XK7OJ7XmPx\u002Bbj92fH5mZ6716WiEtIzGlAGOXYn\u002BJ3L7edy5Tb45SkUMTGgB14QQqV1qWL79HPtM/vaKFX5VEIVigqzSCTANXGJwE5Ktvx1DTev3eNSyhuNoPHO7pRf8/PDOhmqWZ2Sut8ZZIpopPraKEUE7WirXPtse64ZjBn13lLzsH02BbgGsZVW65Y49iOxpacvtEoy/ARlnQQkUgbCaJYbH3p2d51PUhOiTZLjSDjelvafc5kirvE\u002BGHdQyEORT\u002BRilEll","Version":"6769a77965264fa9a41b6b6e5d64654e","ActivationDate":"0001-01-01T00:00:00","ExpirationDate":"9999-12-31T23:59:59.9999999"}}}
```

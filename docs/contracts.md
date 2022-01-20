# Secret request and response contract

Device is sending a secret request messages using a Telemetry message to the cloud.
The message will have the **secret-request-id** property set to the RequestId.

```
{
  "RequestId": "6cba680f-9f6e-4daa-a6dd-2918b8b8d157",
  "CreateDate": "2021-11-19T07:56:25.447128+00:00",
  "Secrets": [
    {
      "Name": "test",
      "Version": "1234" // optional, otherwise all versions will be returned
    }
  ]
}
```

The cloud will respond with the Direct Method **UpdateSecrets** with the following payload.

```
{
  "RequestId": "6cba680f-9f6e-4daa-a6dd-2918b8b8d157",
  "Secrets": [
    {
      "Name": "test",
      "Value": "secret from Azure",
      "Version": "1234", // optional
      "ActivationDate": "2021-01-01T00:00:00", // optional
      "ExpirationDate": "2022-01-01T00:00:00" // optional
    }
  ]
}
```

The RequestId in the request message and the DirectMethod call should match.
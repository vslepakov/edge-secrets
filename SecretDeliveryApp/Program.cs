using SecretDeliveryApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<SecretRequestProcessor>();
builder.Services.AddScoped<ISecretProvider, AzureKeyVaultClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAPIKeyCheckMiddleware();
}

// Doing the validation handshake with EventGrid so the subcription can be created
app.MapMethods("/events", new[] { "OPTIONS" }, (http) =>
 {
     app.Logger.LogInformation("Received request for validation.");
     http.Response.StatusCode = 200;
     http.Response.Headers.Add("Webhook-Allowed-Origin", "eventgrid.azure.net");

     return Task.CompletedTask;
 }
); 

// Handle events
app.MapPost("/events", async (HttpContext http, SecretRequestProcessor processor) =>
{
    string content = string.Empty;

    using (var readStream = new StreamReader(http.Request.Body))
    content = await readStream.ReadToEndAsync();

    await processor.ProcessAsync(content);
   
});

app.Run();
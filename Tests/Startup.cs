using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Tests
{
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<Startup>()
                .Build();

            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", config["AZURE_CLIENT_ID"]);
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", config["AZURE_TENANT_ID"]);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", config["AZURE_CLIENT_SECRET"]);

            hostBuilder.ConfigureHostConfiguration(builder => builder.AddConfiguration(config));
        }
    }
}

#region

using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

#endregion

namespace UpdateActiveDirectory
{
    internal class Program
    {
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        public static ApiAuthConfiguration ApiConfiguration { get; private set; }

        public static WhatToProcess WhatToProcess { get; private set; }

        private static async Task Main(string[] args)
        {
            ApiConfiguration = Configuration.GetSection(nameof(ApiAuthConfiguration)).Get<ApiAuthConfiguration>();
            WhatToProcess = Configuration.GetSection(nameof(WhatToProcess)).Get<WhatToProcess>();
            
            if (WhatToProcess.AddClaim)
            {
                await ProcessMethods.AddClaimToUserAsync();
            }

            if (WhatToProcess.AddUser)
            {
                await ProcessMethods.AddUserAsync();
            }
            
            if (WhatToProcess.DeleteUser)
            {
                await ProcessMethods.DeleteUserAsync();
            }
            
            if (WhatToProcess.ChangeUserPassword)
            {
                await ProcessMethods.ChangeUserPasswordAsync();
            }

            if (WhatToProcess.ListUsers)
            {
                await ProcessMethods.ListUsersAsync();
            }
            
            Console.WriteLine("Done");
        }
    }
}
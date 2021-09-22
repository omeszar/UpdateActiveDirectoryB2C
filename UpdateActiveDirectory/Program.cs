#region

using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Directory = System.IO.Directory;

#endregion

namespace UpdateActiveDirectory
{
    internal class Program
    {
        private static ApiAuthConfiguration _apiConfiguration;
        private static WhatToProcess _whatToProcess;

        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        static async Task Main(string[] args)
        {
            _apiConfiguration = Configuration.GetSection(nameof(ApiAuthConfiguration)).Get<ApiAuthConfiguration>();
            _whatToProcess = Configuration.GetSection(nameof(WhatToProcess)).Get<WhatToProcess>();
            if (_whatToProcess.AddClaim)
            {
                await AddClaimToUserAsync();
            }

            if (_whatToProcess.AddUser)
            {
                await AddUserAsync();
            }

            if (_whatToProcess.ListUsers)
            {
                await ListUsersAsync();
            }
            
            await Task.CompletedTask;
            Console.WriteLine("Done");
        }

        private static async Task ListUsersAsync()
        {
            var client = GetGraphServiceClient();
            
            var users = await client.Users
                .Request()
                .Select(e => new
                {
                    e.DisplayName,
                    e.Id,
                    e.Identities,
                    e.GivenName,
                    e.Surname,
                    e.UserPrincipalName
                })
                .GetAsync();
            
            var pageIterator = PageIterator<User>.CreatePageIterator(client, users,
                (user) =>
                {
                    Console.WriteLine($"{user.Id}");
                    Console.WriteLine($"{user.GivenName} {user.Surname}");
                    Console.WriteLine($"Display Name: {user.DisplayName}");
                    Console.WriteLine($"Principal Name: {user.UserPrincipalName}");
                    
                    foreach (var objectIdentity in user.Identities)
                    {
                        if (objectIdentity.SignInType.Contains("emailAddress",
                            StringComparison.InvariantCultureIgnoreCase))
                        {
                            Console.WriteLine($"Identity: {objectIdentity.IssuerAssignedId}");
                        }
                    }

                    Console.WriteLine($"**************{Environment.NewLine}");
                    return true;
                },
                (req) => req);

            await pageIterator.IterateAsync();
        }

        private static async Task AddClaimToUserAsync()
        {
            var client = GetGraphServiceClient();

            var extensions = new Dictionary<string, object>
            {
                {
                    $"extension_{_apiConfiguration.ExtensionAppId}_{_whatToProcess.AddClaimModel.AttributeName}",
                    _whatToProcess.AddClaimModel.ClaimValue
                }
            };

            var result = await client.Users[_whatToProcess.AddClaimModel.UserId]
                .Request()
                .UpdateAsync(new User
                {
                    AdditionalData = extensions
                });
        }

        private static async Task AddUserAsync()
        {
            var client = GetGraphServiceClient();

            var user = new User()
            {
                AccountEnabled = true,
                DisplayName = _whatToProcess.AddUserModel.DisplayName,
                PasswordProfile = new PasswordProfile()
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = "Rolo2018"
                },
                PasswordPolicies = "DisablePasswordExpiration",
                GivenName = _whatToProcess.AddUserModel.GivenName,
                Surname = _whatToProcess.AddUserModel.Surname,
                Identities = new List<ObjectIdentity>
                {
                    new()
                    {
                        SignInType = "emailAddress",
                        Issuer = _apiConfiguration.TenantId,
                        IssuerAssignedId = _whatToProcess.AddUserModel.Email
                    }
                }
            };

            var result = await client.Users.Request().AddAsync(user);
            Console.WriteLine($"New user created: {result.Id}");
        }

        private static GraphServiceClient GetGraphServiceClient()
        {
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(_apiConfiguration.ApiClientId)
                .WithTenantId(_apiConfiguration.TenantId)
                .WithClientSecret(_apiConfiguration.ApiClientSecret)
                .Build();

            var authProvider = new ClientCredentialProvider(confidentialClientApplication);

            var client = new GraphServiceClient(authProvider);
            return client;
        }
    }
}
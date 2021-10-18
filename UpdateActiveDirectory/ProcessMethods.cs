#region

using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

#endregion

namespace UpdateActiveDirectory
{
    internal class ProcessMethods
    {
        public static async Task ListUsersAsync()
        {
            var client = GetGraphServiceClient();
            var custom1 =
                $"extension_{Program.ApiConfiguration.ExtensionAppId}_website";
            var custom2 =
                $"extension_{Program.ApiConfiguration.ExtensionAppId}_CustomerNumber";
            
            var users = await client.Users
                .Request()
                // .Select(e => new
                // {
                //     e.DisplayName,
                //     e.Id,
                //     e.Identities,
                //     e.GivenName,
                //     e.Surname,
                //     e.UserPrincipalName,
                //     e.UserType
                // })
                .Select($"id,displayName,identities,givenName,surName,userPrincipalName,userType, {custom1},{custom2}")
                .GetAsync();

            var pageIterator = PageIterator<User>.CreatePageIterator(client, users,
                (user) =>
                {
                    Console.WriteLine($"{user.Id}");
                    Console.WriteLine($"{user.GivenName} {user.Surname}");
                    Console.WriteLine($"Display Name: {user.DisplayName}");
                    Console.WriteLine($"Principal Name: {user.UserPrincipalName}");
                    Console.WriteLine($"Type: {user.UserType}");
                    if (user.AdditionalData != null)
                    {
                        Console.WriteLine("***Custom Attributes***");
                        foreach (var (key, value) in user.AdditionalData)
                        {
                            Console.WriteLine($"{key}: {value}");
                        }    
                        Console.WriteLine("***Custom Attributes***");
                    }
                    

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

        public static async Task AddClaimToUserAsync()
        {
            var client = GetGraphServiceClient();

            var extensions = new Dictionary<string, object>
            {
                {
                    $"extension_{Program.ApiConfiguration.ExtensionAppId}_{Program.WhatToProcess.AddClaimModel.AttributeName}",
                    Program.WhatToProcess.AddClaimModel.ClaimValue
                }
            };

            var result = await client.Users[Program.WhatToProcess.AddClaimModel.UserId]
                .Request()
                .UpdateAsync(new User
                {
                    AdditionalData = extensions
                });
        }

        public static async Task AddUserAsync()
        {
            var client = GetGraphServiceClient();

            var user = new User()
            {
                AccountEnabled = true,
                DisplayName = Program.WhatToProcess.AddUserModel.DisplayName,
                PasswordProfile = new PasswordProfile()
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = Program.WhatToProcess.AddUserModel.Password
                },
                PasswordPolicies = "DisablePasswordExpiration",
                GivenName = Program.WhatToProcess.AddUserModel.GivenName,
                Surname = Program.WhatToProcess.AddUserModel.Surname,
                Identities = new List<ObjectIdentity>
                {
                    new()
                    {
                        SignInType = "emailAddress",
                        Issuer = Program.ApiConfiguration.TenantId,
                        IssuerAssignedId = Program.WhatToProcess.AddUserModel.Email
                    }
                }
            };

            var result = await client.Users.Request().AddAsync(user);
            Console.WriteLine($"New user created: {result.Id}");
        }

        public static async Task DeleteUserAsync()
        {
            var client = GetGraphServiceClient();

            await client.Users[Program.WhatToProcess.DeleteUserModel.UserId]
                .Request().DeleteAsync();
        }

        public static async Task ChangeUserPasswordAsync()
        {
            var client = GetGraphServiceClient();
            
            var user = new User
            {
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = Program.WhatToProcess.ChangeUserPasswordModel.NewPassword
                }
            };
            
            await client.Users[Program.WhatToProcess.ChangeUserPasswordModel.UserId]
                .Request()
                .UpdateAsync(user);
        }
        
        private static GraphServiceClient GetGraphServiceClient()
        {
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(Program.ApiConfiguration.ApiClientId)
                .WithTenantId(Program.ApiConfiguration.TenantId)
                .WithClientSecret(Program.ApiConfiguration.ApiClientSecret)
                .Build();

            var authProvider = new ClientCredentialProvider(confidentialClientApplication);

            var client = new GraphServiceClient(authProvider);
            return client;
        }

        
    }
}
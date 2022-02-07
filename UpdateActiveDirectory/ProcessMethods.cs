#region

using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            var custom3 =
                $"extension_{Program.ApiConfiguration.ExtensionAppId}_SBAmortizationRole";

            var custom4 =
                $"extension_{Program.ApiConfiguration.ExtensionAppId}_MWFRole";

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
                .Select(
                    $"id,displayName,identities,givenName,surName,userPrincipalName,userType,otherMails, {custom1},{custom2}, {custom3}, {custom4}")
                .GetAsync();

            await DisplayListOfUsersAsync(client, users);
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

        public static async Task FindUser()
        {
            var client = GetGraphServiceClient();

            var filter = new StringBuilder();

            filter.Append($"startsWith(givenName, '{Program.WhatToProcess.FindUserModel.SearchString}')");
            filter.Append(" or ");
            filter.Append($"startsWith(Surname, '{Program.WhatToProcess.FindUserModel.SearchString}')");
            filter.Append(" or ");
            filter.Append($"startsWith(displayName, '{Program.WhatToProcess.FindUserModel.SearchString}')");
            // filter.Append(" or ");
            // filter.Append($"startsWith(mail, '{Program.WhatToProcess.FindUserModel.SearchString}')");
            filter.Append(" or ");
            filter.Append($"otherMails/any(x:startsWith(x, '{Program.WhatToProcess.FindUserModel.SearchString}'))");


            var custom1 =
                $"extension_{Program.ApiConfiguration.ExtensionAppId}_website";
            var custom2 =
                $"extension_{Program.ApiConfiguration.ExtensionAppId}_CustomerNumber";

            var custom3 =
                $"extension_{Program.ApiConfiguration.ExtensionAppId}_SBAmortizationRole";

            var custom4 =
                $"extension_{Program.ApiConfiguration.ExtensionAppId}_MWFRole";
            
            Console.WriteLine(filter.ToString());

            var users = await client.Users.Request().Filter(filter.ToString())
                .Select(
                    $"id,displayName,identities,givenName,surName,userPrincipalName,userType,otherMails, {custom1},{custom2}, {custom3}, {custom4}")
                .GetAsync();
            
            await DisplayListOfUsersAsync(client, users);
        }

        private static async Task DisplayListOfUsersAsync(GraphServiceClient client, IGraphServiceUsersCollectionPage users)
        {
            var pageIterator = PageIterator<User>.CreatePageIterator(client, users,
                (user) =>
                {
                    Console.WriteLine($"{user.Id}");
                    Console.WriteLine($"First: {user.GivenName} - Last: {user.Surname}");
                    Console.WriteLine($"Display Name: {user.DisplayName}");
                    Console.WriteLine($"Principal Name: {user.UserPrincipalName}");
                    Console.WriteLine($"Type: {user.UserType}");
                    Console.WriteLine($"Mail: {user.OtherMails?.First()}");
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
## Quick AAD B2C User Management
The purpose of this is to demonstrate user management of User's in Azure AD B2C.

This is not a full application. It is just for demonstration purposes only.

### appsettings.json explained
```json
{
  "ApiAuthConfiguration": {
    "TenantId": "your_tenant_subdomain.onmicrosoft.com",
    "ApiClientId": "API Application (client) ID",
    "ApiClientSecret": "Create a secret for the Web API",
    "ExtensionAppId": "b2c-extensions-app Application (client) ID"
  },
  "WhatToProcess": {
    "AddUser": false,
    "AddClaim": false,
    "ListUsers": false,
    "AddClaimModel": {
      "UserId": "123456-",
      "AttributeName": "",
      "ClaimValue": ""
    },
    "AddUserModel": {
      "DisplayName": "",
      "GivenName": "",
      "Surname": "",
      "Email": ""
    }
  }
}
```
The ApiAuthConfiguration will come from Azure. See below on how to register the application.

The WhatToProcess section is what you want the program to do:
* AddUser - Will add a user in the AddUserModel section
* AddClaim - Will add the claim to the user in the AddClaimModel. The UserId is the Id. The AttributeName is the **Custom Attribute**
* ListUsers - Will display all users in the directory.


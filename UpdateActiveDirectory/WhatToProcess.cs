namespace UpdateActiveDirectory
{
    public class WhatToProcess
    {
        public bool AddUser { get; set; }
        public bool AddClaim { get; set; }
        public bool ListUsers { get; set; }
        public AddClaimModel AddClaimModel { get; set; }
        public AddUserModel AddUserModel { get; set; }
    }

    public class AddClaimModel
    {
        public string UserId { get; set; }
        public string AttributeName { get; set; }
        public string ClaimValue { get; set; }
    }

    public class AddUserModel
    {
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
    }
}
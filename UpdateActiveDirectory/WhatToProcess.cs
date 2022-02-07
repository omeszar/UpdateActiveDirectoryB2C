namespace UpdateActiveDirectory
{
    public class WhatToProcess
    {
        public bool AddUser { get; set; }
        public bool AddClaim { get; set; }
        public bool ListUsers { get; set; }
        public bool DeleteUser { get; set; }
        public bool ChangeUserPassword { get; set; }
        public bool FindUser { get; set; }
        public AddClaimModel AddClaimModel { get; set; }
        public AddUserModel AddUserModel { get; set; }
        public DeleteUserModel DeleteUserModel { get; set; }
        public ChangeUserPasswordModel ChangeUserPasswordModel { get; set; }
        public FindUserModel FindUserModel { get; set; }
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
        public string Password { get; set; }
    }

    public class DeleteUserModel
    {
        public string UserId { get; set; }
    }

    public class ChangeUserPasswordModel
    {
        public string UserId { get; set; }
        public string NewPassword { get; set; }
    }

    public class FindUserModel
    {
        public string SearchString { get; set; }
    }
}
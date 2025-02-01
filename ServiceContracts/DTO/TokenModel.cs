using System;

namespace ServiceContracts.DTO
{
    public class TokenModel
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace si.birokrat.next.common_proxy_core.authorization {
    public static class Authorizer {
        public static dynamic CreateBearerAccessToken(int expiresIn, string securityKey, string issuer, string audience, Claim[] claims = null) {
            var expirationDate = expiresIn > 0 ? DateTime.Now.AddSeconds(expiresIn) : (DateTime?)null;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            if (claims == null) {
                claims = new Claim[] { };
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expirationDate,
                signingCredentials: credentials
            );

            return new {
                access_token = new JwtSecurityTokenHandler().WriteToken(token),
                expires_in = expiresIn,
                token_type = "Bearer"
            };
        }

        public static MyToken CreateBearerAccessTokenObject(int expiresIn, string securityKey, string issuer, string audience, Claim[] claims = null) {
            var expirationDate = expiresIn > 0 ? DateTime.Now.AddSeconds(expiresIn) : (DateTime?)null;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            if (claims == null) {
                claims = new Claim[] { };
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expirationDate,
                signingCredentials: credentials
            );

            return new MyToken(new JwtSecurityTokenHandler().WriteToken(token), expiresIn, "Bearer");
        }

    }

    public class MyToken {
        string access_token;
        int expiresIn;
        string token_type;

        public string Token_type { get => token_type; set => token_type = value; }
        public int ExpiresIn { get => expiresIn; set => expiresIn = value; }
        public string Access_token { get => access_token; set => access_token = value; }

        public MyToken(string access_token, int expiresIn, string token_type) {
            this.access_token = access_token;
            this.expiresIn = expiresIn;
            this.token_type = token_type;
        }
    }
}

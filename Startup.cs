using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(AwsDemo.Startup))]

namespace AwsDemo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            var OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(1), // 要件に応じて期限を
                AllowInsecureHttp = true
            };
            app.UseOAuthBearerTokens(OAuthOptions);
        }
    }

    class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private string UserId { get; set; }
        private string UserName { get; set; }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult(0);
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            if (IsValid(context.UserName, context.Password))
            {
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaims(new[]
                {
                    new Claim("UserId", UserId),
                    new Claim("UserName", UserName)
                });
                context.Validated(identity);
            }
            else
            {
                context.Rejected();
            }
            return Task.FromResult(0);
        }

        private bool IsValid(string id, string password)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand())
            {
                con.Open();
                cmd.Connection = con;
                cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                var sql = "SELECT name FROM users WHERE id = @id AND password = @password;";
                cmd.CommandText = sql;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@id", SqlDbType.Char, 7).Value = id;
                cmd.Parameters.Add("@password", SqlDbType.VarChar, 50).Value = password;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        UserId = id;
                        UserName = reader["name"].ToString();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AwsDemo.Controllers
{
    public class UsersController : ApiController
    {
        // POST: api/User
        public void Post([FromBody]JObject json)
        {
            try
            {
                var id = json["id"].Value<string>();
                var password = json["password"].Value<string>();
                var name = json["name"].Value<string>();
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = null;
                }
                var email = json["email"].Value<string>();
                if (string.IsNullOrWhiteSpace(email))
                {
                    email = null;
                }

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    var sql = "INSERT INTO users VALUES (@id, @password, @name, @email);";
                    cmd.CommandText = sql;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.Char, 7).Value = id;
                    cmd.Parameters.Add("@password", SqlDbType.VarChar, 100).Value = password;
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = (object)name ?? DBNull.Value;
                    cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = (object)email ?? DBNull.Value;
                    cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                }
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        // GET: api/User
        [Authorize]
        public JObject Get()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userId = identity.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
            var userName = identity.Claims.FirstOrDefault(c => c.Type == "UserName").Value;
            return new JObject {
                { "id", new JValue(userId) },
                { "name", new JValue(userName) }
            };
        }

        //// PUT: api/User
        //[Authorize]
        //public void Put([FromBody]JObject json)
        //{
        //}
    }
}

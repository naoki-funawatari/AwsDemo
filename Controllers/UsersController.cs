using Newtonsoft.Json.Linq;
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
        [Authorize]
        public JObject Post([FromBody]JObject json)
        {
            var id = json["id"].ToString();
            var password = json["password"].ToString();

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
                        return new JObject {
                            { "name", new JValue(reader["name"].ToString()) }
                        };
                    }
                }
            }

            throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
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

        // PUT: api/User
        public void Put([FromBody]JObject json)
        {
            try
            {
                var id = json["id"].ToString();
                var password = json["password"].ToString();
                var name = json["name"].ToString();

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    var sql = "INSERT INTO users VALUES (@id,  @password,  @name);";
                    cmd.CommandText = sql;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.Char, 7).Value = id;
                    cmd.Parameters.Add("@password", SqlDbType.VarChar, 50).Value = password;
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 20).Value = name;
                    cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                }
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }
    }
}

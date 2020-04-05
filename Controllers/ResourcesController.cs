using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AwsDemo.Controllers
{
    public class ResourcesController : ApiController
    {
        // GET: api/Resources
        public JObject Get([FromBody]JObject json)
        {
            var newDate = DateTime.Parse(json["newDate"].ToString());
            var view = json["view"].ToString();
            var action = json["action"].ToString();


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
                if (view == "month")
                {

                }
                if (view == "week")
                {

                }
                if (view == "day")
                {

                }
                if (view == "agenda")
                {

                }

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

        // GET: api/Resources/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Resources
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Resources/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Resources/5
        public void Delete(int id)
        {
        }
    }
}

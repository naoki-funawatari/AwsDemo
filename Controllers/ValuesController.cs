using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AwsDemo.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ValuesController : ApiController
    {
        // GET: api/Values
        public IEnumerable<string> Get()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand())
            {
                con.Open();
                cmd.Connection = con;
                cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                var sql = "SELECT id, password, name FROM users;";
                cmd.CommandText = sql;

                var users = new List<string>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add($"{reader["id"]} - {reader["name"]}");
                    }
                }
                return users;
            }
            //return new string[] { "value1", "value2" };
        }

        //// GET: api/Values/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Values
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Values/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Values/5
        //public void Delete(int id)
        //{
        //}
    }
}

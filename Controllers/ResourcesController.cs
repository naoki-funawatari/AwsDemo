using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AwsDemo.Controllers
{
    public class ResourcesController : ApiController
    {
        // GET: api/Resources
        [Authorize]
        public IEnumerable<JObject> Get()
        {
            var resouces = new List<JObject>();
            try
            {
                //var newDate = DateTime.Parse(json["newDate"].ToString());
                //var view = json["view"].ToString();
                //var action = json["action"].ToString();

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    var sql = "SELECT id, title FROM resources;";
                    cmd.CommandText = sql;
                    cmd.Parameters.Clear();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resouces.Add(new JObject {
                                { "id", new JValue(reader.GetInt32(0)) },
                                { "title", new JValue(reader.GetString(1)) },
                            });
                        }
                    }
                    //if (view == "month")
                    //{
                    //}
                    //if (view == "week")
                    //{
                    //}
                    //if (view == "day")
                    //{
                    //}
                    //if (view == "agenda")
                    //{
                    //}
                }

                return resouces;
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
        }

        //// GET: api/Resources/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Resources
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Resources/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Resources/5
        //public void Delete(int id)
        //{
        //}
    }
}

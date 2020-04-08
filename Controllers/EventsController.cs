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
    public class EventsController : ApiController
    {
        // GET: api/Events
        public IEnumerable<JObject> Get()
        {
            var events = new List<JObject>();
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    var sql = "SELECT id, title, all_day, [start], [end], resource_id FROM events;";
                    cmd.CommandText = sql;
                    cmd.Parameters.Clear();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            events.Add(new JObject {
                                { "id", new JValue(reader.GetInt32(0)) },
                                { "title", new JValue(reader.GetString(1)) },
                                { "allDay", new JValue(reader.GetBoolean(2)) },
                                { "start", new JValue(reader.GetDateTime(3)) },
                                { "end", new JValue(reader.GetDateTime(4)) },
                                { "resourceId", new JValue(reader.GetInt32(5)) },
                            });
                        }
                    }
                }

                return events;
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
        }

        //// GET: api/Events/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Events
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Events/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Events/5
        //public void Delete(int id)
        //{
        //}
    }
}

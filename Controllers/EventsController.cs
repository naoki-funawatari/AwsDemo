using Newtonsoft.Json.Linq;
using System;
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
        [Authorize]
        public IEnumerable<JObject> Get()
        {
            try
            {
                return GetEvents();
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        // POST: api/Events
        [Authorize]
        public IEnumerable<JObject> Post([FromBody]JObject json)
        {
            try
            {
                var title = json["title"].ToString();
                var allDay = bool.Parse(json["allDay"].ToString());
                var start = DateTime.Parse(json["start"].ToString()).ToLocalTime();
                var end = DateTime.Parse(json["end"].ToString()).ToLocalTime();
                var resourceId = int.Parse(json["resourceId"].ToString());

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    var sql = "INSERT INTO events VALUES (@title, @all_day, @start, @end, @resource_id);";
                    cmd.CommandText = sql;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@title", SqlDbType.NVarChar, 50).Value = title;
                    cmd.Parameters.Add("@all_day", SqlDbType.Bit).Value = allDay;
                    cmd.Parameters.Add("@start", SqlDbType.DateTime).Value = start;
                    cmd.Parameters.Add("@end", SqlDbType.DateTime).Value = end;
                    cmd.Parameters.Add("@resource_id", SqlDbType.Int).Value = resourceId;
                    cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                }

                return GetEvents();
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        // PUT: api/Events
        [Authorize]
        public IEnumerable<JObject> Put([FromBody]JObject json)
        {
            try
            {
                var id = int.Parse(json["id"].ToString());
                var title = json["title"].ToString();
                var allDay = bool.Parse(json["allDay"].ToString());
                var start = DateTime.Parse(json["start"].ToString()).ToLocalTime();
                var end = DateTime.Parse(json["end"].ToString()).ToLocalTime();
                var resourceId = int.Parse(json["resourceId"].ToString());

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    var sql =
                        " UPDATE events" +
                        " SET    title       = @title" +
                        "      , all_day     = @all_day" +
                        "      , [start]     = @start" +
                        "      , [end]       = @end" +
                        "      , resource_id = @resource_id" +
                        " WHERE  id          = @id;";
                    cmd.CommandText = sql;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@title", SqlDbType.NVarChar, 50).Value = title;
                    cmd.Parameters.Add("@all_day", SqlDbType.Bit).Value = allDay;
                    cmd.Parameters.Add("@start", SqlDbType.DateTime).Value = start;
                    cmd.Parameters.Add("@end", SqlDbType.DateTime).Value = end;
                    cmd.Parameters.Add("@resource_id", SqlDbType.Int).Value = resourceId;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                }

                return GetEvents();
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        // DELETE: api/Events
        [Authorize]
        public IEnumerable<JObject> Delete([FromBody]JObject json)
        {
            try
            {
                var id = int.Parse(json["id"].ToString());

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    var sql = "DELETE events WHERE id = @id;";
                    cmd.CommandText = sql;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                }

                return GetEvents();
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        /// <summary>
        /// イベントの一覧を取得します。
        /// </summary>
        /// <returns>イベントの一覧。</returns>
        private IEnumerable<JObject> GetEvents()
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
                        yield return new JObject {
                            { "id", new JValue(reader.GetInt32(0)) },
                            { "title", new JValue(reader.GetString(1)) },
                            { "allDay", new JValue(reader.GetBoolean(2)) },
                            { "start", new JValue(reader.GetDateTime(3)) },
                            { "end", new JValue(reader.GetDateTime(4)) },
                            { "resourceId", new JValue(reader.GetInt32(5)) },
                        };
                    }
                }
            }
        }
    }
}

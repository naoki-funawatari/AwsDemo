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
        public IEnumerable<JObject> Get([FromUri]string view, [FromUri]DateTime[] range, [FromBody]JObject json)
        {
            try
            {
                return GetEvents(view, range);
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        // POST: api/Events
        [Authorize]
        public IEnumerable<JObject> Post([FromUri]string view, [FromUri]DateTime[] range, [FromBody]JObject json)
        {
            try
            {
                var title = json["title"].Value<string>();
                var allDay = json["allDay"].Value<bool>();
                var start = json["start"].Value<DateTime>().ToLocalTime();
                var end = json["end"].Value<DateTime>().ToLocalTime();
                var resourceIds = json["resourceIds"].Values<int>();

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    cmd.CommandText = "INSERT INTO events VALUES (@title, @all_day, @start, @end);";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@title", SqlDbType.NVarChar, 50).Value = title;
                    cmd.Parameters.Add("@all_day", SqlDbType.Bit).Value = allDay;
                    cmd.Parameters.Add("@start", SqlDbType.DateTime).Value = start;
                    cmd.Parameters.Add("@end", SqlDbType.DateTime).Value = end;
                    cmd.ExecuteNonQuery();

                    // 新規に、イベントに対してリソースを登録する
                    cmd.CommandText = "INSERT INTO event_resources VALUES((SELECT IDENT_CURRENT('events')), @resource_id)";
                    foreach (var resourceId in resourceIds)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@resource_id", SqlDbType.Int).Value = resourceId;
                        cmd.ExecuteNonQuery();
                    }

                    cmd.Transaction.Commit();
                }

                return GetEvents(view, range);
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        // PUT: api/Events
        [Authorize]
        public IEnumerable<JObject> Put([FromUri]string view, [FromUri]DateTime[] range, [FromBody]JObject json)
        {
            try
            {
                var id = json["id"].Value<int>();
                var title = json["title"].Value<string>();
                var allDay = json["allDay"].Value<bool>();
                var start = json["start"].Value<DateTime>().ToLocalTime();
                var end = json["end"].Value<DateTime>().ToLocalTime();
                var resourceIds = json["resourceIds"].Values<int>();

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    cmd.CommandText =
                        " UPDATE events" +
                        " SET    title       = @title" +
                        "      , all_day     = @all_day" +
                        "      , [start]     = @start" +
                        "      , [end]       = @end" +
                        " WHERE  id          = @id;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@title", SqlDbType.NVarChar, 50).Value = title;
                    cmd.Parameters.Add("@all_day", SqlDbType.Bit).Value = allDay;
                    cmd.Parameters.Add("@start", SqlDbType.DateTime).Value = start;
                    cmd.Parameters.Add("@end", SqlDbType.DateTime).Value = end;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();

                    // 一度、イベントに紐づくリソースを全削除する。
                    cmd.CommandText = "DELETE event_resources WHERE event_id = @event_id;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@event_id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();

                    // 新規に、イベントに対してリソースを登録しなおす
                    cmd.CommandText = "INSERT INTO event_resources VALUES(@event_id, @resource_id)";
                    foreach (var resourceId in resourceIds)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@event_id", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@resource_id", SqlDbType.Int).Value = resourceId;
                        cmd.ExecuteNonQuery();
                    }

                    cmd.Transaction.Commit();
                }

                return GetEvents(view, range);
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        // DELETE: api/Events
        [Authorize]
        public IEnumerable<JObject> Delete([FromUri]string view, [FromUri]DateTime[] range, [FromBody]JObject json)
        {
            try
            {
                var id = json["id"].Value<int>();

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    cmd.CommandText = "DELETE events WHERE id = @id;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();

                    // 一度、イベントに紐づくリソースを全削除する。
                    cmd.CommandText = "DELETE event_resources WHERE event_id = @event_id;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@event_id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                }

                return GetEvents(view, range);
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
        private IEnumerable<JObject> GetEvents(string view, DateTime[] range)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand())
            {
                con.Open();
                cmd.Connection = con;
                cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                cmd.Parameters.Clear();
                cmd.CommandText = "";
                cmd.CommandText += " SELECT [events].id";
                cmd.CommandText += "      , [events].title";
                cmd.CommandText += "      , [events].all_day";
                cmd.CommandText += "      , [events].[start]";
                cmd.CommandText += "      , [events].[end]";
                cmd.CommandText += " 	 , event_resources.resource_id";
                cmd.CommandText += " FROM [events] INNER JOIN event_resources";
                cmd.CommandText += "   ON [events].id = event_resources.event_id";
                if (view == "day")
                {
                    cmd.CommandText += " WHERE events.id IN (";
                    cmd.CommandText += "     SELECT [id] FROM (";
                    cmd.CommandText += "         SELECT id, CAST([start] AS date)[d] FROM events UNION";
                    cmd.CommandText += "         SELECT id, CAST([end]   AS date)[d] FROM events";
                    cmd.CommandText += "     ) COND WHERE d = @date";
                    cmd.CommandText += " )";
                    cmd.Parameters.Add("@date", SqlDbType.Date).Value = range[0];
                }
                else if (view == "week")
                {
                    cmd.CommandText += " WHERE events.id IN (";
                    cmd.CommandText += "     SELECT [id] FROM (";
                    cmd.CommandText += "         SELECT id, CAST([start] AS date)[d] FROM events UNION";
                    cmd.CommandText += "         SELECT id, CAST([end]   AS date)[d] FROM events";
                    cmd.CommandText += "     ) COND WHERE d IN (@date0, @date1, @date2, @date3, @date4, @date5, @date6)";
                    cmd.CommandText += " )";
                    cmd.Parameters.Add("@date0", SqlDbType.Date).Value = range[0];
                    cmd.Parameters.Add("@date1", SqlDbType.Date).Value = range[1];
                    cmd.Parameters.Add("@date2", SqlDbType.Date).Value = range[2];
                    cmd.Parameters.Add("@date3", SqlDbType.Date).Value = range[3];
                    cmd.Parameters.Add("@date4", SqlDbType.Date).Value = range[4];
                    cmd.Parameters.Add("@date5", SqlDbType.Date).Value = range[5];
                    cmd.Parameters.Add("@date6", SqlDbType.Date).Value = range[6];
                }
                else if (view == "month")
                {
                    cmd.CommandText += " WHERE events.id IN (";
                    cmd.CommandText += "     SELECT [id] FROM (";
                    cmd.CommandText += "         SELECT id, [start] [d] FROM events UNION";
                    cmd.CommandText += "         SELECT id, [end]   [d] FROM events";
                    cmd.CommandText += "     ) COND WHERE d BETWEEN @date0 AND @date1";
                    cmd.CommandText += " )";
                    cmd.Parameters.Add("@date0", SqlDbType.DateTime).Value = range[0];
                    cmd.Parameters.Add("@date1", SqlDbType.DateTime).Value = range[1];
                }
                else if (view == "agenda")
                {
                    cmd.CommandText += " WHERE events.id IN (";
                    cmd.CommandText += "     SELECT [id] FROM (";
                    cmd.CommandText += "         SELECT id, [start] [d] FROM events UNION";
                    cmd.CommandText += "         SELECT id, [end]   [d] FROM events";
                    cmd.CommandText += "     ) COND WHERE d BETWEEN @date0 AND @date1";
                    cmd.CommandText += " )";
                    cmd.Parameters.Add("@date0", SqlDbType.DateTime).Value = range[0];
                    cmd.Parameters.Add("@date1", SqlDbType.DateTime).Value = range[1];
                }
                cmd.CommandText += " ;";
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

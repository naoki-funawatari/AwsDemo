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
    public class ResourcesController : ApiController
    {
        // GET: api/Resources
        [Authorize]
        public IEnumerable<JObject> Get()
        {
            try
            {
                return GetResources();
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        // POST: api/Resources
        [Authorize]
        public IEnumerable<JObject> Post([FromBody]JObject json)
        {
            try
            {
                var title = json["title"].Value<string>();
                var remarks = json["remarks"].Value<string>();

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    var sql = "INSERT INTO resources VALUES (@title, @resource_type_id, @remarks, null);";
                    cmd.CommandText = sql;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@title", SqlDbType.NVarChar, 20).Value = title;
                    cmd.Parameters.Add("@resource_type_id", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@remarks", SqlDbType.NVarChar, 50).Value = remarks;
                    cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                }

                return GetResources();
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        // PUT: api/Resources
        public IEnumerable<JObject> Put([FromBody]JObject json)
        {
            try
            {
                var id = json["id"].Value<int>();
                var title = json["title"].Value<string>();
                var remarks = json["remarks"].Value<string>();

                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    var sql =
                        " UPDATE resources" +
                        " SET    title   = @title" +
                        "      , remarks = @remarks" +
                        " WHERE  id      = @id;";
                    cmd.CommandText = sql;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@title", SqlDbType.NVarChar, 20).Value = title;
                    cmd.Parameters.Add("@remarks", SqlDbType.NVarChar, 50).Value = remarks;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                }

                return GetResources();
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        // DELETE: api/Resources
        public IEnumerable<JObject> Delete([FromBody]JObject json)
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

                    var sql =
                        " UPDATE resources" +
                        " SET    deleted = @deleted" +
                        " WHERE  id      = @id;";
                    cmd.CommandText = sql;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@deleted", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                }

                return GetResources();
            }
            catch
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
        }

        /// <summary>
        /// リソースの一覧を取得します。
        /// </summary>
        /// <returns>リソースの一覧。</returns>
        private IEnumerable<JObject> GetResources()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand())
            {
                con.Open();
                cmd.Connection = con;
                cmd.Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                var sql =
                    " SELECT resources.id," +
                    "        resources.title," +
                    "        resources.resource_type_id," +
                    "        resource_type.title resource_type_title," +
                    "        resources.remarks" +
                    " FROM resources INNER JOIN resource_type" +
                    "   ON resources.resource_type_id = resource_type.id" +
                    " WHERE resources.deleted IS null;";
                cmd.CommandText = sql;
                cmd.Parameters.Clear();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new JObject {
                            { "id", new JValue(reader.GetInt32(0)) },
                            { "title", new JValue(reader.GetString(1)) },
                            { "resource_type_id", new JValue(reader.GetInt32(2)) },
                            { "resource_type_title", new JValue(reader.GetString(3)) },
                            { "remarks", new JValue(reader.IsDBNull(4) ? string.Empty : reader.GetString(4) ) },
                        };
                    }
                }
            }
        }
    }
}

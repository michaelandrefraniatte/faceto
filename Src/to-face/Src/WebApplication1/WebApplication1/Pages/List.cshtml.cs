using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Web;
using Newtonsoft.Json;
using System.Net;
namespace WebApplication1.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class ListModel : PageModel
    {
        private readonly ILogger<ListModel> _logger;
        public ListModel(ILogger<ListModel> logger)
        {
            _logger = logger;
        }
        public void OnGet()
        {
        }
        System.Data.IDataReader selectQueryString;
        System.Data.IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Database=master;");
        //System.Data.IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Database=webapplication1db;uid=thisismyname;pwd=thisismypassword;");
        //System.Data.IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(@"Server=db831557172.hosting-data.io;Database=db831557172;uid=dbo831557172;pwd=thisismypassword;");
        public ActionResult OnPost(int pagecounter, string search)
        {
            try
            {
                pagecounter = pagecounter * 10;
                if (search.Trim().Length > 0)
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    System.Data.IDbCommand command = connection.CreateCommand();
                    string rqst = "SELECT Link, DateCreated = max(DateCreated), Channel = max(Channel), Title = max(Title), [User] = max([User]), [Type] = max([Type]) FROM Users WHERE CHARINDEX(@search, Channel) > 0 or CHARINDEX(@search, Title) > 0 GROUP BY Link ORDER BY max(DateCreated) desc offset @pagecounter rows fetch next 10 rows only";
                    command.CommandText = rqst;
                    System.Data.IDbDataParameter pagecounterParam = command.CreateParameter();
                    pagecounterParam.ParameterName = "@pagecounter";
                    pagecounterParam.Direction = System.Data.ParameterDirection.Input;
                    pagecounterParam.Value = pagecounter;
                    command.Parameters.Add(pagecounterParam);
                    System.Data.IDbDataParameter searchParam = command.CreateParameter();
                    searchParam.ParameterName = "@search";
                    searchParam.Direction = System.Data.ParameterDirection.Input;
                    searchParam.Value = search;
                    command.Parameters.Add(searchParam);
                    selectQueryString = command.ExecuteReader();
                    var rows = this.ConvertToDictionary(selectQueryString);
                    return new JsonResult(JsonConvert.SerializeObject(rows, Formatting.Indented));
                }
            }
            catch { connection.Close(); }
            return new JsonResult("");
        }
        private IEnumerable<Dictionary<string, object>> ConvertToDictionary(System.Data.IDataReader reader)
        {
            var columns = new List<string>();
            var rows = new List<Dictionary<string, object>>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i));
            }
            while (reader.Read())
            {
                rows.Add(columns.ToDictionary(column => column, column => reader[column]));
            }
            return rows;
        }
    }
}
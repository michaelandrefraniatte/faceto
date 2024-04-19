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
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public void OnGet()
        {
        }
        static string type = "";
        System.Data.IDataReader selectQueryString;
        System.Data.IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Database=master;");
        //System.Data.IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Database=webapplication1db;uid=thisismyname;pwd=thisismypassword;");
        //System.Data.IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(@"Server=db831557172.hosting-data.io;Database=db831557172;uid=dbo831557172;pwd=thisismypassword;");
        public ActionResult OnPost(string str, string strname, string strcom, int pagecounter, int define)
        {
            if (define == 0)
            {
                try
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    System.Data.IDbCommand command = connection.CreateCommand();
                    pagecounter = pagecounter * 30;
                    string rqst = "SELECT * FROM Users where CHARINDEX(@str, Link) > 0 ORDER BY DateCreated asc offset @pagecounter rows fetch next 30 rows only";
                    command.CommandText = rqst;
                    System.Data.IDbDataParameter strParam = command.CreateParameter();
                    strParam.ParameterName = "@str";
                    strParam.Direction = System.Data.ParameterDirection.Input;
                    strParam.Value = str;
                    command.Parameters.Add(strParam);
                    System.Data.IDbDataParameter pagecounterParam = command.CreateParameter();
                    pagecounterParam.ParameterName = "@pagecounter";
                    pagecounterParam.Direction = System.Data.ParameterDirection.Input;
                    pagecounterParam.Value = pagecounter;
                    command.Parameters.Add(pagecounterParam);
                    selectQueryString = command.ExecuteReader();
                    var rows = this.ConvertToDictionary(selectQueryString);
                    return new JsonResult(JsonConvert.SerializeObject(rows, Formatting.Indented));
                }
                catch { connection.Close(); }
            }
            if (define == 1)
            {
                try
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    string videoidfromlink = str.Replace(@"https://www.youtube.com/watch?v=", "");
                    string name = new WebClient().DownloadString("https://youtube.com/get_video_info?video_id=" + videoidfromlink);
                    string channel = DecodeChannelString(name);
                    string title = DecodeTitleString(name);
                    string user = DecodeUserString(name);
                    System.Data.IDbCommand command = connection.CreateCommand();
                    string rqst = "INSERT INTO Users (name, DateCreated, Link, Comment, Channel, Title, [User], [Type]) VALUES (@strname, getdate(), @str, @strcom, @channel, @title, @user, @type)";
                    command.CommandText = rqst;
                    System.Data.IDbDataParameter strnameParam = command.CreateParameter();
                    strnameParam.ParameterName = "@strname";
                    strnameParam.Direction = System.Data.ParameterDirection.Input;
                    strnameParam.Value = strname;
                    command.Parameters.Add(strnameParam); 
                    System.Data.IDbDataParameter strParam = command.CreateParameter();
                    strParam.ParameterName = "@str";
                    strParam.Direction = System.Data.ParameterDirection.Input;
                    strParam.Value = str;
                    command.Parameters.Add(strParam);
                    System.Data.IDbDataParameter strcomParam = command.CreateParameter();
                    strcomParam.ParameterName = "@strcom";
                    strcomParam.Direction = System.Data.ParameterDirection.Input;
                    strcomParam.Value = strcom;
                    command.Parameters.Add(strcomParam);
                    System.Data.IDbDataParameter channelParam = command.CreateParameter();
                    channelParam.ParameterName = "@channel";
                    channelParam.Direction = System.Data.ParameterDirection.Input;
                    channelParam.Value = channel;
                    command.Parameters.Add(channelParam);
                    System.Data.IDbDataParameter titleParam = command.CreateParameter();
                    titleParam.ParameterName = "@title";
                    titleParam.Direction = System.Data.ParameterDirection.Input;
                    titleParam.Value = title;
                    command.Parameters.Add(titleParam);
                    System.Data.IDbDataParameter userParam = command.CreateParameter();
                    userParam.ParameterName = "@user";
                    userParam.Direction = System.Data.ParameterDirection.Input;
                    userParam.Value = user;
                    command.Parameters.Add(userParam);
                    System.Data.IDbDataParameter typeParam = command.CreateParameter();
                    typeParam.ParameterName = "@type";
                    typeParam.Direction = System.Data.ParameterDirection.Input;
                    typeParam.Value = type;
                    command.Parameters.Add(typeParam);
                    command.ExecuteNonQuery();
                    return new JsonResult("");
                }
                catch { connection.Close(); }
            }
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
        private static string DecodeChannelString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            newUrl = newUrl.Replace("+", " ");
            string toBeSearched1 = $"ownerChannelName\":\"";
            try
            {
                string toBeSearched2 = $"\",\"liveBroadcastDetails";
                int pFrom = newUrl.IndexOf(toBeSearched1) + toBeSearched1.Length;
                int pTo = newUrl.LastIndexOf(toBeSearched2);
                return newUrl.Substring(pFrom, pTo - pFrom);
            }
            catch
            {
                string toBeSearched2 = $"\",\"uploadDate";
                int pFrom = newUrl.IndexOf(toBeSearched1) + toBeSearched1.Length;
                int pTo = newUrl.LastIndexOf(toBeSearched2);
                return newUrl.Substring(pFrom, pTo - pFrom);
            }
        }
        private static string DecodeTitleString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            newUrl = newUrl.Replace("+", " ");
            string toBeSearched1 = $"title\":\"";
            string toBeSearched2 = $"\",\"lengthSeconds";
            int pFrom = newUrl.IndexOf(toBeSearched1) + toBeSearched1.Length;
            int pTo = newUrl.LastIndexOf(toBeSearched2);
            return newUrl.Substring(pFrom, pTo - pFrom);
        }
        private static string DecodeUserString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            newUrl = newUrl.Replace("+", " ");
            string toBeSearched1 = $"http://www.youtube.com/user/";
            string toBeSearched2 = $"\",\"externalChannelId";
            int pFrom = newUrl.IndexOf(toBeSearched1) + toBeSearched1.Length;
            int pTo = newUrl.LastIndexOf(toBeSearched2);
            if (pTo - pFrom <= 255)
            {
                type = "user";
                return newUrl.Substring(pFrom, pTo - pFrom);
            }
            else
            {
                type = "channel";
                toBeSearched1 = $"http://www.youtube.com/channel/";
                toBeSearched2 = $"\",\"externalChannelId";
                pFrom = newUrl.IndexOf(toBeSearched1) + toBeSearched1.Length;
                pTo = newUrl.LastIndexOf(toBeSearched2);
                return newUrl.Substring(pFrom, pTo - pFrom);
            }
        }
    }
}

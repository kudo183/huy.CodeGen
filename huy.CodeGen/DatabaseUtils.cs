﻿using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace huy.CodeGen
{
    public static class DatabaseUtils
    {
        private static string serverName = ".";
        private static Dictionary<string, string> _typeMapping = new Dictionary<string, string>()
        {
            {"int", "int" },
            {"bit","bool" },
            {"date","System.DateTime" },
            {"datetime2","System.DateTime" },
            {"time","System.TimeSpan" },
            {"nvarchar", "string" }
        };

        public static List<string> ListTables(string dbName)
        {
            List<string> tables = new List<string>();

            var server = new Microsoft.SqlServer.Management.Smo.Server(serverName);
            var db = new Microsoft.SqlServer.Management.Smo.Database(server, dbName);
            db.Refresh();
            foreach (Microsoft.SqlServer.Management.Smo.Table item in db.Tables)
            {
                var upperFirstLetterName = item.Name[0].ToString().ToUpper() + item.Name.Substring(1);
                tables.Add(upperFirstLetterName);
            }
            return tables;
        }

        public static List<EntityProperty> ListColumnsOfTable(string dbName, string tableName)
        {
            var result = new List<EntityProperty>();

            var server = new Microsoft.SqlServer.Management.Smo.Server(serverName);
            var db = new Microsoft.SqlServer.Management.Smo.Database(server, dbName);
            var table = new Microsoft.SqlServer.Management.Smo.Table(db, tableName);
            table.Refresh();
            foreach (Microsoft.SqlServer.Management.Smo.Column item in table.Columns)
            {
                result.Add(new EntityProperty()
                {
                    PropertyType = _typeMapping[item.DataType.Name],
                    PropertyName = item.Name,
                    IsForeignKey = item.IsForeignKey
                });
            }
            return result;
        }

        public static string ConcatString(List<string> input)
        {
            var sb = new StringBuilder();
            foreach (var item in input)
            {
                sb.AppendLine(item);
            }
            return sb.ToString();
        }
    }
}
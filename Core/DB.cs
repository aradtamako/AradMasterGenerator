using Core.Master.Model;
using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Z.Dapper.Plus;

namespace Core
{
    public class DB
    {
        public static DB Instance { get; private set; } = new DB();

        static DB()
        {
            SupportColumnAttribute();
        }

        static void SupportColumnAttribute()
        {
            Assembly.GetAssembly(typeof(IDataModel))
                .GetTypes()
                .Where(t => typeof(IDataModel).IsAssignableFrom(t) && typeof(IDataModel) != t)
                .ToList()
                .ForEach(dataModelType =>
                {
                    Dapper.SqlMapper.SetTypeMap(
                        dataModelType,
                        new CustomPropertyTypeMap(
                            dataModelType,
                            (type, columnName) =>
                                type.GetProperties().FirstOrDefault(prop =>
                                    prop.Name == columnName ||
                                    prop.GetCustomAttributes(true)
                                        .OfType<ColumnAttribute>()
                                        .Any(attr => attr.Name == columnName)
                                )
                        )
                    );
                });
        }

        private MySqlConnection Connect()
        {
            var conn = new MySqlConnection(Config.Config.Instance.Database.ConnectionString);
            conn.Open();
            return conn;
        }

        public void ExecuteSQL(string sql)
        {
            using var conn = Connect();
            var command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void ExecuteSQL(string sql, object param)
        {
            using var conn = Connect();
            conn.Execute(sql, param);
        }

        public IEnumerable<T> Query<T>(string sql, object? param = null)
        {
            using var conn = Connect();
            return conn.Query<T>(sql, param);
        }

        public void Insert<T>(T item)
        {
            using var conn = Connect();
            conn.BulkInsert(item);
        }

        public void Update<T>(T item)
        {
            using var conn = Connect();
            conn.BulkUpdate(item);
        }

        public void Merge<T>(T item)
        {
            using var conn = Connect();
            conn.BulkMerge(item);
        }
    }
}
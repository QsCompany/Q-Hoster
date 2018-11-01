using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace SQL
{
    public interface ISQLUnit
    {
        string Name { get;  }
    }
    public abstract class SQLUnit<T> : ISQLUnit where T : class, ISQLUnit
    {
        public string Name { get; protected set; }
        public override string ToString()
        {
            return Name;
        }
        public static object GetValue(object value, object _default)
        {
            if (value == DBNull.Value) return _default;
            return value;
        }

        public Dictionary<string, T> Childrens { get; } = new Dictionary<string, T>();
        public T this[string name] => Childrens.TryGetValue(name.ToLowerInvariant(), out var child) ? child : null;
        public void Add(T table) => this.Childrens.Add(table.Name.ToLowerInvariant(), table);
    }
    public class SQLDatabase:SQLUnit<SQLSchema>
    {
        public SQLDatabase(DbConnection sql)
        {
            GrabbeSchemas(sql);
        }

        public string ConnectionString { get; }
        public Dictionary<string, SQLSchema> Schemas => Childrens;
        
        public void GrabbeSchemas(DbConnection db)
        {
            var df = db.GetSchema("databases");
            foreach (System.Data.DataRow r in df.Rows)
            {
                var v = r["database_name"] as string;
                switch (v)
                {
                    case "mysql":
                    case "information_schema":
                    case "":
                    case null:
                        continue;
                    default:
                        Add(new SQLSchema(this, r));
                        continue;
                }
            }
            GrabbeTables(db);
        }
        public void GrabbeTables(DbConnection db)
        {
            var df = db.GetSchema("tables");
            List<string> tables = new List<string>();
            foreach (System.Data.DataRow r in df.Rows)
            {
                var schemaName = r["table_schema"] as string;
                var schema = this[schemaName];
                if (schema == null) continue;
                schema.Add(new SQLTable(schema, r));
            }
            this.GrabbeColumns(db);
        }

        public void GrabbeColumns(DbConnection db)
        {
            var df = db.GetSchema("columns");
            List<string> tables = new List<string>();
            foreach (System.Data.DataRow r in df.Rows)
            {
                var schemaName = r["table_schema"] as string;
                var tableName = r["table_name"] as string;

                var schema = this[schemaName];
                if (schema == null) continue;
                var table = schema[tableName];
                if (table == null) continue;
                table.Add(new SQLColumn(table, r));
            }
        }
    }
    public class SQLSchema:SQLUnit<SQLTable>
    {
        public SQLDatabase Database { get; }

        public Dictionary<string, SQLTable> Tables => Childrens;
        
        public SQLSchema(SQLDatabase database,DataRow data)
        {
            Database = database;
            Name = data["database_name"] as string;
        }
    }
    
    public class SQLTable:SQLUnit<SQLColumn>
    {
        public SQLSchema Schema { get; }
        public Dictionary<string, SQLColumn> Columns => Childrens;
        public SQLTable(SQLSchema schema, DataRow data)
        {
            Schema = schema;
            Name = data["TABLE_NAME"] as string;
        }
        public SQLTable()
        {

        }
    }
    public class SQLColumn : ISQLUnit
    {
        public string DBType { get; }
        public SQLTable Table { get; }
        public Type Type => null;
        public int Ordinal { get; }
        public object Default { get; }
        public bool IsNullable { get; }
        public int CharMaxLength { get; }
        public bool IsKey { get; }

        public string Name { get; }

        public SQLColumn(SQLTable table,DataRow data)
        {
            Table = table;
            Name = data["COLUMN_NAME"] as string;
            DBType = data["COLUMN_TYPE"] as string;
            IsKey = (data["COLUMN_KEY"] as string == "PRI");
            Ordinal = (int)(long)SQLUnit<SQLColumn>.GetValue(data["ORDINAL_POSITION"], -1L);
            Default = SQLUnit<SQLColumn>.GetValue(data["COLUMN_DEFAULT"], null);
            IsNullable = (data["IS_NULLABLE"] as string) == "YES";
            CharMaxLength = (int)(long)SQLUnit<SQLColumn>.GetValue(data["CHARACTER_MAXIMUM_LENGTH"], -1L);
        }
        public override string ToString()
        {
            return $"{Name} {DBType} {(IsKey ? "PRIMARY KEY" : "")} ORDINAL {Ordinal} {(Default != null ? $"DEFAULT({Default})" : string.Empty)} " + (IsNullable ? " NULLABLE" : "");
        }
        public bool IsSameType(Server.DProperty property)
        {
            return true;
        }
    }
}
// https://forums.futura-sciences.com/physique/352711-relation-entre-joules-temperature-un-fil-de-cuivre.html

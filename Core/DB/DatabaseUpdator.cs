using System;
using System.Collections.Generic;
using System.Data;
using Server;
using System.Linq;

namespace models
{
    public class DatabaseUpdator
    {
        DataBaseStructure d;
        public static List<Path> ItemConstraints = new List<Path>();
        public static List<Path> ListConstraints = new List<Path>();
        public DatabaseUpdator(DataBaseStructure d) => this.d = d;
        private static object ps;
        private static object e;

        public void UpdateRow(DataRow t, object[] ps)
        {
            var e = t.GetProperties();
            var j = 0;
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps.Length != e.Length)
                {
                    DatabaseUpdator.e = e;
                    DatabaseUpdator.ps = ps;

                }
                var x = e[i];
                var val = ps[i - j];
                if (x.IsOptional) { j++;continue; }
                if (val == DBNull.Value) val = null;
                if (typeof(DataRow).IsAssignableFrom(x.Type))
                {
                    if (val != null)
                    {
                        var id = ((IConvertible)val).ToInt64(null);
                        var dr = d[x.Type].Get(id, false);
                        if (dr == null) ItemConstraints.Add(new Path() { Id = id, Owner = t, Property = x });
                        else t.set(x.Index, dr);
                    }
                }
                else if (typeof(DataTable).IsAssignableFrom(x.Type))
                {
                    ListConstraints.Add(new Path { Id = val == null ? -1 : ((IConvertible)val).ToInt64(null), Owner = t, Property = x });
                }
                else
                {
                    var c = x.Converter;
                    if (c.IsComplex)
                        ((ComplexConverter)x.Converter).ToCsValue(null, x, t, val);
                    else t.set(x.Index, ((BasicConverter)x.Converter).ToCsValue(val));
                }
            }
        }
                
        public int DropEntity()
        {
            var err = 0;
            var x = d.DB.SQL.GetSchema("Tables");
            foreach (System.Data.DataRow r in x.Rows)
            {
                var c = "DROP TABLE [dbo].[" + r.ItemArray[2] + "]";
                using (var s = d.DB.SQL.CreateCommand())
                    try
                    {
                        s.CommandText = c;
                        s.ExecuteNonQuery();
                    }
                    catch (Exception e) { MyConsole.WriteLine(e.Message); err++; }
            }
            return err;
        }
        public bool Update(DataTable  v,string tableName)
        {
            if (v == null) return false;
            var sql = d.DB.SQL;
            using (var cmd = sql.CreateCommand())
                try
                {
                    cmd.CommandText = "SELECT * from `" + tableName + "`";
                    v.Read(this, cmd.ExecuteReader(CommandBehavior.Default), true);

                    MyConsole.WriteLine($"N° Row In {tableName} :  {v.Count} ");
                }
                catch (Exception ee)
                {
                    MyConsole.WriteLine(ee.Message);
                    return false;
                }
            return true;
        }
        public void ExecConstraints()
        {
            foreach (var c in ItemConstraints)
                c.Property.OnUpload(d, c);
            foreach (var c in ListConstraints)
                c.Property.OnUpload(d, c);

            ItemConstraints.Clear();
            ListConstraints.Clear();
            Database.IsUploading = false;
        }
        public void Update()
        {
            Database.IsUploading = true;
            var sql = d.DB.SQL;
            var dps = d.GetProperties();
            using (var cmd = sql.CreateCommand())
                foreach (var dp in dps)
                {
                    if ((dp.Attribute & PropertyAttribute.UnUploadable) == PropertyAttribute.UnUploadable) continue;
                    var v = d.GetValue(dp) as DataTable;
                    if (v != null)
                        if (!Update(v, dp.Name))
                        {
                            var c = DataTable.CreateTable(dp.Name, DObject.GetProperties(dp.Type.BaseType.GetGenericArguments()[0]));
                            d.Exec(c);
                        }
                }
            ExecConstraints();
        }
    }

}
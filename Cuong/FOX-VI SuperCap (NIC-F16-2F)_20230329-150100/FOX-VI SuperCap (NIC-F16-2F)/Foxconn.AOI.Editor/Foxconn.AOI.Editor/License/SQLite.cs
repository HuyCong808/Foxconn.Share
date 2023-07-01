using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace Foxconn.AOI.Editor
{
    public class SQLite
    {
        private static object mutext = new object();
        private static readonly string _password = "nguyenquangtiep";
        private string firstConnectionString = string.Empty;
        private string connectionString = string.Empty;
        public string tableName = "table_license";
        private string dbpath = null;

        public SQLite(string dataSource)
        {
            dbpath = dataSource;
            firstConnectionString = string.Format("Data Source={0};Version={1};password={2}", dataSource, 3, null);
            connectionString = string.Format("Data Source={0};Version={1};password={2}", dataSource, 3, _password);
        }

        public void CreateDatabase()
        {
            try
            {
                FileInfo fileInfo = new FileInfo(dbpath);
                if (!fileInfo.Exists && !fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();
                if (File.Exists(dbpath))
                    return;
                SQLiteConnection.CreateFile(fileInfo.FullName);
                ResetPassword();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool ResetPassword()
        {
            lock (mutext)
            {
                using (SQLiteConnection connection = new SQLiteConnection(firstConnectionString))
                {
                    connection.Open();
                    using (new SQLiteCommand(connection))
                    {
                        try
                        {
                            connection.ChangePassword("NQT");
                            return true;
                        }
                        catch (SQLiteException ex)
                        {
                            return false;
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }

        public int ExecuteNonQuery(string commandText)
        {
            int num = 0;
            lock (mutext)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand(connection))
                    {
                        try
                        {
                            sqLiteCommand.CommandText = commandText;
                            num = sqLiteCommand.ExecuteNonQuery();
                        }
                        catch (SQLiteException ex)
                        {
                            Trace.WriteLine("ExecuteNonQuery:" + commandText);
                            return num;
                        }
                    }
                    connection.Close();
                }
            }
            return num;
        }

        public int ExecuteNonQuery(string sql, SQLiteParameter[] parameters)
        {
            int num = 0;
            lock (mutext)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand(connection))
                    {
                        sqLiteCommand.CommandText = sql;
                        if (parameters != null)
                            sqLiteCommand.Parameters.AddRange(parameters);
                        num = sqLiteCommand.ExecuteNonQuery();
                    }
                }
            }
            return num;
        }

        public void ExecuteNonQueryBatch(List<KeyValuePair<string, SQLiteParameter[]>> list)
        {
            lock (mutext)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch
                    {
                        throw;
                    }
                    using (SQLiteTransaction sqLiteTransaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand sqLiteCommand = new SQLiteCommand(connection))
                        {
                            try
                            {
                                foreach (KeyValuePair<string, SQLiteParameter[]> keyValuePair in list)
                                {
                                    sqLiteCommand.CommandText = keyValuePair.Key;
                                    if (keyValuePair.Value != null)
                                        sqLiteCommand.Parameters.AddRange(keyValuePair.Value);
                                    sqLiteCommand.ExecuteNonQuery();
                                }
                                sqLiteTransaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                sqLiteTransaction.Rollback();
                                throw;
                            }
                        }
                    }
                }
            }
        }

        public object ExecuteScalar(string sql)
        {
            lock (mutext)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand(connection))
                    {
                        try
                        {
                            connection.Open();
                            sqLiteCommand.CommandText = sql;
                            return sqLiteCommand.ExecuteScalar();
                        }
                        catch (SQLiteException ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

        public DataTable ExecuteDataTable(string sql)
        {
            lock (mutext)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
                    {
                        SQLiteDataAdapter sqLiteDataAdapter = new SQLiteDataAdapter(cmd);
                        DataTable dataTable = new DataTable();
                        sqLiteDataAdapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        public void UpdateDB(string EncyptStr, string machineID, string userRight, string trialTime)
        {
            lock (mutext)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteTransaction sqLiteTransaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand sqLiteCommand = new SQLiteCommand(connection))
                        {
                            try
                            {
                                sqLiteCommand.CommandText = string.Format("update {0} set EncyptedString='{1}',MachineID='{2}',UserRight='{3}',TrialTime='{4}' where ID=1", tableName, EncyptStr, machineID, userRight, trialTime);
                                sqLiteCommand.ExecuteNonQuery();
                                sqLiteTransaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                sqLiteTransaction.Rollback();
                                throw;
                            }
                        }
                    }
                }
            }
        }

        public SQLiteDataReader ExecuteReader(string sql, params SQLiteParameter[] parameters)
        {
            lock (mutext)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    using (SQLiteCommand sqLiteCommand = new SQLiteCommand(sql, connection))
                    {
                        try
                        {
                            if ((uint)parameters.Length > 0U)
                                sqLiteCommand.Parameters.AddRange(parameters);
                            connection.Open();
                            return sqLiteCommand.ExecuteReader(CommandBehavior.CloseConnection);
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }
            }
        }

        public DataTable GetSchema()
        {
            lock (mutext)
            {
                using (SQLiteConnection sqLiteConnection = new SQLiteConnection(connectionString))
                {
                    try
                    {
                        sqLiteConnection.Open();
                        return sqLiteConnection.GetSchema("TABLES");
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
        }

        public void Test()
        {
            lock (mutext)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteTransaction sqLiteTransaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand sqLiteCommand = new SQLiteCommand(connection))
                        {
                            try
                            {
                                for (int index = 0; index < 10; ++index)
                                {
                                    sqLiteCommand.CommandText = string.Format("insert or ignore into table_product (ModelID,Exposure1,Exposure2) values ('{0}',{1},{2})", 'A', 100 + index, 1 + index);
                                    sqLiteCommand.ExecuteNonQuery();
                                }
                                sqLiteTransaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                sqLiteTransaction.Rollback();
                                throw;
                            }
                        }
                    }
                }
            }
        }
    }
}

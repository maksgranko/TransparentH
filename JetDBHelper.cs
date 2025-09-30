using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;

namespace TransparentH
{
    public class JetDBHelper
    {
        private string _connectionString;

        public JetDBHelper(string dbFilePath)
        {
            _connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={dbFilePath};";
        }

        // Method to get all values from a specified table
        public List<dynamic[]> GetAllValues(string tableName)
        {
            List<dynamic[]> dataList = new List<dynamic[]>();

            try
            {
                using (OleDbConnection connection = new OleDbConnection(_connectionString))
                {
                    connection.Open();
                    string query = $"SELECT * FROM {tableName}";

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dynamic[] row = new dynamic[reader.FieldCount];
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[i] = reader.GetValue(i);
                                }
                                dataList.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return dataList;
        }

        // Method to get all table names in the database
        public List<string> GetAllTableNames()
        {
            List<string> tableNames = new List<string>();

            try
            {
                using (OleDbConnection connection = new OleDbConnection(_connectionString))
                {
                    connection.Open();
                    DataTable schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                    foreach (DataRow row in schemaTable.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        tableNames.Add(tableName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return tableNames;
        }
        public DataTable GetDataTable(string tableName)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (OleDbConnection connection = new OleDbConnection(_connectionString))
                {
                    connection.Open();
                    string query = $"SELECT * FROM {tableName}";

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return dataTable;
        }
    }

    
}
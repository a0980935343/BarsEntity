﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Barsix.BarsEntity
{
    public static class StringExt
    {
        public static string CamelToSnake(this string source)
        {
            List<string> words = new List<string>();
            int prev = 0;

            for (int i = 1; i < source.Length; i++)
            {
                if (source.Substring(i, 1) == source.Substring(i, 1).ToUpper())
                {
                    words.Add(source.Substring(prev, i - prev).ToUpper());

                    prev = i;
                }
            }

            if (prev < source.Length)
            {
                words.Add(source.Substring(prev, source.Length - prev).ToUpper());
            }

            return string.Join("_", words);
        }

        public static string F(this string template, params object[] phList)
        {
            return string.Format(template, phList);
        }

        public static string camelCase(this string source)
        {
            return source.Substring(0, 1).ToLower() + source.Substring(1);
        }

        public static string Q(this string source, string quotes)
        {
            if (quotes.Length == 1)
                return quotes + source + quotes;
            else if (quotes.Length == 2)
                return quotes.Substring(0, 1) + source + quotes.Substring(1);
            else
                throw new ArgumentException();
        }

        public static string Ind(this string source, int indent)
        {
            string indPart = "    ";
            for (int i = 0; i < indent; i++)
                source = indPart + source;

            return source;
        }
    }
}
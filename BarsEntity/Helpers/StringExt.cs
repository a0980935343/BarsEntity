using System;
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
                if (source.Substring(i, 1) == source.Substring(i, 1).ToUpper() && source.Substring(i-1, 1) == source.Substring(i-1, 1).ToLower())
                {
                    words.Add(source.Substring(prev, i - prev).ToUpper());

                    prev = i;
                } else
                if (source.Substring(i, 1) == source.Substring(i, 1).ToUpper() && source.Length >= i+2 && source.Substring(i + 1, 1) == source.Substring(i + 1, 1).ToLower())
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

        public static string CutFirst(this string str, int length)
        {
            return str.Substring(length);
        }

        public static string CutLast(this string str, int length)
        {
            return str.Substring(0, str.Length - length);
        }

        public static string GetFirst(this string str, int length)
        {
            return str.Substring(0, length);
        }

        public static string GetLast(this string str, int length)
        {
            return str.Substring(str.Length - length, length);
        }

        public static string R(this string template, params object[] phList)
        {
            string result = template;

            for (int i = 0; i < phList.Length; i++)
            {
                result = result.Replace("{" + i + "}", phList[i] == null ? string.Empty : phList[i].ToString());
            }

            return result.Replace("{{", "{").Replace("}}", "}");
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
            var left = quotes[0];
            var right = quotes[quotes.Length - 1];

            return left + source + right;
        }

        public static string Ind(this string source, int indent)
        {
            string indPart = "    ";
            for (int i = 0; i < indent; i++)
                source = indPart + source;

            return source;
        }

        public static string Untag(this string source, string tag)
        {
            var open = "<" + tag + ">";
            var close = "</" + tag + ">";
            var str = source.Trim();
            if (str.IndexOf(open) >= 0 && str.IndexOf(close) > 0 && (str.IndexOf(open) < str.IndexOf(close)))
                return str.Substring(str.IndexOf(open) + tag.Length + 2, str.IndexOf(close) - (str.IndexOf(open) + tag.Length + 2));
            else
                return source;
        }

        public static string Tag(this string source, string tag)
        {
            return "<" + tag + ">" + source + "</" + tag + ">";
        }

        public static string Unwrap(this string source, string bounds)
        { 
            var left = bounds[0];
            var right = bounds[bounds.Length-1];

            return source.Right(left).Left(right);
        }

        public static string Left(this string source, string seporator)
        {
            return source.Substring(0, source.IndexOf(seporator));
        }

        public static string Right(this string source, string seporator)
        {
            return source.Substring(source.IndexOf(seporator) + seporator.Length);
        }

        public static string Left(this string source, char seporator)
        {
            return source.Substring(0, source.IndexOf(seporator));
        }

        public static string Right(this string source, char seporator)
        {
            return source.Substring(source.IndexOf(seporator) + 1);
        }
    }
}

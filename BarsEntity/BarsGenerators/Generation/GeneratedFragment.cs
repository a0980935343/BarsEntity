using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Barsix.BarsEntity.BarsGenerators
{
    public class GeneratedFragment
    {
        public IBarsGenerator Generator;

        public string FileName;
        public string FilePath;
        public bool InsertToFile;
        public string InsertClass;
        public string InsertMethod;

        public List<string> Lines = new List<string>();

        public List<string> GetStringList()
        {
            var result = new List<string>();

            if (Generator != null)
                result.Add("///   " + Generator.GetType().Name);

            result.AddRange(Lines);
            result.Add("");

            return result;
        }

        public void Insert(ProjectInfo project)
        {
            if (!InsertToFile)
                return;

            var fileName = Path.Combine(project.RootFolder, Path.Combine(FilePath ?? "", FileName));

            if (!File.Exists(fileName))
                return;
            
            var lines = File.ReadAllLines(fileName);

            int classStart = 0;
            int classScopeStart = 0;

            int methodStart = 0;
            int methodScopeStart = 0;
            int methodScopeEnd = 0;

            for(var i = 0; i < lines.Length; i++)
            {
                if (classStart == 0)
                {
                    if (lines[i].Length >= InsertClass.Length+4 && lines[i].Substring(4, InsertClass.Length) == InsertClass)
                    {
                        classStart = i;
                        continue;
                    }
                }
                else
                if (classScopeStart == 0)
                {
                    if (lines[i].Length >= 5 && lines[i].Substring(0, 5).Trim() == "{")
                    {
                        classScopeStart = i;
                        continue;
                    };
                }
                else
                if (methodStart == 0)
                {
                    if (lines[i].Length >= InsertMethod.Length + 8 && lines[i].Substring(8, InsertMethod.Length) == InsertMethod)
                    {
                        methodStart = i;
                        continue;
                    };
                }                
                else
                if (methodScopeStart == 0)
                {
                    if (lines[i].Length >= 9 && lines[i].Substring(0, 9).Trim() == "{")
                    {
                        methodScopeStart = i;
                        continue;
                    };
                }
                else
                if (methodScopeEnd == 0)
                {
                    if (lines[i].Length >= 9 && lines[i].Substring(0, 9).Trim() == "}")
                    {
                        methodScopeEnd = i;
                        break;
                    };
                }
            }

            if (methodScopeEnd == 0)
                return;

            var list = lines.ToList();
            list.Insert(methodScopeEnd, "");
            var fragmentLines = Lines.ToList();
            fragmentLines.Reverse();

            foreach (var fragmentLine in fragmentLines)
            {
                list.Insert(methodScopeEnd + 1, fragmentLine.Ind(3));
            }

            File.WriteAllLines(fileName, list.ToArray());
        }
    }
}

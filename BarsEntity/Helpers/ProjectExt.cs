using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using EnvDTE;
using VSLangProj;

namespace Barsix.BarsEntity
{
    public static class ProjectExt
    {
        public static bool HasReference(this Project project, string referenceName)
        {
            var vsproject = project.Object as VSProject;

            foreach (Reference reference in vsproject.References)
            {
                if (reference.Name == referenceName)
                    return true;
            }

            return false;
        }

        public static string RootFolder(this Project project)
        {
            Property prop = null;

            try
            {
                prop = project.Properties.Item("FullPath");
            }
            catch (ArgumentException)
            {
                try
                {
                    prop = project.Properties.Item("ProjectDirectory");
                }
                catch (ArgumentException)
                {
                    prop = project.Properties.Item("ProjectPath");
                }
            }

            if (prop != null)
            {
                string value = prop.Value.ToString();

                if (File.Exists(value))
                {
                    return Path.GetDirectoryName(value);
                }
                else if (Directory.Exists(value))
                {
                    return value;
                }

                throw new ArgumentException("Не найден путь проекта " + project.Name);
            }
            else
            {
                throw new ArgumentException("Не найден путь проекта " + project.Name);
            }
        }

        public static string DefaultNamespace(this Project project)
        {
            return project.Properties.Item("DefaultNamespace").Value.ToString();
        }

        public static Dictionary<string, CodeClass> GetClassList(this Project project, string filter = "")
        {
            var classes = new Dictionary<string, CodeClass>();

            Func<CodeNamespace, Dictionary<string, CodeClass>> enumClasses = null;
            enumClasses = ns =>
            {
                var list = new Dictionary<string, CodeClass>();

                if (!ns.FullName.StartsWith(filter))
                    return list;

                foreach (CodeNamespace ens in ns.Members.OfType<CodeNamespace>())
                {
                    var nsClasses = enumClasses(ens);
                    foreach(var cls in nsClasses)
                        list.Add(cls.Key, cls.Value);
                }

                foreach (CodeClass @class in ns.Members.OfType<CodeClass>())
                {
                    if (@class.FullName.StartsWith(project.DefaultNamespace()))
                        try
                        {
                            list.Add(@class.FullName.Substring(project.DefaultNamespace().Length + 1), @class);
                        }
                        catch 
                        {
                            var x = 1;
                        }
                    else
                        try
                        {
                            list.Add(@class.FullName.Substring(filter.Length+1), @class);
                        }
                        catch 
                        {
                            var x = 1;
                        }
                }

                return list;
            };

            foreach (CodeNamespace element in project.CodeModel.CodeElements.OfType<CodeNamespace>())
            {
                var nsClasses = enumClasses(element);
                foreach (var cls in nsClasses)
                    classes.Add(cls.Key, cls.Value);
            }
            return classes;
        }
    }
}

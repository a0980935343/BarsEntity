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

        public static void GetClassList(this Project project, IDictionary<string, CodeClass> classes, ICollection<string> enums, string filter = "")
        {
            Action<CodeNamespace> enumClasses = null;
            enumClasses = ns =>
            {
                foreach (CodeNamespace ens in ns.Members.OfType<CodeNamespace>())
                {
                    enumClasses(ens);
                }

                foreach (CodeClass @class in ns.Members.OfType<CodeClass>())
                {
                    if (@class.FullName.StartsWith(project.DefaultNamespace()))
                    {
                        if (!classes.ContainsKey(@class.FullName.Substring(project.DefaultNamespace().Length + 1)))
                            classes.Add(@class.FullName.Substring(project.DefaultNamespace().Length + 1), @class);
                    }
                    else
                    {
                        if (!classes.ContainsKey(@class.FullName.Substring(filter.Length + 1)))
                            classes.Add(@class.FullName.Substring(filter.Length + 1), @class);
                    }
                }

                foreach (CodeEnum @class in ns.Members.OfType<CodeEnum>())
                {
                    if (@class.FullName.StartsWith(project.DefaultNamespace()))
                    {
                        enums.Add(@class.FullName.Substring(project.DefaultNamespace().Length + 1));
                    }
                    else
                    {
                        enums.Add(@class.FullName.Substring(filter.Length + 1));
                    }
                }
            };

            foreach (CodeNamespace element in project.CodeModel.CodeElements.OfType<CodeNamespace>().Where(x => x.FullName.StartsWith(filter)))
            {
                enumClasses(element);
            }
        }
    }
}

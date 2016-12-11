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

        public static ProjectProfileBase GetProjectProfile(this Project project)
        {
            if (project.DefaultNamespace().StartsWith("Bars.MosKs"))
                return new MosKsProfile();
            else
                return new EmptyProfile();
        }

        public static void GetClassList(this Project project, IDictionary<string, CodeClass> classes, 
            ICollection<string> enums, string filter, IEnumerable<string> bases)
        {
            //List<string> bases = new List<string>{ "PersistentObject", "BaseEntity", "NamedBaseEntity"};

            Action<CodeNamespace, int> enumClasses = null;
            enumClasses = (ns, level) =>
            {
                var nsList = filter.Split('.').ToList();
                var nsFilter = level < nsList.Count ? string.Join(".", nsList.Take(level)) : filter;

                foreach (CodeNamespace ens in ns.Members.OfType<CodeNamespace>().Where(x => x.FullName.StartsWith(nsFilter)))
                {
                    enumClasses(ens, level + 1);
                }

                foreach (CodeClass @class in ns.Members.OfType<CodeClass>())
                {
                    if (@class.FullName.StartsWith(filter))
                    {
                        bool isEntity = false;
                        if (bases != null)
                        {
                            foreach (CodeClass bs in @class.Bases.OfType<CodeClass>())
                            {
                                if (bases.Contains(bs.Name)) // base class match
                                {
                                    isEntity = true;
                                }
                                else
                                {
                                    foreach (CodeClass bsbs in bs.Bases.OfType<CodeClass>())
                                    {
                                        if (bases.Contains(bsbs.Name)) // or grand-base class match
                                        {
                                            isEntity = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            isEntity = true;
                        }

                        if (!classes.ContainsKey(@class.FullName.Substring(filter.Length + 1)) && isEntity)
                            classes.Add(@class.FullName.Substring(filter.Length + 1), @class);
                    }
                }

                foreach (CodeEnum @class in ns.Members.OfType<CodeEnum>())
                {
                    if (@class.FullName.StartsWith(filter))
                    {
                        enums.Add(@class.FullName.Substring(filter.Length + 1));
                    }
                }
            };

            var filterPrefix = filter;
            if (filterPrefix.Contains('.'))
                filterPrefix = filter.Substring(0, filterPrefix.IndexOf('.'));

            foreach (CodeNamespace element in project.CodeModel.CodeElements.OfType<CodeNamespace>().Where(x => x.FullName.StartsWith(filterPrefix)))
            {
                enumClasses(element, 2);
            }
        }

        public static ProjectItem AddItem(this Project project, string filePath, string fileName, string fileBody, Dictionary<string, object> properties)
        {
            if (!Directory.Exists(Path.Combine(project.RootFolder(), filePath ?? string.Empty)))
            {
                Directory.CreateDirectory(Path.Combine(project.RootFolder(), filePath ?? string.Empty));
            }

            string fullPath = Path.Combine(project.RootFolder(), filePath ?? string.Empty, fileName);

            File.WriteAllText(fullPath, fileBody, Encoding.UTF8);

            var projectItem = project.ProjectItems.AddFromFile(fullPath);

            foreach (Property prop in projectItem.Properties)
            {
                if (properties.ContainsKey(prop.Name))
                    prop.Value = properties[prop.Name];
            }
            return projectItem;
        }
    }
}

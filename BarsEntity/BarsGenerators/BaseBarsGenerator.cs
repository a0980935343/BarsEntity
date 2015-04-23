using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class BaseBarsGenerator : IBarsGenerator
    {
        protected Project _project;
        protected EntityOptions _options;
        protected string _projectFolder;

        public virtual void Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            _project = project;
            _options = options;
            _projectFolder = project.RootFolder();
        }

        protected void CheckFolder(string folder)
        {
            if (!Directory.Exists(Path.Combine(_projectFolder, folder)))
            {
                Directory.CreateDirectory(Path.Combine(_projectFolder, folder));
            }
        }

        protected bool FileExists(string name)
        {
            return File.Exists(Path.Combine(_projectFolder, name));
        }

        protected ProjectItem CreateFile(string file, string content)
        {
            Encoding encoding = Encoding.UTF8;
            string fullPath = Path.Combine(_projectFolder, file);
            File.WriteAllText(fullPath, content, encoding);

            return _project.ProjectItems.AddFromFile(fullPath);
        }

       
    }
}

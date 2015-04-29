﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using EnvDTE;

namespace Barsix.BarsEntity
{
    using BarsGenerators;
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class GenerationManager 
    {
        private Project _project;

        private List<IBarsGenerator> _generators = new List<IBarsGenerator>();

        private Dictionary<Type, GeneratedFile> _files = new Dictionary<Type, GeneratedFile>();

        private GeneratedFragments _fragments;

        private static Dictionary<string, CodeClass> _classList = new Dictionary<string, CodeClass>();

        private static List<string> _enumList = new List<string>();
        
        public GenerationManager(Project project, bool classSearching = true)
        {
            _project = project;

            if (classSearching && _classList.Count == 0)
            {
                Task.Factory.StartNew(() =>
                {
                    _project.GetClassList(_classList, _enumList, "Bars");
                    _generators.ForEach(g => g.ClassList = _classList.Keys.ToList().Concat(_enumList).ToList());
                });
            }
        }

        public Dictionary<string, EntityOptions> ClassExists(string className)
        {
            return _classList.Where(x => x.Key.EndsWith("." + className)).ToDictionary(x => x.Key, x => CodeClassExt.ToOptions(x.Value, _enumList));
        }
        
        public string EnumExists(string enumName)
        {
            return _enumList.FirstOrDefault(x => x.EndsWith("." + enumName));
        }

        private void AddFile(IBarsGenerator generator, GeneratedFile file)
        {
            if (_files.ContainsKey(generator.GetType()))
                _files[generator.GetType()] = file;
            else
                _files.Add(generator.GetType(), file);
        }

        public void AddGenerator(IBarsGenerator generator)
        {
            if (!_generators.Any(x => x.GetType() == generator.GetType()))
            {
                generator.ClassList = _classList.Keys.Concat(_enumList).ToList();
                _generators.Add(generator);
            }
        }

        public void Generate(EntityOptions options)
        {
            var result = new GenerationResult();
            _fragments = new GeneratedFragments();
            var projectInfo = new ProjectInfo{ RootFolder = _project.RootFolder(), DefaultNamespace = _project.DefaultNamespace()};
            foreach (var generator in _generators)
            {
                try
                {
                    AddFile(generator, generator.Generate(projectInfo, options, _fragments));
                }
                catch (Exception ex)
                {
                    result.Errors.Add(generator.GetType().Name + ": " + ex.Message + "\n\n" + ex.StackTrace);
                }
            }
        }

        public Dictionary<IBarsGenerator, ProjectItem> AddToProject(EntityOptions options, List<Type> generatorTypes)
        {
            var result = new Dictionary<IBarsGenerator, ProjectItem>();

            Generate(options);

            Encoding encoding = Encoding.UTF8;

            foreach (var file in _files.Where(x => generatorTypes.Contains(x.Key) && x.Value != null && x.Value.Name != null).Select(x => x.Value))
            {
                if (!Directory.Exists(Path.Combine(_project.RootFolder(), file.Path ?? string.Empty)))
                {
                    Directory.CreateDirectory(Path.Combine(_project.RootFolder(), file.Path ?? string.Empty));
                }

                string fullPath = Path.Combine(_project.RootFolder(), file.Path ?? string.Empty, file.Name);
                File.WriteAllText(fullPath, string.Join(Environment.NewLine, file.Body), encoding);
                var projectItem = _project.ProjectItems.AddFromFile(fullPath);

                foreach (Property prop in projectItem.Properties)
                {
                    if (file.Properties.ContainsKey(prop.Name))
                        prop.Value = file.Properties[prop.Name];
                }

                result.Add(file.Generator, projectItem);
            }

            return result;
        }

        public GeneratedFragments Fragments { get { return _fragments ?? new GeneratedFragments(); } }

        public IEnumerable<IBarsGenerator> Generators { get { return _generators; } }

        public IEnumerable<KeyValuePair<Type, GeneratedFile>> Files { get { return _files.ToList(); } }
    }
}

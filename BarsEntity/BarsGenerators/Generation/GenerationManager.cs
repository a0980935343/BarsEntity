using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;
    using Barsix.BarsEntity.Types;
    using EnvDTE80;

    public class GenerationManager 
    {
        private Project _project;

        private List<IBarsGenerator> _generators = new List<IBarsGenerator>();

        private Dictionary<Type, List<GeneratedFile>> _files = new Dictionary<Type, List<GeneratedFile>>();

        private GeneratedFragments _fragments;

        private static Dictionary<string, CodeClass> _classList = new Dictionary<string, CodeClass>();

        private static List<string> _enumList = new List<string>();

        public List<Type> UsedGenerators = new List<Type>();

        public GenerationManager(Project project, bool classSearching = true)
        {
            _project = project;

            if (classSearching) 
                FindClasses(false, false);
        }

        public void FindClasses(bool forceSearch, bool onlyInProject, Action<int> onComplete = null)
        {
            if (_classList.Count == 0 || forceSearch)
            {
                Task.Factory.StartNew(() =>
                {
                    _project.GetClassList(_classList, _enumList, 
                                          onlyInProject ? _project.DefaultNamespace() + ".Entities" : "Bars",
                                          new List<string> { "PersistentObject", "BaseEntity", "NamedBaseEntity" });

                    _generators.ForEach(g => g.CodeItemNameList = _classList.Keys.ToList().Concat(_enumList).ToList());
                    if (onComplete != null)
                        onComplete(_classList.Count);
                });
            }
        }

        public Dictionary<string, EntityOptions> ClassExists(string className)
        {
            return _classList.Where(x => x.Key.EndsWith("." + className)).ToDictionary(x => x.Key, x => CodeClassExt.ToOptions(x.Value, _enumList, _project));
        }
        
        public string EnumExists(string enumName)
        {
            return _enumList.FirstOrDefault(x => x.EndsWith("." + enumName));
        }

        private void AddFiles(IBarsGenerator generator, IEnumerable<GeneratedFile> files)
        {
            if (_files.ContainsKey(generator.GetType()))
                _files[generator.GetType()].AddRange(files);
            else
                _files.Add(generator.GetType(), files.ToList());
        }

        public void AddGenerator(IBarsGenerator generator)
        {
            if (!_generators.Any(x => x.GetType() == generator.GetType()))
            {
                generator.CodeItemNameList = _classList.Keys.Concat(_enumList).ToList();

                var request = generator.ClassRequest;
                if (request != null)
                {
                    var classes = new Dictionary<string, CodeClass>();
                    _project.GetClassList(classes, null, 
                                          _project.DefaultNamespace() + request.NamespaceSuffix, request.BaseElements);
                    generator.Classes = classes;
                }

                _generators.Add(generator);
            }
        }

        public void Generate(EntityOptions options, IEnumerable<Type> generatorTypes = null)
        {
            var result = new GenerationResult();
            _fragments = new GeneratedFragments();
            _files.Clear();

            var projectInfo = new ProjectInfo{ RootFolder = _project.RootFolder(), DefaultNamespace = _project.DefaultNamespace()};
            foreach (var generator in _generators)
            {
                if (generatorTypes != null && !generatorTypes.Contains(generator.GetType()))
                    continue;

                try
                {
                    var files = generator.Generate(projectInfo, options, _fragments);
                    if (files.Any())
                        AddFiles(generator, files);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(generator.GetType().Name + ": " + ex.Message + "\n\n" + ex.StackTrace);
                }
            }
        }

        public Dictionary<IBarsGenerator, ProjectItem> AddToProject(EntityOptions options)
        {
            var result = new Dictionary<IBarsGenerator, ProjectItem>();

            Generate(options, UsedGenerators);

            foreach (var file in _files.Where(x => UsedGenerators.Contains(x.Key) && x.Value != null && x.Value.Any())
                .SelectMany(x => x.Value)
                .Where(x => x.Name != null))
            {
                var projectItem = _project.AddItem(file.Path, file.Name, string.Join(Environment.NewLine, file.Body), file.Properties);

                /*if (!Directory.Exists(Path.Combine(_project.RootFolder(), file.Path ?? string.Empty)))
                {
                    Directory.CreateDirectory(Path.Combine(_project.RootFolder(), file.Path ?? string.Empty));
                }

                string fullPath = Path.Combine(_project.RootFolder(), file.Path ?? string.Empty, file.Name);
                
                File.WriteAllText(fullPath, string.Join(Environment.NewLine, file.Body), Encoding.UTF8);
                
                var projectItem = _project.ProjectItems.AddFromFile(fullPath);

                foreach (Property prop in projectItem.Properties)
                {
                    if (file.Properties.ContainsKey(prop.Name))
                        prop.Value = file.Properties[prop.Name];
                }*/

                result.Add(file.Generator, projectItem);
            }

            return result;
        }

        public void InsertFragments()
        {
            var projectInfo = new ProjectInfo { RootFolder = _project.RootFolder(), DefaultNamespace = _project.DefaultNamespace() };
            foreach (var generator in _generators)
            {
                if (UsedGenerators != null && !UsedGenerators.Contains(generator.GetType()))
                    continue;

                var fragments = _fragments.SelectMany(x => x.Value.Where(f => f.InsertToFile && f.Generator == generator));

                try
                {
                    foreach (var fragment in fragments)
                    {
                        fragment.Insert(projectInfo);
                    }
                }
                catch 
                {
                    
                }
            }
        }

        public GeneratedFragments Fragments { get { return _fragments ?? new GeneratedFragments(); } }

        public IEnumerable<IBarsGenerator> Generators { get { return _generators; } }

        public IEnumerable<KeyValuePair<Type, List<GeneratedFile>>> Files { get { return _files.ToList(); } }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class SignableEntitiesManifestGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files.First();

            if (!File.Exists(Path.Combine(project.RootFolder, "SignableEntitiesManifest.cs")))
            {
                var ns = new NamespaceInfo { Name = project.DefaultNamespace + ".Domain" };
                ns.InnerUsing.Add("B4.Modules.DigitalSignature");
                ns.InnerUsing.Add("Entities");

                ns.OuterUsing.Add("System.Collections.Generic");

                var cls = new ClassInfo { Name = "SignableEntitiesManifest" };
                cls.Interfaces.Add("ISignableEntitiesManifest");

                var classInfo = (new FieldInfo
                {
                    Name = options.ClassName + "Id",
                    Type = "string",
                    Value = "\"{0}\"".R(options.ClassFullName)
                })
                .Public.Const;

                _knownTypes.Clear();
                _knownTypes.Add(options.ClassName);
                _knownTypes.Add(cls.Name);
                _knownTypes.AddRange(cls.Interfaces);
                _knownTypes.Add("SignableEntityInfo");
                _knownTypes.Add("IEnumerable");

                cls.AddField(classInfo);

                var getAllInfo = new MethodInfo { Name = "GetAllInfo", Type = "IEnumerable<SignableEntityInfo>" };
                getAllInfo.Body.Add("return new[]");
                getAllInfo.Body.Add("{");
                getAllInfo.Body.Add("    new SignableEntityInfo({0}Id, \"{1}\", typeof({0}))".R(options.ClassName, options.DisplayName));
                getAllInfo.Body.Add("};");
                cls.AddMethod(getAllInfo);

                ns.NestedValues.Add(cls);

                var module = new GeneratedFragment
                {
                    FileName = "Module.cs",
                    InsertToFile = true,
                    InsertClass = "public class Module",
                    InsertMethod = "public override void Install()",
                    Generator = this
                };
                module.Lines.Add("Container.Register(Component.For<ISignableEntitiesManifest>().ImplementedBy<SignableEntitiesManifest>().LifestyleTransient());");

                fragments.Add("Module.cs", module);

                file.Name = "SignableEntitiesManifest.cs";
                file.Body = ns.Generate();
                return files;
            }
            else
            {
                _knownTypes.Add("SignableEntityInfo");

                var field = (new FieldInfo
                {
                    Name = options.ClassName + "Id",
                    Type = "string",
                    Value = "\"{0}\"".R(options.ClassFullName) 
                })
                .Public.Const.Generate(0).First();

                fragments.AddLines("SignableEntitiesManifest.cs", this, new List<string> { 
                    field, 
                    "", 
                    "    new SignableEntityInfo({0}Id, \"{1}\", typeof({0}))".R(options.ClassName, options.DisplayName) });

                file.Body.Add("/**");
                file.Body.Add(" *     Файл SignableEntitiesManifest.cs уже есть в проекте");
                file.Body.Add(" *     Вставьте в манифест строки ниже для регистрации новой сущности");
                file.Body.Add(" */");
                file.Body.Add("");

                fragments.First(x => x.Key == "SignableEntitiesManifest.cs").Value.ForEach(f =>{ file.Body.AddRange(f.Lines); } );
                return files;
            }
        }
    }
}

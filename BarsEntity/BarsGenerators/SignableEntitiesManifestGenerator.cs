using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class SignableEntitiesManifestGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);

            if (!File.Exists(Path.Combine(_projectFolder, "SignableEntitiesManifest.cs")))
            {
                var ns = new NamespaceInfo { Name = "{0}.Domain".F(project.Name) };
                ns.InnerUsing.Add("Bars.B4.Modules.DigitalSignature");
                ns.InnerUsing.Add("Entities");

                ns.OuterUsing.Add("System.Collections.Generic");

                var cls = new ClassInfo { Name = "SignableEntitiesManifest" };
                cls.Interfaces.Add("ISignableEntitiesManifest");

                var classInfo = (new FieldInfo
                {
                    Name = options.ClassName + "Id",
                    Type = "string",
                    Value = "\"{0}\"".F(options.ClassFullName)
                })
                .Public.Const;

                _knownTypes.Clear();
                _knownTypes.Add(cls.Name);
                _knownTypes.AddRange(cls.Interfaces);
                _knownTypes.Add("SignableEntityInfo");
                _knownTypes.Add("IEnumerable");

                cls.AddField(classInfo);

                var getAllInfo = new MethodInfo { Name = "GetAllInfo", Type = "IEnumerable<SignableEntityInfo>" };
                getAllInfo.Body.Add("return new[]");
                getAllInfo.Body.Add("{");
                getAllInfo.Body.Add("    new SignableEntityInfo({0}Id, \"{1}\", typeof({0}))".F(options.ClassName, options.DisplayName));
                getAllInfo.Body.Add("};");
                cls.AddMethod(getAllInfo);

                ns.NestedValues.Add(cls);
                                
                fragments.AddLines("Module.cs", this, new List<string> { 
                    "Container.Register(Component.For<ISignableEntitiesManifest>().ImplementedBy<SignableEntitiesManifest>().LifestyleTransient());"});

                file.Name = "SignableEntitiesManifest.cs";
                file.Body = ns.Generate();
                return file;
            }
            else
            {
                var field = (new FieldInfo
                {
                    Name = options.ClassName + "Id",
                    Type = "string",
                    Value = "\"{0}\"".F(options.ClassFullName) 
                })
                .Public.Const.Generate(0).First();

                fragments.AddLines("SignableEntitiesManifest.cs", this, new List<string> { 
                    field, 
                    "", 
                    "    new SignableEntityInfo({0}Id, \"{1}\", typeof({0}))".F(options.ClassName, options.DisplayName) });

                return null;
            }
        }
    }
}

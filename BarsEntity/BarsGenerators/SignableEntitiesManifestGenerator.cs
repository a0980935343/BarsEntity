using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class SignableEntitiesManifestGenerator : BaseBarsGenerator
    {
        public override void Generate(Project project, EntityOptions options)
        {
            base.Generate(project, options);

            if (!FileExists("SignableEntitiesManifest.cs"))
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

                cls.AddField(classInfo);

                var getAllInfo = new MethodInfo { Name = "GetAllInfo", Type = "IEnumerable<SignableEntityInfo>" };
                getAllInfo.Body.Add("return new[]");
                getAllInfo.Body.Add("{");
                getAllInfo.Body.Add("    new SignableEntityInfo({0}Id, \"{1}\", typeof({0}))".F(options.ClassName, options.DisplayName));
                getAllInfo.Body.Add("}");
                cls.AddMethod(getAllInfo);

                ns.NestedValues.Add(cls);

                CreateFile("SignableEntitiesManifest.cs", ns.ToString());
                
                DontForget.Add("Module.cs/Module/Install");
                DontForget.Add("Container.Register(Component.For<ISignableEntitiesManifest>().ImplementedBy<SignableEntitiesManifest>().LifestyleTransient());");
            }
            else
            {
                DontForget.Add("SignableEntitiesManifest.cs");

                var field = (new FieldInfo
                {
                    Name = options.ClassName + "Id",
                    Type = "string",
                    Value = "\"{0}\"".F(options.ClassFullName) 
                })
                .Public.Const.Generate(0).First();

                DontForget.Add(field);
                DontForget.Add("");
                DontForget.Add("SignableEntitiesManifest.cs/GetAllInfo");
                DontForget.Add("    new SignableEntityInfo({0}Id, \"{1}\", typeof({0}))".F(options.ClassName, options.DisplayName));
            }
        }
    }
}

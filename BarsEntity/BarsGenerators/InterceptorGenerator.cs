using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class InterceptorGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files.First();

            var ns = new NamespaceInfo() { Name = _project.DefaultNamespace + ".DomainServices" };
            var cls = new ClassInfo
            {
                Name = options.ClassName + "DomainServiceInterceptor",
                BaseClass = "EmptyDomainInterceptor<{0}>".R(options.ClassName)
            };
            ns.NestedValues.Add(cls);
            ns.InnerUsing.Add("B4");

            if (options.Stateful && options.Interceptor.Actions.Contains("BeforeCreate"))
            {
                ns.InnerUsing.Add("B4.Modules.States");
            }

            ns.InnerUsing.Add("Entities");

            _knownTypes.Clear();
            _knownTypes.Add(cls.Name);
            _knownTypes.Add("EmptyDomainInterceptor");
            _knownTypes.Add("IDomainService");
            _knownTypes.Add(options.ClassName);
            _knownTypes.Add("IStateProvider");

            foreach (string methodName in options.Interceptor.Actions)
            {
                var action = new MethodInfo 
                { 
                    Name = methodName + "Action", 
                    Type = "IDataResult", 
                    IsOverride = true,
                    Params = "IDomainService<{0}> service, {0} entity".R(options.ClassName),
                    Body = new List<string> { "return base.{0}Action(service, entity);".R(methodName) }
                };

                if (options.Stateful && methodName == "BeforeCreate")
                {
                    ns.InnerUsing.Add("B4.Modules.States");
                    action.Body.Clear();
                    action.Body.Add("var stateProvider = Container.Resolve<IStateProvider>();");
                    action.Body.Add("stateProvider.SetDefaultState(entity);");
                    action.Body.Add("return Success();");
                }
                cls.AddMethod(action);
            }

            fragments.AddLines("Module.cs", this, new List<string> { 
                "Container.Register(Component.For<IDomainServiceInterceptor<{0}>>().ImplementedBy<{0}DomainServiceInterceptor>().LifeStyle.Transient);".R(options.ClassName)});
                        
            file.Name = options.ClassName + "DomainServiceInterceptor.cs";
            file.Path = (Directory.Exists(Path.Combine(_project.RootFolder, "DomainService")) ? "DomainService" : "DomainServices") + (!string.IsNullOrWhiteSpace(options.Subfolder) ? "\\" + options.Subfolder : "");
            file.Body = ns.Generate();
            return files;
        }
    }
}

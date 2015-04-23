using System;
using System.Collections.Generic;
using System.Linq;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class InterceptorGenerator : BaseBarsGenerator
    {
        public override void Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            base.Generate(project, options, fragments);

            CheckFolder("DomainServices");

            var ns = new NamespaceInfo() { Name = project.Name + ".DomainServices" };
            var cls = new ClassInfo
            {
                Name = options.ClassName + "DomainServiceInterceptor",
                BaseClass = "EmptyDomainInterceptor<{0}>".F(options.ClassName)
            };
            ns.NestedValues.Add(cls);
            ns.InnerUsing.Add("Bars.B4");
            ns.InnerUsing.Add("Entities");

            InterceptorOptions dsOpts = options.Interceptor;
            foreach (string methodName in dsOpts.Actions)
            {
                var action = new MethodInfo 
                { 
                    Name = "{0}Action".F(methodName), 
                    Type = "IDataResult", 
                    IsOverride = true,
                    Params = "IDomainService<{0}> service, {0} entity".F(options.ClassName),
                    Body = new List<string> { "return base.{0}Action(service, entity);".F(methodName) }
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
                "Container.Register(Component.For<IDomainServiceInterceptor<{0}>>().ImplementedBy<{0}DomainServiceInterceptor>().LifeStyle.Transient);".F(options.ClassName)});

            var pi = CreateFile("DomainServices\\" + options.ClassName + "DomainServiceInterceptor.cs", ns.ToString());
        }
    }
}

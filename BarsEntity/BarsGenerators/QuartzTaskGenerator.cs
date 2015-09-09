using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.BarsGenerators
{
    using CodeGeneration.CSharp;

    public class QuartzTaskGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, BarsOptions.EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files.First();

            if (options.ClassName.EndsWith("Task"))
                options.ClassName = options.ClassName.CutLast(4);

            _knownTypes.Clear();
            _knownTypes.Add(options.ClassName + "Task");
            _knownTypes.Add("I" + options.ClassName + "Task");
            _knownTypes.Add("BaseTask");
            _knownTypes.Add("ITask");
            _knownTypes.Add("DateTime");

            NamespaceInfo ns = new NamespaceInfo(){ Name = project.DefaultNamespace + ".Tasks" };

            InterfaceInfo @interface = new InterfaceInfo { Name = "I" + options.ClassName + "Task" };
            @interface.Interfaces.Add("ITask");
            ns.NestedValues.Add(@interface);

            ClassInfo cls = new ClassInfo { Name = options.ClassName, BaseClass = "BaseTask", Interfaces = new List<string> { @interface.Name } };
            ns.NestedValues.Add(cls);

            var exec = new MethodInfo { Name = "Execute", IsOverride = true, Params = "DynamicDictionary @params" };
            exec.Body.Add("ExplicitSessionScope.CallInNewScope(() =>");
            exec.Body.Add("{");
            exec.Body.Add("");
            exec.Body.Add("});");
            cls.AddMethod(exec);

            ns.InnerUsing.Add("B4.Utils");
            ns.InnerUsing.Add("B4.Modules.Quartz");
            ns.InnerUsing.Add("B4.IoC.Lifestyles.SessionLifestyle");

            GeneratedFragment module = new GeneratedFragment();
            module.Generator = this;
            module.Lines.Add("Component.For<I" + options.ClassName + "Task>()");
            module.Lines.Add("    .Forward<ITask>()");
            module.Lines.Add("    .ImplementedBy<" + options.ClassName + "Task>()");
            module.Lines.Add("    .LifestyleTransient()");
            module.Lines.Add("    .RegisterIn(Container);");
            fragments.Add("Module.cs", module);

            GeneratedFragment dataAccessInstaller = new GeneratedFragment();
            dataAccessInstaller.Generator = this;
            dataAccessInstaller.Lines.Add("TriggerBuilder.Create()");
            dataAccessInstaller.Lines.Add("    .StartNow()");
            dataAccessInstaller.Lines.Add("    .WithCronSchedule(\"0 00 4 * * ?\")");
            dataAccessInstaller.Lines.Add("    .ScheduleTask<I" + options.ClassName + "Task>()");
            fragments.Add("DataAccessInstaller.cs", dataAccessInstaller);

            file.Name = options.ClassName + "Task.cs";
            file.Path = "Tasks";
            file.Body = ns.Generate();

            return files;
        }
    }
}

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

    public class DomainServiceGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);
            
            var ns = new NamespaceInfo() { Name = "{0}.DomainServices".F(project.Name) };
            var cls = new ClassInfo
            {
                Name = options.ClassName + "DomainService",
                BaseClass = "{0}DomainService<{1}>".F((options.AcceptFiles ? "FileStorage" : "Base"), options.ClassName)
            };
            ns.NestedValues.Add(cls);
            ns.OuterUsing.Add("System");
            ns.OuterUsing.Add("System.Collections.Generic");
            ns.OuterUsing.Add("System.Linq");

            ns.InnerUsing.Add("B4");
            if (options.AcceptFiles)
                ns.InnerUsing.Add("B4.Modules.FileStorage.DomainService");

            ns.InnerUsing.Add("Entities");

            _knownTypes.Clear();
            _knownTypes.Add(cls.Name);
            _knownTypes.Add("FileStorageDomainService");
            _knownTypes.Add("BaseDomainService");
            _knownTypes.Add(options.ClassName);
            _knownTypes.Add("List");
            _knownTypes.Add("IDataResult");
            _knownTypes.Add("BaseParams");
            _knownTypes.Add("IDomainServiceInterceptor");

            if (options.DomainService.Save)
            {
                var mi = new MethodInfo()
                {
                    IsOverride = true,
                    Name = "Save",
                    Type = "IDataResult",
                    Params = "BaseParams baseParams"
                };

                mi.Body.Add("var values = new List<{0}>();".F(options.ClassName));

                mi.Body.Add("InTransaction(() =>");
                mi.Body.Add("{");
                mi.Body.Add("    var saveParams = GetSaveParam(baseParams);");
                mi.Body.Add("    foreach (var record in saveParams.Records)");
                mi.Body.Add("    {");
                mi.Body.Add("        var value = record.AsObject();");
                mi.Body.Add("        SaveInternal(value);");
                mi.Body.Add("");
                mi.Body.Add("        values.Add(value);");
                mi.Body.Add("    }");
                mi.Body.Add("});");
                mi.Body.Add("");
                mi.Body.Add("return new SaveDataResult(values);");

                cls.AddMethod(mi);
            }

            if (options.DomainService.SaveInternal)
            {
                var mi = new MethodInfo()
                {
                    IsOverride = true,
                    Access = "protected",
                    Name = "SaveInternal",
                    Type = "void",
                    Params = "{0} value".F(options.ClassName)
                };

                mi.Body.Add("IDataResult result;");
                mi.Body.Add("var interceptors = Container.ResolveAll<IDomainServiceInterceptor<{0}>>();".F(options.ClassName));
                mi.Body.Add("foreach (var interceptor in interceptors)");
                mi.Body.Add("{");
                mi.Body.Add("    result = interceptor.BeforeCreateAction(this, value);");
                mi.Body.Add("    if (!result.Success)");
                mi.Body.Add("    {");
                mi.Body.Add("        throw new ValidationException(result.Message);");
                mi.Body.Add("    }");
                mi.Body.Add("}");
                mi.Body.Add("");
                mi.Body.Add("Repository.Save(value);");
                mi.Body.Add("");
                mi.Body.Add("foreach (var interceptor in interceptors)");
                mi.Body.Add("{");
                mi.Body.Add("    result = interceptor.AfterCreateAction(this, value);");
                mi.Body.Add("    if (!result.Success)");
                mi.Body.Add("    {");
                mi.Body.Add("        throw new ValidationException(result.Message);");
                mi.Body.Add("    }");
                mi.Body.Add("}");

                cls.AddMethod(mi);
            }

            if (options.DomainService.Update)
            {
                var mi = new MethodInfo()
                {
                    IsOverride = true,
                    Name = "Update",
                    Type = "IDataResult",
                    Params = "BaseParams baseParams"
                };

                mi.Body.Add("var values = new List<{0}>();".F(options.ClassName));

                mi.Body.Add("InTransaction(() =>");
                mi.Body.Add("{");
                mi.Body.Add("    var saveParams = GetSaveParam(baseParams);");
                mi.Body.Add("    foreach (var record in saveParams.Records)");
                mi.Body.Add("    {");
                mi.Body.Add("        var value = record.AsObject();");
                mi.Body.Add("        UpdateInternal(value);");
                mi.Body.Add("");
                mi.Body.Add("        values.Add(value);");
                mi.Body.Add("    }");
                mi.Body.Add("});");
                mi.Body.Add("");
                mi.Body.Add("return new BaseDataResult(values);");

                cls.AddMethod(mi);
            }

            if (options.DomainService.UpdateInternal)
            {
                var mi = new MethodInfo()
                {
                    IsOverride = true,
                    Access = "protected",
                    Name = "UpdateInternal",
                    Type = "void",
                    Params = "{0} value".F(options.ClassName)
                };

                mi.Body.Add("IDataResult result;");
                mi.Body.Add("var interceptors = Container.ResolveAll<IDomainServiceInterceptor<{0}>>();".F(options.ClassName));
                mi.Body.Add("foreach (var interceptor in interceptors)");
                mi.Body.Add("{");
                mi.Body.Add("    result = interceptor.BeforeUpdateAction(this, value);");
                mi.Body.Add("    if (!result.Success)");
                mi.Body.Add("    {");
                mi.Body.Add("        throw new ValidationException(result.Message);");
                mi.Body.Add("    }");
                mi.Body.Add("}");
                mi.Body.Add("");
                mi.Body.Add("Repository.Update(value);");
                mi.Body.Add("");
                mi.Body.Add("foreach (var interceptor in interceptors)");
                mi.Body.Add("{");
                mi.Body.Add("    result = interceptor.AfterUpdateAction(this, value);");
                mi.Body.Add("    if (!result.Success)");
                mi.Body.Add("    {");
                mi.Body.Add("        throw new ValidationException(result.Message);");
                mi.Body.Add("    }");
                mi.Body.Add("}");

                cls.AddMethod(mi);
            }

            if (options.DomainService.Delete)
            {
                var mi = new MethodInfo()
                {
                    IsOverride = true,
                    Name = "Delete",
                    Type = "IDataResult",
                    Params = "BaseParams baseParams"
                };

                mi.Body.Add("var idList = Converter.ToLongArray(baseParams.Params, \"records\");");

                mi.Body.Add("InTransaction(() =>");
                mi.Body.Add("{");
                mi.Body.Add("    foreach (var id in idList)");
                mi.Body.Add("    {");
                mi.Body.Add("    }");
                mi.Body.Add("});");
                mi.Body.Add("");
                mi.Body.Add("return new BaseDataResult(idList);");

                ns.InnerUsing.Add("B4.DomainService.BaseParams");
                cls.AddMethod(mi);
            }

            if (options.DomainService.DeleteInternal)
            {
                var mi = (new MethodInfo()
                {
                    Name = "DeleteInternal",
                    Type = "void",
                    Params = "object id"
                }).Protected.Override;

                mi.Body.Add("IDataResult result;");
                mi.Body.Add("var value = Container.Resolve<IDomainService<{0}>>().Get((long)id);".F(options.ClassName));
                mi.Body.Add("var interceptors = Container.ResolveAll<IDomainServiceInterceptor<{0}>>();".F(options.ClassName));
                mi.Body.Add("foreach (var interceptor in interceptors)");
                mi.Body.Add("{");
                mi.Body.Add("    result = interceptor.BeforeDeleteAction(this, value);");
                mi.Body.Add("    if (!result.Success)");
                mi.Body.Add("    {");
                mi.Body.Add("        throw new ValidationException(result.Message);");
                mi.Body.Add("    }");
                mi.Body.Add("}");
                mi.Body.Add("");
                mi.Body.Add("Repository.Delete(value);");
                mi.Body.Add("");
                mi.Body.Add("foreach (var interceptor in interceptors)");
                mi.Body.Add("{");
                mi.Body.Add("    result = interceptor.AfterDeleteAction(this, value);");
                mi.Body.Add("    if (!result.Success)");
                mi.Body.Add("    {");
                mi.Body.Add("        throw new ValidationException(result.Message);");
                mi.Body.Add("    }");
                mi.Body.Add("}");

                cls.AddMethod(mi);
            }

            fragments.AddLines("Module.cs", this, new List<string> { 
                "Container.RegisterDomain<{0}DomainService>();".F(options.ClassName)});

            file.Name = options.ClassName + "DomainService.cs";
            file.Path = "DomainServices";
            file.Body = ns.Generate();
            return file;
        }
    }
}

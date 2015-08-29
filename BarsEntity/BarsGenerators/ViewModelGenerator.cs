using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class ViewModelGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files.First();

            var ns = new NamespaceInfo() { Name = _project.DefaultNamespace + ".ViewModel" };
            var cls = new ClassInfo()
            {
                Name = options.ClassName + "ViewModel"
            };
            ns.NestedValues.Add(cls);

            if (options.Fields.Any(x => x.TypeName == "DateTime"))
                ns.OuterUsing.Add("System");
            ns.OuterUsing.Add("System.Linq");
            ns.OuterUsing.Add("System.Web.Mvc");


            ns.InnerUsing.Add("B4");
            ns.InnerUsing.Add("B4.Utils");

            if (options.Stateful)
                ns.InnerUsing.Add("B4.Modules.States");

            if (options.Signable)
                ns.InnerUsing.Add("B4.Modules.DigitalSignature.Entities");

            if (options.AcceptFiles)
                ns.InnerUsing.Add("B4.Modules.FileStorage");

            ns.InnerUsing.Add("Entities");

            cls.BaseClass = "BaseViewModel<" + options.ClassName + ">";

            _knownTypes.Clear();
            _knownTypes.Add(cls.Name);
            _knownTypes.Add(options.ClassName);
            _knownTypes.Add("BaseViewModel");
            _knownTypes.Add("BaseDataResult");
            _knownTypes.Add("ListDataResult");
            _knownTypes.Add("IDataResult");
            _knownTypes.Add("BaseParams");
            _knownTypes.Add("IDomainService");

            if (options.Controller.List && options.Controller.ViewModel)
            {
                var list = new MethodInfo()
                {
                    Name = "List",
                    Type = "IDataResult",
                    Params = "IDomainService<" +options.ClassName+ "> domainService, BaseParams baseParams",
                    IsOverride = true
                };
                var proxyClass = new ClassInfo { Name = "QueryResult" };
                _knownTypes.Add("QueryResult");

                proxyClass.AddProperty(new PropertyInfo() { Name = "Id", Type = "long" });

                foreach (var field in options.Fields.Where(x => !x.Collection))
                {
                    proxyClass.AddProperty(new PropertyInfo() { Name = field.FieldName, Type = field.FullTypeName });
                    if (!field.IsBasicType() && field.TypeName != field.FieldName)
                    {
                        _knownTypes.Add(field.TypeName);
                        ns.InnerUsing.AddDistinct(GetTypeNamespace(field.TypeName));
                    }
                }

                if (options.Signable)
                {
                    proxyClass.AddProperty(new PropertyInfo() { Name = "Signed", Type = "bool" });
                    _knownTypes.Add("DigSignature");
                }

                if (options.View.TreeGrid)
                {
                    proxyClass.AddProperty(new PropertyInfo() { Name = "_parent", Type = "long" });
                    proxyClass.AddProperty(new PropertyInfo() { Name = "_is_leaf", Type = "bool" });
                    proxyClass.AddProperty(new PropertyInfo() { Name = "_is_loaded", Type = "bool" });
                }

                proxyClass.Properties.ToList().ForEach(x => x.Public.Virtual.Auto.Get().Set());

                ns.NestedValues.Add(proxyClass);

                list.Body.Add("var loadParam = baseParams.GetLoadParam();");

                var owner = options.Fields.FirstOrDefault(x => x.OwnerReference);
                var parent = options.Fields.FirstOrDefault(x => x.ParentReference);

                if (owner != null)
                    list.Body.Add("var {0}Id = baseParams.Params[\"{0}Id\"].ToLong();".R(owner.FieldName.camelCase()));

                if (options.Signable)
                    list.Body.Add("var qDigitalSignatures = Container.Resolve<IDomainService<DigSignature>>().GetAll();");

                list.Body.Add("var qList = domainService.GetAll()");

                if (owner != null)
                    list.Body.Add("    .Where(x => x.{0}.Id == {1}Id)".R(owner.FieldName, owner.FieldName.camelCase()));


                list.Body.Add("    .Select(x => new QueryResult {");
                foreach (var prop in proxyClass.Properties)
                {
                    if (options.Signable && prop.Name == "Signed")
                        list.Body.Add("        Signed = qDigitalSignatures.Any(ds => ds.EntityTypeId == \"{0}\" && ds.EntityId == x.Id),".R(options.ClassFullName));
                    else if (prop.Name == "_is_loaded")
                        list.Body.Add("        _is_loaded = true,");
                    else if (prop.Name == "_parent")
                        list.Body.Add("        _parent = x.{0} != null ? x.{0}.Id : 0,".R(parent.FieldName));
                    else if (prop.Name == "_is_leaf")
                        list.Body.Add("        _is_leaf = !domainService.GetAll().Any(y => y.{0} == x),".R(parent.FieldName));
                    else
                        list.Body.Add("        {0} = x.{0},".R(prop.Name));
                }
                var last = list.Body.Last();
                last = last.Substring(0, last.Length - 1);
                list.Body.RemoveAt(list.Body.Count - 1);
                list.Body.Add(last);

                list.Body.Add("    })");
                list.Body.Add("    .Filter(loadParam, Container);");

                list.Body.Add("return new ListDataResult(qList.Order(loadParam).Paging(loadParam), qList.Count());");

                cls.AddMethod(list);
            }

            if (options.Controller.Get && options.Controller.ViewModel)
            {
                var get = new MethodInfo()
                {
                    Name = "Get",
                    Type = "IDataResult",
                    Params = "IDomainService<" + options.ClassName + "> domainService, BaseParams baseParams",
                    IsOverride = true
                };
                var body = get.Body;

                body.Add("var id = baseParams.Params[\"id\"].ToLong();");
                body.Add("var entity = domainService.Get(id);");

                if (options.Signable)
                {
                    body.Add("var qDigitalSignatures = Container.ResolveDomain<Bars.B4.Modules.DigitalSignature.Entities.DigSignature>().GetAll();");
                }

                body.Add("var data = new {");
                body.Add("    entity.Id,");

                if (options.BaseClass == "NamedBaseEntity" && !options.Fields.Any(x => x.FieldName == "Name"))
                    body.Add("    entity.Name" + (options.Fields.Any(x => x.FieldName != "Name") ? "," : ""));

                foreach (var field in options.Fields.Where(x => !x.Collection && !x.TypeName.EndsWith("View")))
                {
                    body.Add("    entity.{0},".R(field.FieldName));
                }

                if (options.Signable)
                {
                    body.Add("    Signed = qDigitalSignatures.Any(y => y.EntityTypeId == \"{0}\" && y.EntityId == entity.Id),".R(options.ClassFullName));
                }

                body[body.Count - 1] = body.Last().Substring(0, body.Last().Length - 1);

                body.Add("};");

                body.Add("return new BaseDataResult(data);");

                cls.AddMethod(get);
            }

            fragments.AddLines("Module.cs", this, new List<string> { 
                "Container.Register(Component.For<IViewModel<{0}>>().ImplementedBy<{0}ViewModel>());".R(options.ClassName)});

            file.Name = options.ClassName + "ViewModel.cs";
            file.Path = "ViewModel\\" + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
            file.Body = ns.Generate();
            return files;
        }
    }
}

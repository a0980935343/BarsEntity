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
    
    public class ControllerGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);

            var ns = new NamespaceInfo() { Name = "{0}.Controllers".F(project.Name) };
            var cls = new ClassInfo()
            {
                Name = "{0}Controller".F(options.Controller.Name)
            };
            ns.NestedValues.Add(cls);

            if (options.Fields.Any(x => x.TypeName == "DateTime"))
                ns.OuterUsing.Add("System");
            ns.OuterUsing.Add("System.Linq");
            ns.OuterUsing.Add("System.Web.Mvc");
            
            
            ns.InnerUsing.Add("Bars.B4");
            ns.InnerUsing.Add("Bars.B4.Utils");

            if (options.Stateful)
                ns.InnerUsing.Add("Bars.B4.Modules.States");

            if (options.Signable)
                ns.InnerUsing.Add("Bars.B4.Modules.DigitalSignature.Entities");

            if (options.AcceptFiles)
                ns.InnerUsing.Add("Bars.B4.Modules.FileStorage");

            ns.InnerUsing.Add("Entities");

            if (options.Permission != null && options.Permission.Prefix != "")
            {
                cls.Attributes.Add("ActionPermission(\"Update\", \"{0}.Edit\")".F(options.Permission.Prefix));
                cls.Attributes.Add("ActionPermission(\"Save\",   \"{0}.Edit\")".F(options.Permission.Prefix));
                cls.Attributes.Add("ActionPermission(\"Create\", \"{0}.Create\")".F(options.Permission.Prefix));
                cls.Attributes.Add("ActionPermission(\"Delete\", \"{0}.Delete\")".F(options.Permission.Prefix));
            }

            var basePrefix = "B4.Alt.";
            if (options.View.Inline)
                basePrefix = "B4.Alt.Inline";
            else if (options.AcceptFiles)
                basePrefix = "FileStorage";

            cls.BaseClass = "{0}DataController<{1}>".F(basePrefix, options.ClassName);

            _knownTypes.Clear();
            _knownTypes.Add(cls.Name);
            _knownTypes.Add(options.ClassName);
            _knownTypes.Add("DataController");
            _knownTypes.Add("InlineDataController");
            _knownTypes.Add("FileStorageDataController");
            _knownTypes.Add("ActionResult");
            _knownTypes.Add("BaseParams");
            _knownTypes.Add("IDomainService");

            if (options.Fields.Any(x => x.OwnerReference) || options.View.TreeGrid || options.Signable)
            {
                var list = new MethodInfo() 
                { 
                    Name = "List",
                    Type = "ActionResult",
                    Params = "BaseParams baseParams",
                    IsOverride = true
                };

                var proxyClass = new ClassInfo { Name = "QueryResult" };
                _knownTypes.Add("QueryResult");
                
                if (options.View.TreeGrid || options.Signable)
                {
                    proxyClass.AddProperty(new PropertyInfo() { Name = "Id", Type = "long" });

                    foreach (var field in options.Fields.Where(x => !x.Collection))
                    {
                        proxyClass.AddProperty(new PropertyInfo() { Name = field.FieldName, Type = field.FullTypeName });
                        if (!field.IsBasicType() && field.TypeName != field.FieldName)
                            _knownTypes.Add(field.TypeName);
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
                }
                
                list.Body.Add("var loadParam = baseParams.GetLoadParam();");

                var owner = options.Fields.FirstOrDefault(x => x.OwnerReference);
                var parent = options.Fields.FirstOrDefault(x => x.ParentReference);

                if (owner != null)
                    list.Body.Add("var {0}Id = baseParams.Params[\"{0}Id\"].ToLong();".F(owner.FieldName.camelCase()));

                if (options.Signable)
                    list.Body.Add("var qDigitalSignatures = Container.Resolve<IDomainService<DigSignature>>().GetAll();");

                list.Body.Add("var qList = DomainService.GetAll()");

                if (owner != null)
                    list.Body.Add("    .Where(x => x.{0}.Id == {1}Id)".F(owner.FieldName, owner.FieldName.camelCase()));

                if (options.View.TreeGrid || options.Signable)
                {
                    list.Body.Add("    .Select(x => new QueryResult {");
                    foreach (var prop in proxyClass.Properties)
                    {
                        if (options.Signable && prop.Name == "Signed")
                            list.Body.Add("        Signed = qDigitalSignatures.Any(ds => ds.EntityTypeId == \"{0}\" && ds.EntityId == x.Id),".F(options.ClassFullName));
                        else if (prop.Name == "_is_loaded")
                            list.Body.Add("        _is_loaded = true,");
                        else if (prop.Name == "_parent")
                            list.Body.Add("        _parent = x.{0} != null ? x.{0}.Id : 0,".F(parent.FieldName));
                        else if (prop.Name == "_is_leaf")
                            list.Body.Add("        _is_leaf = !DomainService.GetAll().Any(y => y.{0} == x),".F(parent.FieldName));
                        else
                            list.Body.Add("        {0} = x.{0},".F(prop.Name));
                    }
                    var last = list.Body.Last();
                    last = last.Substring(0, last.Length - 1);
                    list.Body.RemoveAt(list.Body.Count-1);
                    list.Body.Add(last);

                    list.Body.Add("    })");
                }
                list.Body.Add("    .Filter(loadParam, Container);");

                list.Body.Add("return new JsonListResult(qList.Order(loadParam).Paging(loadParam).ToList(), qList.Count());");

                cls.AddMethod(list);
            }

            fragments.AddLines("Module.cs", this, new List<string> { 
                "Container.RegisterController<{0}Controller>();".F(options.Controller.Name)});

            file.Name = options.Controller.Name + "Controller.cs";
            file.Path = "Controllers\\" + (options.IsDictionary ? "Dict\\" : "");
            file.Body = ns.Generate();
            return file;
        }
    }
}

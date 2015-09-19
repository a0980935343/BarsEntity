using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;
    
    public class ControllerGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files.First();

            var ns = new NamespaceInfo() { Name = project.DefaultNamespace + ".Controllers" };
            var cls = new ClassInfo()
            {
                Name = options.Controller.Name + "Controller"
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
                ns.InnerUsing.Add("DS = B4.Modules.DigitalSignature.Entities");

            if (options.AcceptFiles)
                ns.InnerUsing.Add("B4.Modules.FileStorage");

            ns.InnerUsing.Add("Entities");

            if (options.Permission != null && options.Permission.Prefix != "")
            {
                cls.Attributes.Add("ActionPermission(\"Update\", \"{0}.Edit\")".R(options.Permission.Prefix));
                cls.Attributes.Add("ActionPermission(\"Save\",   \"{0}.Edit\")".R(options.Permission.Prefix));
                cls.Attributes.Add("ActionPermission(\"Create\", \"{0}.Create\")".R(options.Permission.Prefix));
                cls.Attributes.Add("ActionPermission(\"Delete\", \"{0}.Delete\")".R(options.Permission.Prefix));
            }

            var basePrefix = "B4.Alt.";
            if (options.View.Type == ViewType.ViewModel)
                basePrefix = "B4.Alt.Inline";
            else if (options.AcceptFiles)
                basePrefix = "FileStorage";

            cls.BaseClass = "{0}DataController<{1}>".R(basePrefix, options.ClassName);

            _knownTypes.Clear();
            _knownTypes.Add(cls.Name);
            _knownTypes.Add(options.ClassName);
            _knownTypes.Add("DataController");
            _knownTypes.Add("InlineDataController");
            _knownTypes.Add("FileStorageDataController");
            _knownTypes.Add("ActionResult");
            _knownTypes.Add("BaseParams");
            _knownTypes.Add("IDomainService");

            #region ViewModel & ListSummary actions
            if (options.Controller.List && !options.Controller.ViewModel)
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

                if (options.Signable && !cls.Properties.Any(x => x.Name == "DsDigSignature" && x.Type == "IDomainService<DS.DigSignature>"))
                    cls.AddProperty(new PropertyInfo { Name = "DsDigSignature", Type = "IDomainService<DS.DigSignature>", IsVirtual = false }.Auto.Get().Set());

                list.Body.Add("");
                list.Body.Add("var qList = DomainService.GetAll()");

                if (owner != null)
                    list.Body.Add("    .Where(x => x.{0}.Id == {1}Id)".R(owner.FieldName, owner.FieldName.camelCase()));

                   
                list.Body.Add("    .Select(x => new QueryResult {");
                foreach (var prop in proxyClass.Properties)
                {
                    if (options.Signable && prop.Name == "Signed")
                        list.Body.Add("        Signed = DsDigSignature.GetAll().Any(ds => ds.EntityTypeId == \"{0}\" && ds.EntityId == x.Id),".R(options.ClassFullName));
                    else if (prop.Name == "_is_loaded")
                        list.Body.Add("        _is_loaded = true,");
                    else if (prop.Name == "_parent")
                        list.Body.Add("        _parent = x.{0} != null ? x.{0}.Id : 0,".R(parent.FieldName));
                    else if (prop.Name == "_is_leaf")
                        list.Body.Add("        _is_leaf = !DomainService.GetAll().Any(y => y.{0} == x),".R(parent.FieldName));
                    else
                        list.Body.Add("        {0} = x.{0},".R(prop.Name));
                }
                var last = list.Body.Last();
                last = last.Substring(0, last.Length - 1);
                list.Body.RemoveAt(list.Body.Count - 1);
                list.Body.Add(last);

                list.Body.Add("    })");
                list.Body.Add("    .Filter(loadParam, Container);");
                list.Body.Add("");
                list.Body.Add("return new JsonListResult(qList.Order(loadParam).Paging(loadParam).ToList(), qList.Count());");

                cls.AddMethod(list);
            }

            if (options.Controller.ListSummary)
            {
                var listSummary = new MethodInfo()
                {
                    Name = "ListSummary",
                    Type = "ActionResult",
                    Params = "BaseParams baseParams",
                    IsOverride = false
                };

                listSummary.Body.Add("var loadParam = baseParams.GetLoadParam();");

                var owner = options.Fields.FirstOrDefault(x => x.OwnerReference);
                var parent = options.Fields.FirstOrDefault(x => x.ParentReference);

                if (owner != null)
                    listSummary.Body.Add("var {0}Id = baseParams.Params[\"{0}Id\"].ToLong();".R(owner.FieldName.camelCase()));

                listSummary.Body.Add("");
                listSummary.Body.Add("var qList = DomainService.GetAll()");

                if (owner != null)
                    listSummary.Body.Add("    .Where(x => x.{0}.Id == {1}Id)".R(owner.FieldName, owner.FieldName.camelCase()));

                listSummary.Body.Add("    .Filter(loadParam, Container);");
                listSummary.Body.Add("");
                listSummary.Body.Add("return new JsonNetResult(new {");

                var sumFields = new List<string>();
                foreach (var field in options.Fields.Where(x => !x.Collection))
                {
                    if ((options.Signable && field.FieldName == "Signed") ||
                        field.FieldName == "_is_loaded" || field.FieldName == "_parent" || field.FieldName == "_is_leaf" ||
                        (field.TypeName != "decimal" && field.TypeName != "long" && field.TypeName != "int"))
                        continue;
                    else
                        sumFields.Add(field.FieldName);
                        
                }

                if (sumFields.Any())
                {
                    foreach (var field in sumFields)
                    {
                        listSummary.Body.Add("                     {0} = qList.Sum(x => x.{0}),".R(field));
                    }
                    var last = listSummary.Body.Last();
                    last = last.Substring(0, last.Length - 1);
                    listSummary.Body.RemoveAt(listSummary.Body.Count - 1);
                    listSummary.Body.Add(last);
                }
                else
                {
                    listSummary.Body.Add("                // нет полей типа decimal, long или int!");
                }

                listSummary.Body.Add("                 });");


                cls.AddMethod(listSummary);
            }

            if (options.Controller.Get && !options.Controller.ViewModel)
            {
                var get = new MethodInfo()
                {
                    Name = "Get",
                    Type = "ActionResult",
                    Params = "BaseParams baseParams",
                    IsOverride = true
                };
                var body = get.Body;

                body.Add("var id = baseParams.Params[\"id\"].ToLong();");
                body.Add("var entity = DomainService.Get(id);");

                if (options.Signable && !cls.Properties.Any(x => x.Name == "DsDigSignature" && x.Type == "IDomainService<DS.DigSignature>"))
                {
                    cls.AddProperty(new PropertyInfo { Name = "DsDigSignature", Type = "IDomainService<DS.DigSignature>" }.Public.Auto.Get().Set());
                }

                body.Add("var data = new {");
                body.Add("    entity.Id,");

                if (options.BaseClass == "NamedBaseEntity" && !options.Fields.Any(x => x.FieldName == "Name") && options.Profile is MosKsProfile)
                    body.Add("    entity.Name" + (options.Fields.Any(x => x.FieldName != "Name") ? "," : ""));

                foreach (var field in options.Fields.Where(x => !x.Collection && !x.TypeName.EndsWith("View")))
                {
                    body.Add("    entity.{0},".R(field.FieldName));
                }

                if (options.Signable)
                {
                    body.Add("    Signed = DsDigSignature.GetAll().Any(y => y.EntityTypeId == \"{0}\" && y.EntityId == entity.Id),".R(options.ClassFullName));
                }

                body[body.Count - 1] = body.Last().Substring(0, body.Last().Length - 1);
                
                body.Add("};");

                body.Add("return new JsonGetResult(data);");

                cls.AddMethod(get);
            }
            #endregion

            #region DomainService actions
            if (options.Controller.Update)
            {
                var update = new MethodInfo()
                {
                    Name = "Update",
                    Type = "ActionResult",
                    Params = "BaseParams baseParams",
                    IsOverride = true
                };
                var body = update.Body;

                body.Add("try");
                body.Add("{");
                body.Add("    var values = new List<{0}>();".R(options.ClassName));
                body.Add("    var saveParam = baseParams.Params.Read<SaveParam<{0}>>().Execute(container => Converter.ToSaveParam<{0}>(container, false));".R(options.ClassName));
                body.Add("    foreach (var record in saveParam.Records)");
                body.Add("    {");
                body.Add("        var value = record.AsObject();");
                body.Add("        DomainService.Update(value);");
                body.Add("        values.Add(value);");
                body.Add("    }");
                body.Add("    return new JsonNetResult(new { success = true, message = string.Empty, data = values });");
                body.Add("}");
                body.Add("catch (ValidationException exc)");
                body.Add("{");
                body.Add("    return JsonNetResult.Failure(exc.Message);");
                body.Add("}");

                cls.AddMethod(update);
            }

            if (options.Controller.Delete)
            {
                var delete = new MethodInfo()
                {
                    Name = "Delete",
                    Type = "ActionResult",
                    Params = "BaseParams baseParams",
                    IsOverride = true
                };
                var body = delete.Body;

                body.Add("var ids = Converter.ToLongArray(baseParams.Params, \"records\");");
                body.Add("using (var tr = Container.Resolve<IDataTransaction>())");
                body.Add("{");
                body.Add("    try");
                body.Add("    {");
                body.Add("        foreach (var id in ids)");
                body.Add("        {");
                body.Add("            DomainService.Delete((long)id);");
                body.Add("        }");
                body.Add("");
                body.Add("        tr.Commit();");
                body.Add("    }");
                body.Add("    catch (Exception)");
                body.Add("    {");
                body.Add("        tr.Rollback();");
                body.Add("        throw;");
                body.Add("    }");
                body.Add("}");
                body.Add("");
                body.Add("return JsonNetResult.Success;");

                cls.AddMethod(delete);
            }

            if (options.Controller.Create)
            {
                var create = new MethodInfo()
                {
                    Name = "Create",
                    Type = "ActionResult",
                    Params = "BaseParams baseParams",
                    IsOverride = true
                };
                var body = create.Body;

                body.Add("try");
                body.Add("{");
                body.Add("    var entity = baseParams.Params.Read<SaveParam<{0}>>().Execute(container => Converter.ToSaveParam<{0}>(container, false)).First().AsObject();".R(options.ClassName));
                body.Add("    DomainService.Save(entity);");
                body.Add("");
                body.Add("    return new JsonNetResult(new { success = true, message = string.Empty, data = new object[] { entity } });");
                body.Add("}");
                body.Add("catch (ValidationException exc)");
                body.Add("{");
                body.Add("    return JsonNetResult.Failure(exc.Message);");
                body.Add("}");

                cls.AddMethod(create);
            }
            #endregion

            var module = new GeneratedFragment
            {
                FileName = "Module.cs",
                InsertToFile = true,
                InsertClass = "public class Module",
                InsertMethod = "public override void Install()",
                Generator = this
            };
            module.Lines.Add("Container.RegisterController<{0}Controller>();".R(options.Controller.Name));

            fragments.Add("Module.cs", module);

            file.Name = options.Controller.Name + "Controller.cs";
            file.Path = "Controllers\\" + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
            file.Body = ns.Generate();
            return files;
        }
    }
}

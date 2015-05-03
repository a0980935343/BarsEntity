﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;
    
    public class ControllerGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);

            var ns = new NamespaceInfo() { Name = "{0}.Controllers".F(_project.DefaultNamespace) };
            var cls = new ClassInfo()
            {
                Name = "{0}Controller".F(options.Controller.Name)
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

            if (options.Permission != null && options.Permission.Prefix != "")
            {
                cls.Attributes.Add("ActionPermission(\"Update\", \"{0}.Edit\")".F(options.Permission.Prefix));
                cls.Attributes.Add("ActionPermission(\"Save\",   \"{0}.Edit\")".F(options.Permission.Prefix));
                cls.Attributes.Add("ActionPermission(\"Create\", \"{0}.Create\")".F(options.Permission.Prefix));
                cls.Attributes.Add("ActionPermission(\"Delete\", \"{0}.Delete\")".F(options.Permission.Prefix));
            }

            var basePrefix = "B4.Alt.";
            if (options.View.Type == ViewType.ViewModel)
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

            if (options.Controller.List)
            {
                var list = new MethodInfo()
                {
                    Name = "List",
                    Type = "ActionResult",
                    Params = "BaseParams baseParams",
                    IsOverride = true
                };

                //if (options.Fields.Any(x => x.OwnerReference) || options.View.TreeGrid || options.Signable)
                {
                    var proxyClass = new ClassInfo { Name = "QueryResult" };
                    _knownTypes.Add("QueryResult");

                    //if (options.View.TreeGrid || options.Signable)
                    {
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

                    //if (options.View.TreeGrid || options.Signable)
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
                        list.Body.RemoveAt(list.Body.Count - 1);
                        list.Body.Add(last);

                        list.Body.Add("    })");
                    }
                    list.Body.Add("    .Filter(loadParam, Container);");

                    list.Body.Add("return new JsonListResult(qList.Order(loadParam).Paging(loadParam).ToList(), qList.Count());");

                }
                //else
               // {
                //    list.Body.Add("return base.List(baseParams);");
                //}
                cls.AddMethod(list);
            }

            if (options.Controller.Get)
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
                    body.Add("    entity.{0},".F(field.FieldName));
                }

                if (options.Signable)
                {
                    body.Add("    Signed = qDigitalSignatures.Any(y => y.EntityTypeId == \"{0}\" && y.EntityId == entity.Id),".F(options.ClassFullName));
                }

                body[body.Count - 1] = body.Last().Substring(0, body.Last().Length - 1);
                
                body.Add("};");

                body.Add("return new JsonGetResult(data);");

                cls.AddMethod(get);
            }

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
                body.Add("    var values = new List<{0}>();".F(options.ClassName));
                body.Add("    var saveParam = baseParams.Params.Read<SaveParam<{0}>>().Execute(container => Converter.ToSaveParam<{0}>(container, false));".F(options.ClassName));
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
                body.Add("    var entity = baseParams.Params.Read<SaveParam<{0}>>().Execute(container => Converter.ToSaveParam<{0}>(container, false)).First().AsObject();".F(options.ClassName));
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

            fragments.AddLines("Module.cs", this, new List<string> { 
                "Container.RegisterController<{0}Controller>();".F(options.Controller.Name)});

            file.Name = options.Controller.Name + "Controller.cs";
            file.Path = "Controllers\\" + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
            file.Body = ns.Generate();
            return file;
        }
    }
}

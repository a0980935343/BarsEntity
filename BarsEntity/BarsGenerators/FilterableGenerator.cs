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

    public class FilterableGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);
            
            var ns = new NamespaceInfo() { Name = project.Name + ".DynamicFilterableEntities" };
            var cls = new ClassInfo {
                Name = "Filterable{0}".F(options.ClassName),
                BaseClass = "BaseFilterableEntity"
            };
            ns.NestedValues.Add(cls);
            ns.OuterUsing.Add("System");
            ns.OuterUsing.Add("System.Collections.Generic");
            ns.OuterUsing.Add("System.Linq");
            ns.OuterUsing.Add("System.Linq.Expressions");

            ns.InnerUsing.Add("B4.Modules.ReportPanel");
            ns.InnerUsing.Add("MosKs.DynamicFilters.Entities");
            ns.InnerUsing.Add("Entities");

            _knownTypes.Clear();
            _knownTypes.Add(options.ClassName);
            _knownTypes.Add(cls.Name);
            _knownTypes.Add(cls.BaseClass);
            _knownTypes.Add("EntityAttribute");
            _knownTypes.Add("SimpleSelectFieldConfig");
            _knownTypes.Add("Expression");
            _knownTypes.Add("Func");

            var entityName = new PropertyInfo
            {
                Name = "EntityName",
                Type = "string",
                IsOverride = true,
                IsVirtual = false,
                AutoProperty = false,
                Getter = new List<string> {  "return typeof({0}).Name;".F(options.ClassName) }
            };

            var displayEntityName = new PropertyInfo
            {
                Name = "DisplayEntityName",
                Type = "string",
                IsOverride = true,
                IsVirtual = false,
                AutoProperty = false,
                Getter = new List<string> { "return \"{0}\";".F(options.DisplayName) }
            };

            var entityAttributes = new MethodInfo
            {
                Type = "IList<EntityAttribute>",
                Name = "GetFilterAttributes",
                IsOverride = true
            };

            entityAttributes.Body.Add("var result = new[]");
            entityAttributes.Body.Add("{");

            var fields = options.Fields.Where(x => x.DynamicFilter && !x.Collection).ToList();

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];

                entityAttributes.Body.Add("    new EntityAttribute");
                entityAttributes.Body.Add("    {");
                entityAttributes.Body.Add("        PropertyName = \"{0}\",".F(field.FieldName));
                entityAttributes.Body.Add("        Name = \"{0}\",".F(field.DisplayName));
                entityAttributes.Body.Add("        TypeId = {0}EntityAttributeType.UniqueId,".F(field.DynamicFilterType));

                _knownTypes.Add(field.DynamicFilterType + "EntityAttributeType");

                if (field.IsReference())
                { 
                    entityAttributes.Body.Add("        UserParameter = new PersistentObjectUserParameter");
                    entityAttributes.Body.Add("        {");
                    entityAttributes.Body.Add("            SelectFieldConfig = new SimpleSelectFieldConfig(\"{0}\")".F(field.TypeName));
                    entityAttributes.Body.Add("            {");
                    entityAttributes.Body.Add("                selectWindowConfig = new");
                    entityAttributes.Body.Add("                {");
                    entityAttributes.Body.Add("                    selectWindowXType = \"mosksmultiselectwindow\",");
                    entityAttributes.Body.Add("                    gridConfig  = new { columns = new[] { new { header = \""+field.DisplayName+"\", dataIndex = \"" + field.TextProperty + "\" } } },");
                    entityAttributes.Body.Add("                    storeConfig = new { fields  = new[] { \"Id\", \""+ field.TextProperty +"\" }, controllerName = \"" + field.TypeName + "\", controllerAction = \"List\" }");
                    entityAttributes.Body.Add("                },");
                    entityAttributes.Body.Add("                ActionGetDisplayNamesByIds = \"GetDisplayNamesByIds\"");
                    entityAttributes.Body.Add("            },");
                    entityAttributes.Body.Add("            Require = true");
                    entityAttributes.Body.Add("        },");
                }


                entityAttributes.Body.Add("        InitExpression = ((Expression<Func<{0}, {1}>>) (x => x.{2}{3}))".F(options.ClassName, field.IsReference() ? "long" : field.FullTypeName, field.FieldName, field.IsReference() ? ".Id" : ""));
                entityAttributes.Body.Add("    }}{0}".F(i < fields.Count-1 ? "," : "" ));
            }
            entityAttributes.Body.Add("};");
            entityAttributes.Body.Add("return result;");

            cls.AddProperty(entityName);
            cls.AddProperty(displayEntityName);
            cls.AddMethod(entityAttributes);

            fragments.AddLines("Module.cs", this, new List<string>{ 
                "Container.Register(Component.For<IDynamicFilterable>().ImplementedBy<Filterable{0}>().Named(\"{0}\"));".F(options.ClassName),
                "Container.Register(Component.For<IDomainServiceReadInterceptor<{0}>>().ImplementedBy<DynamicFilterDomainServiceReadInterceptor<{0}>>().LifeStyle.Transient);".F(options.ClassName),
                "Container.Register(Component.For<IDomainServiceReadInterceptor<{0}>>().ImplementedBy<RuleLimitingAccessDomainServiceReadInterceptor<{0}>>().LifeStyle.Transient);".F(options.ClassName)
            });
            
            file.Name = "Filterable" + options.ClassName + "Map.cs";
            file.Path = "Domain\\DynamicFilterableEntities";
            file.Body = ns.Generate();
            return file;
        }
    }
}

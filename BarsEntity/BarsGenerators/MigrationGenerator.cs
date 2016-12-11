using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;
    using Barsix.BarsEntity.Types;
    using EnvDTE;


    public class MigrationGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files.First();

            var versionSpace = "Version_{0}".R(options.MigrationVersion);
            var prevVersionSpace = "Version_{0}".R(options.MigrationVersion);
            var prevVersion = "";
            var version = options.MigrationVersion.Replace("_", "");
            
            // Search previous migration version
            if (Classes != null)
            {
                var migrations = new Dictionary<string, string>();
                foreach (var mig in Classes)
                {
                    var versionAttr = mig.Value.Attributes.OfType<CodeAttribute>().Where(a => a.FullName.EndsWith(".MigrationAttribute")).FirstOrDefault();
                    if (versionAttr == null)
                        continue;

                    if (versionAttr.Children != null && versionAttr.Children.Count > 0)
                    {
                        var start = versionAttr.Children.Cast<CodeElement>().First().GetStartPoint();
                        var finish = versionAttr.Children.Cast<CodeElement>().First().GetEndPoint();

                        migrations.Add(start.CreateEditPoint().GetText(finish).Unwrap("\""), mig.Value.Namespace.Name.Split('.').Last());
                    }
                }
                if (migrations.Any())
                {
                    prevVersion = migrations.Keys.Max();
                    prevVersionSpace = migrations[prevVersion];
                }
            }

            // increment last version + 1
            if (options.FromLastMigration && prevVersion != "")
            {
                version = prevVersion.GetFirst(prevVersion.Length - 2) + (int.Parse(prevVersion.GetLast(2)) + 1).ToString().PadLeft(2, '0');
                versionSpace = prevVersionSpace.GetFirst(prevVersionSpace.Length - 2) + (int.Parse(prevVersionSpace.GetLast(2)) + 1).ToString().PadLeft(2, '0');
            }

            var ns = new NamespaceInfo();
            var cls = new ClassInfo();
            ns.NestedValues.Add(cls);
            ns.Name = project.DefaultNamespace + ".Migrations." + versionSpace;

            ns.InnerUsing.Add("System.Data");
            ns.InnerUsing.Add("B4.Modules.Ecm7.Framework");
            ns.InnerUsing.Add("B4.Modules.NH.Migrations.DatabaseExtensions");
            ns.InnerUsing.Add("MosKs.Core.Migrations");

            long outV = 0;
            if (!long.TryParse(version, out outV))
                throw new Exception("Версия должна состоять только из цифр и символа '_'");

            cls.Attributes.Add("Migration(\"{0}\")".R(version));
            cls.Attributes.Add("MigrationDependsOn(typeof(Migrations.{0}.UpdateSchema))".R(prevVersionSpace));

            cls.Name = "UpdateSchema";
            cls.BaseClass = "Migration";

            _knownTypes.Clear();
            _knownTypes.Add(cls.Name);
            _knownTypes.Add(cls.BaseClass);
            _knownTypes.Add("Column");
            _knownTypes.Add("RefColumn");
            _knownTypes.Add("ColumnProperty");
            _knownTypes.Add("DbType");
            _knownTypes.Add("TableColumns");
            
            var up = new MethodInfo() 
            { 
                Name = "Up",
                IsOverride = true
            };
            var down = new MethodInfo()
            {
                IsOverride = true,
                Name = "Down"
            };

            if (!string.IsNullOrEmpty(options.TableName))
            {
                if (!options.ClassName.EndsWith("View"))
                {
                    up.Body.Add("Database.Add{1}Table(\"{0}\", \"{2}\", new TableColumns {".R(
                        options.TableName, 
                        options.BaseClass.EndsWith("BaseEntity") 
                            ? "Entity" 
                            : options.BaseClass.EndsWith("NamedBaseEntity")
                                ? "NamedEntity" : "PersistentObject",
                        options.DisplayName));

                    for (int i = 0; i < options.Fields.Count; i++)
                    {
                        var field = options.Fields[i];
                        string ind = "    ";
                        if (field.IsReference())
                        {
                            up.Body.Add(ind + "{{ new RefColumn(\"{0}\", \"{1}\", \"{2}\", \"ID\"), {3}}},".R(
                                field.ColumnName, 
                                field.Index, 
                                field.ReferenceTable,
                                field.Comment.Q("\"")) + (i < options.Fields.Count - 1 ? "," : "")
                            );
                        }
                        else if (field.IsBasicType())
                        {
                            string col = "{{ new Column(\"{0}\", ".R(field.ColumnName);
                            string dbType = "DbType." + (field.Enum ? "Int32" : TypeHelper.BasicStrongName(field.TypeName));
                            if (field.TypeName == "string" && field.Length > 0)
                                dbType += ", " + field.Length;

                            col += dbType + ", ColumnProperty.{0}Null".R(field.Nullable ? "" : "Not");
                            if (field.DefaultValue != "")
                                col += field.DefaultValue;

                            col += "), \"" + field.Comment + "\"}";
                            if (i < options.Fields.Count - 1)
                                col += ",";

                            up.Body.Add(ind + col);
                        }
                    }
                    up.Body.Add("});");

                    down.Body.Add("Database.RemoveTable(\"{0}\");".R(options.TableName));
                }
                else // view
                {
                    up.Body.Add("Database.ExecuteNonQuery(@\"create or replace view {0}".R(options.TableName.ToLower()));
                    up.Body.Add("    (");
                    up.Body.Add("        id,");
                    for (int i = 0; i < options.Fields.Count; i++)
                    {
                        var field = options.Fields[i];
                        up.Body.Add("        {0}{1}".R(field.ColumnName.ToLower(), i != options.Fields.Count - 1 ? "," : ""));
                    }
                    up.Body.Add("    )");
                    up.Body.Add("    as");
                    up.Body.Add("    select");
                    up.Body.Add("        id,");
                    for (int i = 0; i < options.Fields.Count; i++)
                    {
                        var field = options.Fields[i];
                        up.Body.Add("        {0}{1}".R(field.ColumnName.ToLower(), i != options.Fields.Count - 1 ? "," : ""));
                    }
                    up.Body.Add("    from mosks_" + options.ClassName.Substring(0, options.ClassName.Length - 4).CamelToSnake().ToLower());
                    up.Body.Add("\");");

                    down.Body.Add("Database.ExecuteNonQuery(\"drop view {0}\");".R(options.TableName.ToLower()));
                }
            }

            cls.AddMethod(up);
            cls.AddMethod(down);

            file.Name = "UpdateSchema.cs";
            file.Path = "Migrations\\" + versionSpace;
            file.Body = ns.Generate();
            return files;
        }

        public override ClassRequest ClassRequest
        {
            get
            {
                return new ClassRequest
                {
                    BaseElements = new List<string> { "Migration" },
                    NamespaceSuffix = ".Migrations"
                };
            }
        }
    }
}

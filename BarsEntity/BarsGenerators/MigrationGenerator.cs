using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class MigrationGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files.First();

            string folderVersion = "Version_{0}".F(options.MigrationVersion);
            
            var ns = new NamespaceInfo();
            var cls = new ClassInfo();
            ns.NestedValues.Add(cls);
            ns.Name = "{0}.Migrations.{1}".F(_project.DefaultNamespace, folderVersion);

            ns.InnerUsing.Add("System.Data");
            ns.InnerUsing.Add("ECM7.Migrator.Framework");
            ns.InnerUsing.Add("B4.Modules.ECM7.DatabaseExtensions");

            long outV = 0;
            if (!long.TryParse(options.MigrationVersion.Replace("_", ""), out outV))
                throw new Exception("Версия должна состоять только из цифр и символа '_'");

            cls.Attributes.Add("Migration({0})".F(options.MigrationVersion.Replace("_", "")));

            cls.Name = "UpdateSchema";
            cls.BaseClass = "Migration";

            _knownTypes.Clear();
            _knownTypes.Add(cls.Name);
            _knownTypes.Add(cls.BaseClass);
            _knownTypes.Add("Column");
            _knownTypes.Add("RefColumn");
            _knownTypes.Add("ColumnProperty");
            _knownTypes.Add("DbType");
            
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
                if (!options.TableName.ToLower().EndsWith("_view"))
                {
                    up.Body.Add("Database.AddEntityTable(\"{0}\",".F(options.TableName));

                    for (int i = 0; i < options.Fields.Count; i++)
                    {
                        var field = options.Fields[i];
                        string ind = "    ";
                        if (field.IsReference())
                        {
                            up.Body.Add(ind + "new RefColumn(\"{0}\", \"{1}\", \"{2}\", \"ID\")".F(field.ColumnName, field.Index, field.ReferenceTable) + (i < options.Fields.Count - 1 ? "," : ""));
                        }
                        else if (field.IsBasicType())
                        {
                            string col = "new Column(\"{0}\", ".F(field.ColumnName);
                            string dbType = "DbType." + (field.Enum ? "Int32" : TypeHelper.BasicStrongName(field.TypeName));
                            if (field.TypeName ==  "string" && field.Length > 0)
                                dbType += ", " + field.Length;

                            col += dbType + ", ColumnProperty.{0}Null".F(field.Nullable ? "" : "Not");
                            if (field.DefaultValue != "")
                                col += field.DefaultValue;

                            col += ")";
                            if (i < options.Fields.Count - 1)
                                col += ",";

                            up.Body.Add(ind + col);
                        }
                    }
                    up.Body.Add(");");

                    down.Body.Add("Database.RemoveEntityTable(\"{0}\");".F(options.TableName));
                }
                else // view
                {
                    up.Body.Add("Database.ExecuteNonQuery(@\"create or replace view {0}".F(options.TableName.ToLower()));
                    up.Body.Add("    (");
                    for (int i = 0; i < options.Fields.Count; i++)
                    {
                        var field = options.Fields[i];
                        up.Body.Add("        {0}{1}".F(field.ColumnName.ToLower(), i != options.Fields.Count-1 ? "," : ""));
                    }
                    up.Body.Add("    )");
                    up.Body.Add("    as");
                    up.Body.Add("    select");
                    for (int i = 0; i < options.Fields.Count; i++)
                    {
                        var field = options.Fields[i];
                        up.Body.Add("        {0}{1}".F(field.ColumnName.ToLower(), i != options.Fields.Count - 1 ? "," : ""));
                    }
                    up.Body.Add("    from mosks_" + options.ClassName.Substring(0, options.ClassName.Length - 4).CamelToSnake().ToLower());
                    up.Body.Add("\");");

                    down.Body.Add("Database.ExecuteNonQuery(\"drop view {0}\");".F(options.TableName.ToLower()));
                }
            }

            cls.AddMethod(up);
            cls.AddMethod(down);

            file.Name = "UpdateSchema.cs";
            file.Path = "Migrations\\" + folderVersion;
            file.Body = ns.Generate();
            return files;
        }

    }
}

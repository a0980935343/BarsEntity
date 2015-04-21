using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class MigrationGenerator : BaseBarsGenerator
    {
        public override void Generate(EnvDTE.Project project, EntityOptions options)
        {
            base.Generate(project, options);

            string folderVersion = "Version_{0}".F(options.MigrationVersion);

            CheckFolder("Migrations\\"+folderVersion);

            var ns = new NamespaceInfo();
            var cls = new ClassInfo();
            ns.NestedValues.Add(cls);
            ns.Name = "{0}.Migrations.{1}".F(project.Name, folderVersion);

            ns.InnerUsing.Add("System.Data");
            ns.InnerUsing.Add("ECM7.Migrator.Framework");
            ns.InnerUsing.Add("B4.Modules.ECM7.DatabaseExtensions");

            long outV = 0;
            if (!long.TryParse(options.MigrationVersion.Replace("_", ""), out outV))
                throw new Exception("Версия должна состоять только из цифр и символа '_'");

            cls.Attributes.Add("Migration({0})".F(options.MigrationVersion.Replace("_", "")));

            cls.Name = "UpdateSchema";
            cls.BaseClass = "Migration";
            
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
                        if (IsReference(field))
                        {
                            up.Body.Add(ind + "new RefColumn(\"{0}\", \"{1}\", \"{2}\", \"ID\")".F(field.ColumnName, field.Index, field.ReferenceTable) + (i < options.Fields.Count - 1 ? "," : ""));
                        }
                        else if (IsBasicType(field.TypeName))
                        {
                            string col = "new Column(\"{0}\", ".F(field.ColumnName);
                            string dbType = "DbType.";
                            switch (field.TypeName)
                            {
                                case "string": dbType += "String, " + field.Length; break;
                                case "DateTime": dbType += "DateTime"; break;
                                case "int": dbType += "Int32"; break;
                                case "long": dbType += "Int64"; break;
                                case "bool": dbType += "Boolean"; break;
                                case "decimal": dbType += "Decimal"; break;
                                case "double": dbType += "Decimal"; break;
                                default: dbType += field.TypeName; break;
                            }
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
                    up.Body.Add("    from dual");
                    up.Body.Add("\");");

                    down.Body.Add("Database.ExecuteNonQuery(\"drop view {0}\");".F(options.TableName.ToLower()));
                }
            }

            cls.AddMethod(up);
            cls.AddMethod(down);

            options.ResultFile = "Migrations\\" +folderVersion + "\\UpdateSchema.cs";

            if (File.Exists(Path.Combine(_projectFolder, options.ResultFile)))
                throw new Exception("Файл '{0}' уже существует! Измените версию миграции".F(options.ResultFile));

            var pi = CreateFile(options.ResultFile, ns.ToString());
        }

    }
}

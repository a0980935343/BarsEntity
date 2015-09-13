using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace Barsix.BarsEntity
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public static class CodeClassExt
    {
        public static EntityOptions ToOptions(CodeClass cls, IEnumerable<string> enums)
        {
            EntityOptions result = new EntityOptions();

            result.ClassName = cls.Name;
            result.ClassFullName = cls.FullName;

            try
            {
                result.DisplayName = CodeObjectExt.GetSummary(cls.DocComment);
            }
            catch
            {
            }

            foreach (var bs in cls.Bases)
            {
                if (bs is CodeClass)
                    result.BaseClass = ((CodeClass)bs).Name;
            }

            foreach (var bs in cls.ImplementedInterfaces)
            {
                if (bs is CodeInterface && ((CodeInterface)bs).Name == "IStatefulEntity")
                    result.Stateful = true;

                if (bs is CodeInterface && ((CodeInterface)bs).Name == "ISignableEntity")
                    result.Signable = true;
            }

            result.Controller = new ControllerOptions { Name = result.ClassName };
            result.View = new ViewOptions { Type = ViewType.EAS };
            result.TableName = cls.FullName.Split('.')[1].ToUpper() + "_" + cls.Name.CamelToSnake();
            result.MigrationVersion = DateTime.Now.ToString("yyyy_MM_dd_00");

            foreach (CodeProperty prop in cls.Members.OfType<CodeProperty>())
            {
                FieldOptions field = new FieldOptions() { FieldName = prop.Name, ColumnName = prop.Name.CamelToSnake() };

                try
                {
                    field.Comment = field.DisplayName = CodeObjectExt.GetSummary(prop.DocComment);
                }
                catch
                {
                }

                string type = prop.Type.AsFullName;
                if (type.StartsWith("System.") && type.Split('.').Length == 2)
                {
                    type = type.Split('.')[1];

                    if (TypeHelper.IsBasicType(type))
                        type = TypeHelper.BasicAlias(type);
                } else
                if (type.StartsWith("System.Nullable<"))
                {
                    type = type.Split('.').ToList().Last();
                    type = type.Substring(0, type.Length - 1);
                    field.Enum = enums.Any(x => x.EndsWith("." + type));

                    if (TypeHelper.IsBasicType(type))
                        type = TypeHelper.BasicAlias(type);
                    field.Nullable = true;
                } else
                if (type.StartsWith("System.Collections.Generic.IList<"))
                {
                    type = type.Split('.').ToList().Last();
                    type = type.Substring(0, type.Length - 1);

                    field.Collection = true;
                }
                else
                {
                    type = type.Split('.').ToList().Last();

                    field.Enum = enums.Any(x => x.EndsWith("." + type));

                    field.Index = cls.Name.CamelToSnake() + "__" + field.ColumnName;
                    field.ReferenceTable = prop.Type.AsFullName.Split('.')[1].ToUpper() + "_" + type.CamelToSnake();
                    field.ColumnName = field.ColumnName + (field.Enum ? "" : "_ID");
                }

                field.TypeName = type;
                if (field.FieldName == "Parent" && field.TypeName == result.ClassName)
                {
                    field.ParentReference = true;
                    field.Nullable = true;
                    result.View.TreeGrid = true;
                }

                result.Fields.Add(field);
            }

            return result;
        }
    }
}

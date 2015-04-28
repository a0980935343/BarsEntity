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
        public static EntityOptions ToOptions(CodeClass cls)
        {
            EntityOptions result = new EntityOptions();

            result.ClassName = cls.Name;
            result.ClassFullName = cls.FullName;

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
            result.View = new ViewOptions();
            result.TableName = cls.FullName.Split('.')[1].ToUpper() + "_" + cls.Name.CamelToSnake();
            result.MigrationVersion = DateTime.Now.ToString("yyyy_MM_dd_00");

            foreach (var el in cls.Members)
            {
                if (el is CodeProperty)
                {
                    FieldOptions field = new FieldOptions() { FieldName = ((CodeProperty)el).Name, ColumnName = ((CodeProperty)el).Name.CamelToSnake() };

                    string type = ((CodeProperty)el).Type.AsFullName;
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
                        field.Index = cls.Name.CamelToSnake() + "__" + field.ColumnName;
                        field.ReferenceTable = ((CodeProperty)el).Type.AsFullName.Split('.')[1].ToUpper() + "_" + type.CamelToSnake();
                        field.ColumnName = field.ColumnName + "_ID";
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
            }

            return result;
        }
    }
}

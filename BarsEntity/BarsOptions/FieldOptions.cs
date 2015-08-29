using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Barsix.BarsEntity.BarsOptions
{
    using BarsGenerators;

    public class FieldOptions
    {
        public string TypeName = "string";

        public bool Collection;

        public string FieldName;

        public string ColumnName;

        public string DisplayName;

        public string ViewType = "textfield";
        public string ViewColumnType = "easwraptextcolumn";

        public string TextProperty = "Name";

        public string Comment;

        public string DefaultValue;

        public bool Nullable;

        public bool Enum;

        public int Length;

        public bool OwnerReference;

        public bool ParentReference;

        public string ReferenceTable;

        public string Index;

        public bool DynamicFilter;

        public string DynamicFilterType;

        public bool GroupField;

        internal void SetRelatedTypes()
        {
            if (this.IsBasicType())
            {
                switch (TypeName)
                {
                    case "string": 
                        ViewType = "textfield"; 
                        ViewColumnType = "easwraptextcolumn";
                        DynamicFilterType = "String";
                        break;
                    case "long": 
                        ViewType = "numberfield"; 
                        ViewColumnType = "easwraptextcolumn";
                        DynamicFilterType = "Long";
                        break;
                    case "bool": ViewType = "checkbox"; ViewColumnType = "checkcolumn"; DynamicFilterType = "Boolean"; break;
                    case "int": ViewType = "numberfield"; ViewColumnType = "easwraptextcolumn"; DynamicFilterType = "Integer"; break;
                    case "DateTime": ViewType = "datefield"; ViewColumnType = "easgriddatecolumn"; DynamicFilterType = "DateTime"; break;
                    case "decimal": ViewType = "eascurrencyfield"; ViewColumnType = "eascurrencycolumn"; DynamicFilterType = "Decimal"; break;
                }
            }
            else if (this.IsReference())
            {
                ViewType = "easselectfield";
                DynamicFilterType = "LongSet";
            }
        }

        public string FullTypeName { get { return Collection ? "IList<{0}>".R(TypeName) : TypeName + (Nullable && (this.IsBasicType() || this.TypeName.EndsWith("Enum")) && TypeName != "string" ? "?" : ""); } }
    }
}

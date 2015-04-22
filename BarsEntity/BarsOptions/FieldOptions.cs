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

        public int Length = 100;

        public bool JsonIgnore;

        public bool OwnerReference;

        public bool ParentReference;

        public string ReferenceTable;

        public string Index;

        public bool DynamicFilter;

        public string DynamicFilterType;

        internal void SetRelatedTypes()
        {
            if (BarsGenerators.BaseBarsGenerator.IsBasicType(TypeName))
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
            else if (BarsGenerators.BaseBarsGenerator.IsReference(this))
            {
                ViewType = "easselectfield";
                DynamicFilterType = "LongSet";
            }
        }

        public string FullTypeName { get { return Collection ? "IList<{0}>".F(TypeName) : TypeName + (Nullable && BaseBarsGenerator.IsBasicType(TypeName) && TypeName != "string" ? "?" : ""); } }
    }
}

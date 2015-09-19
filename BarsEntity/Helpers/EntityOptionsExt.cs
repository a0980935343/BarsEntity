using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace Barsix.BarsEntity
{
    using BarsOptions;

    public static class EntityOptionsExt
    {
        public static void Map(this EntityOptions opts, TextBox text, Func<EntityOptions, string> selector)
        {
            text.Text = selector.Invoke(opts);
        }

        public static void Map(this EntityOptions opts, CheckBox check, Func<EntityOptions, bool> selector)
        {
            check.Checked = selector.Invoke(opts);
        }

        public static void Map(this EntityOptions opts, ComboBox combo, Func<EntityOptions, string> selector)
        {
            if (combo.DropDownStyle == ComboBoxStyle.DropDownList)
            {
                for (int i = 0; i < combo.Items.Count; i++)
                {
                    if (combo.Items[i].ToString() == selector.Invoke(opts))
                        combo.SelectedIndex = i;
                }
            }
            else
            {
                combo.Text = selector.Invoke(opts);
            }
        }

        public static EntityOptions Load(string fileName)
        {
            XmlSerializer xml = new XmlSerializer(typeof(EntityOptions));
            StreamReader reader = new StreamReader(fileName);
            EntityOptions options = (EntityOptions)xml.Deserialize(reader);
            reader.Close();
            return options;
        }

        public static void Save(this EntityOptions options, string fileName)
        {
            XmlSerializer xml = new XmlSerializer(typeof(EntityOptions));
            StreamWriter writer = new StreamWriter(fileName);
            xml.Serialize(writer, options);
            writer.Close();
        }

        public static bool IsStandartBaseClass(this EntityOptions options, string className)
        {
            return className == "BaseEntity" || (className == "NamedBaseEntity" && options.Profile is MosKsProfile) || className == "PersistentObject";
        }

        public static string MapBaseClass(this EntityOptions options)
        {
            if (!options.IsStandartBaseClass(options.BaseClass))
            {
                return options.InheritanceType == InheritanceType.BaseJoinedClass ? "BaseJoinedSubclassMap" : "SubclassMap";
            }
            else
            {
                return options.BaseClass + "Map";
            }
        }
    }
}

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
        public static void Map(this EntityOptions opts, TextBox tb, Func<EntityOptions, string> selector)
        {
            tb.Text = selector.Invoke(opts);
        }

        public static void Map(this EntityOptions opts, CheckBox tb, Func<EntityOptions, bool> selector)
        {
            tb.Checked = selector.Invoke(opts);
        }

        public static void Map(this EntityOptions opts, ComboBox tb, Func<EntityOptions, string> selector)
        {
            for(int i=0; i<tb.Items.Count; i++)
            {
                if (tb.Items[i].ToString() == selector.Invoke(opts))
                    tb.SelectedIndex = i;
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
    }
}

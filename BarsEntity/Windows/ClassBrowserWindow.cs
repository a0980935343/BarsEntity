using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Barsix.BarsEntity.Windows
{
    using BarsOptions;

    public partial class ClassBrowserWindow : Form
    {
        public ClassBrowserWindow(IEnumerable<EntityOptions> classes)
        {
            InitializeComponent();
            _classes = classes;

            cbNamespace.Items.Clear();
            foreach (var @class in classes)
            {
                cbNamespace.Items.Add(@class.ClassFullName);
            }
            cbNamespace.SelectedIndex = 0;
        }

        public EntityOptions Class;

        private IEnumerable<EntityOptions> _classes;

        private void SelectClass(EntityOptions @class)
        {
            lbMembers.Items.Clear();
            foreach (var member in @class.Fields)
            {
                lbMembers.Items.Add(member.FieldName).SubItems.Add(member.FullTypeName);
            }
            chState.Checked = @class.Stateful;
            chSign.Checked = @class.Signable;
            chTree.Checked = @class.View.TreeGrid;
            Class = @class;
        }

        private void cbNamespace_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = cbNamespace.SelectedIndex;
            if (idx > -1 && idx < _classes.Count())
                SelectClass(_classes.ToArray()[idx]);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Barsix.BarsEntity
{
    using BarsOptions;
    using BarsGenerators;

    public partial class ConfirmCreationParts : Form
    {
        public ConfirmCreationParts()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void UncheckAllExcept(CheckBox checkBox)
        {
            foreach (var control in Controls)
            {
                if (control is CheckBox && (CheckBox)control != checkBox)
                {
                    ((CheckBox)control).Checked = false;
                }
            }
            checkBox.Checked = true;
        }

        private void btnViewModel_Click(object sender, EventArgs e)
        {
            UncheckAllExcept(chViewModel);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

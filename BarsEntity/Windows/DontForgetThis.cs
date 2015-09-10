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
    public partial class DontForgetThis : Form
    {
        public BarsGenerators.GenerationManager GenerationManager;

        public DontForgetThis()
        {
            InitializeComponent();
        }

        private void DontForgetThis_Load(object sender, EventArgs e)
        {

        }

        private void btnCopyClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);
        }

        private void btnInsertFragments_Click(object sender, EventArgs e)
        {
            GenerationManager.InsertFragments();
        }
    }
}

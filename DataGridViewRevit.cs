using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FindLabel
{
    public partial class DataGridViewRevit : Form
    {
        public Class1 accessC1 = new Class1(); // nova instancia accessC1 do tipo Class1

        public List<List<string>> Labels { get; set; }
        public DataGridViewRevit()
        {
            Labels = GetLabelsFound();
            InitializeComponent();
        }

        private List<List<string>> GetLabelsFound()
        {
            var list = new List<List<string>>();
            List<List<string>> list2 = accessC1.LabelsFound;
            list = accessC1.ReturnLabelsFoundList();  // Devolve a lista dos labels encontrados para fazer a DataGridView
            return list2;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void dataGridView1_Load(object sender, DataGridViewCellEventArgs e)
        {
            var labels = this.Labels;
            dataGridView1.DataSource = labels;
        }
    }
}

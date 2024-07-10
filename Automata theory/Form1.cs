using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Automata_theory
{
    public partial class Laba6Pi : Form
    {
        public Laba6Pi()
        {
            InitializeComponent();
        }

        private void Laba6Img_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                dataGridView1.Rows.Add();
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }

        private bool CheckCells()
        {
            List<List<List<int>>> result = new List<List<List<int>>>();
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    List<List<int>> list = new List<List<int>>();
                    for (int j = 0; j < 2; j++)
                    {
                        string value = dataGridView1.Rows[i].Cells[j].Value.ToString();
                        List<int> ints = new List<int>();
                        for (int k = 0; k < value.Length; k++)
                        {
                            ints.Add(int.Parse($"{value[k]}") - 1);
                        }
                        list.Add(ints);
                    }
                    result.Add(list);
                }
            }
            catch { return false; }
            Ribbon1.ExternalPi = result;
            Ribbon1.IsPiEnteredExternally = true;
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckCells())
                this.Close();
        }
    }
}

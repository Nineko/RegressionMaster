using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RegressionMaster
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
       
        public void PasteInData(ref DataGridView dgv)
        {
            this.DebubText.AppendText("PasteInData");
            char[] rowSplitter = { '\n', '\r' };  // Cr and Lf.
            char[] columnSplitter = { '\t', ',' };           // Tab. , 

            IDataObject dataInClipboard = Clipboard.GetDataObject();

            string stringInClipboard =
                dataInClipboard.GetData(DataFormats.Text).ToString();

            string[] rowsInClipboard = stringInClipboard.Split(rowSplitter,
                StringSplitOptions.RemoveEmptyEntries);

            int r = dgv.SelectedCells[0].RowIndex;
            int c = dgv.SelectedCells[0].ColumnIndex;

            if (dgv.Rows.Count < (r + rowsInClipboard.Length))
                dgv.Rows.Add(r + rowsInClipboard.Length - dgv.Rows.Count);

            // Loop through lines:

            int iRow = 0;
            while (iRow < rowsInClipboard.Length)
            {
                // Split up rows to get individual cells:

                string[] valuesInRow =
                    rowsInClipboard[iRow].Split(columnSplitter);

                // Cycle through cells.
                // Assign cell value only if within columns of grid:

                int jCol = 0;
                while (jCol < valuesInRow.Length)
                {
                    if ((dgv.ColumnCount - 1) >= (c + jCol))
                        dgv.Rows[r + iRow].Cells[c + jCol].Value =
                        valuesInRow[jCol];

                    jCol += 1;
                } 

                iRow += 1;
            } 
        }

        private void dataGridView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.DebubText.AppendText("Form1_KeyPress");
            char value = e.KeyChar;
            int key = (int)value;
            if (key != 22)
                return;
            PasteInData(ref this.dataGridView1);
        }

        private void dataGridView2_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.DebubText.AppendText("Form2_KeyPress");
            char value = e.KeyChar;
            int key = (int)value;
            if (key != 22)
                return;
            PasteInData(ref this.dataGridView2);
        }
    }

}

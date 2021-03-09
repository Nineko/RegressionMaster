using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;


namespace RegressionMaster
{
    public partial class MainForm : Form
    {
        public class DataBox
        {
            public DataBox(int size,string _name)
            {
                name = _name;
                GroundTrue  = new double[size];
                VisionPixel = new double[size];
                AfterRegression = new double[size];
                Diff = new double[size];
                A = 0;
                B = 0;
                R2 = 0;
            }
            public string name;
            public double[] GroundTrue;
            public double[] VisionPixel;
            public double[] AfterRegression;
            public double[] Diff;
            public double A;
            public double B;
            public double R2;

        }

        public List<DataBox> GroupDataBox;

        List<int>RegressionIndex;

        public MainForm()
        {
            InitializeComponent();
            GroupDataBox = new List<DataBox>();
            this.dataGridView1.Rows.Add();
            this.dataGridView2.Rows.Add();
            this.dataGridView1.Rows[0].Cells[0].Value = true;
            this.dataGridView2.Rows[0].Cells[0].Value = true;
        }
        protected override void OnShown(EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView2.ClearSelection();
        }

        public void PasteInData(ref DataGridView dgv)
        {
            char[] rowSplitter = { '\n', '\r' };  // Cr and Lf.
            char[] columnSplitter = { '\t', ',' };           // Tab. , 

            IDataObject dataInClipboard = Clipboard.GetDataObject();

            string stringInClipboard = dataInClipboard.GetData(DataFormats.Text).ToString();

            string[] rowsInClipboard = stringInClipboard.Split(rowSplitter, StringSplitOptions.RemoveEmptyEntries);

            int ClipboardColsNum = (rowsInClipboard[0].Split(columnSplitter)).Length;


            int r = dgv.SelectedCells[0].RowIndex;
            int c = dgv.SelectedCells[0].ColumnIndex;


            if (dgv.Rows.Count < (r + rowsInClipboard.Length))
                dgv.Rows.Add(r + rowsInClipboard.Length - dgv.Rows.Count);


            if (dgv.ColumnCount < (c + ClipboardColsNum))
                dgv.ColumnCount = (c + ClipboardColsNum);

            for (int i = 1; i < dgv.ColumnCount; i++)
            {
                string NewColName = Convert.ToChar(64+i).ToString();
                dgv.Columns[i].Name = NewColName;
                dgv.Columns[i].HeaderText = NewColName;
            }

            // Loop through lines:

            int iRow = 0;
            while (iRow < rowsInClipboard.Length)
            {
                string[] valuesInRow = rowsInClipboard[iRow].Split(columnSplitter);
                if (iRow!=0)
                    dgv.Rows[r + iRow].HeaderCell.Value = iRow.ToString();
                int jCol = 0;
                while (jCol < valuesInRow.Length)
                {
                    if ((dgv.ColumnCount - 1) >= (c + jCol))
                        dgv.Rows[r + iRow].Cells[c + jCol].Value =valuesInRow[jCol];
                    jCol += 1;
                }

                dgv.Rows[r + iRow].Cells[0].Value = true;
                iRow += 1;
            }
        }

        private void DataGridView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char value = e.KeyChar;
            int key = (int)value;
            if (key != 22)
                return;
            PasteInData(ref this.dataGridView1);
        }

        private void DataGridView2_KeyPress(object sender, KeyPressEventArgs e)
        {
            char value = e.KeyChar;
            int key = (int)value;
            if (key != 22)
                return;
            PasteInData(ref this.dataGridView2);
        }

        private void RegressionBtn_Click(object sender, EventArgs e)
        {
            if (HaveSameColsRows())
            {
                DataColle();
                IniTagPage();
                DoRegression();
                FillupTabPage();
            }
        }

        private bool HaveSameColsRows()
        {
            bool same = false;

            if (this.dataGridView1.ColumnCount == this.dataGridView2.ColumnCount && this.dataGridView1.RowCount == this.dataGridView2.RowCount)      
                same = true;
            else
                MessageBox.Show("資料筆數(Column/Row)不一致，請確認後再嘗試迴歸.");

            return same;
        }

        private void DataColle()
        {
            GroupDataBox = new List<DataBox>();
            RegressionIndex = new List<int>();
            int Category = dataGridView1.ColumnCount - 1;
            int Row = dataGridView1.RowCount - 1;

            for (int r = 0; r < Row; r++)
                if ((bool)dataGridView1.Rows[r + 1].Cells[0].Value)
                    RegressionIndex.Add(r + 1);


            for (int col = 0; col < Category; col++)
            {
                DataBox dataBox = new DataBox(Row, dataGridView1.Rows[0].Cells[col + 1].Value.ToString());
                List<double> _GroundTrue = new List<double>();
                List<double> _VisionPixel = new List<double>();
                for (int r = 0; r < Row; r++)
                {
                    if ((bool)dataGridView1.Rows[r + 1].Cells[0].Value)
                    {
                        _GroundTrue.Add(Convert.ToDouble(dataGridView1.Rows[r + 1].Cells[col + 1].Value));
                        _VisionPixel.Add(Convert.ToDouble(dataGridView2.Rows[r + 1].Cells[col + 1].Value));
                    }
                }

                dataBox.GroundTrue = _GroundTrue.ToArray();
                dataBox.VisionPixel = _VisionPixel.ToArray();
                GroupDataBox.Add(dataBox);
            }


        }

        private TabPage CustomTabPage(string name)
        {
            TabPage newTabPage = new TabPage(name);

            #region Define
            
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Base = new System.Windows.Forms.TableLayoutPanel();
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Param = new System.Windows.Forms.TableLayoutPanel();
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Result = new System.Windows.Forms.TableLayoutPanel();
            System.Windows.Forms.TextBox textBox_B= new System.Windows.Forms.TextBox();
            System.Windows.Forms.TextBox textBox_A= new System.Windows.Forms.TextBox();
            System.Windows.Forms.TextBox textBox_R2= new System.Windows.Forms.TextBox();
            System.Windows.Forms.Label label_B= new System.Windows.Forms.Label();
            System.Windows.Forms.Label label_A= new System.Windows.Forms.Label();
            System.Windows.Forms.Label label_R2= new System.Windows.Forms.Label();
            System.Windows.Forms.DataVisualization.Charting.Chart chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataGridView dataGridView1 = new System.Windows.Forms.DataGridView(); ;
            System.Windows.Forms.DataGridViewTextBoxColumn NO= new System.Windows.Forms.DataGridViewTextBoxColumn();
            System.Windows.Forms.DataGridViewTextBoxColumn Ground= new System.Windows.Forms.DataGridViewTextBoxColumn();
            System.Windows.Forms.DataGridViewTextBoxColumn Vision= new System.Windows.Forms.DataGridViewTextBoxColumn();
            System.Windows.Forms.DataGridViewTextBoxColumn AfterRegression= new System.Windows.Forms.DataGridViewTextBoxColumn();
            System.Windows.Forms.DataGridViewTextBoxColumn Diff= new System.Windows.Forms.DataGridViewTextBoxColumn();

            #endregion

            #region Base

            #region Result

            #region Param

            #region textBox_B
            textBox_B.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox_B.Location = new System.Drawing.Point(297, 30);
            textBox_B.Name = "textBox_B";
            textBox_B.Size = new System.Drawing.Size(93, 22);
            textBox_B.TabIndex = 3;
            #endregion    
            #region textBox_A
            textBox_A.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox_A.Location = new System.Drawing.Point(101, 30);
            textBox_A.Name = "textBox_A";
            textBox_A.Size = new System.Drawing.Size(92, 22);
            textBox_A.TabIndex = 3;
            #endregion
            #region textBox_R2
             
            textBox_R2.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox_R2.Location = new System.Drawing.Point(101, 3);
            textBox_R2.Name = "textBox_R2";
            textBox_R2.Size = new System.Drawing.Size(92, 22);
            textBox_R2.TabIndex = 3;
            #endregion
            #region label_B
            label_B.AutoSize = true;
            label_B.Dock = System.Windows.Forms.DockStyle.Fill;
            label_B.Location = new System.Drawing.Point(199, 30);
            label_B.Margin = new System.Windows.Forms.Padding(3);
            label_B.Name = "label_B";
            label_B.Size = new System.Drawing.Size(92, 21);
            label_B.TabIndex = 3;
            label_B.Text = "B";
            label_B.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            #endregion
            #region label_A
            label_A.AutoSize = true;
            label_A.Dock = System.Windows.Forms.DockStyle.Fill;
            label_A.Location = new System.Drawing.Point(3, 30);
            label_A.Margin = new System.Windows.Forms.Padding(3);
            label_A.Name = "label_A";
            label_A.Size = new System.Drawing.Size(92, 21);
            label_A.TabIndex = 3;
            label_A.Text = "A";
            label_A.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            #endregion
            #region label_R2    
            label_R2.AutoSize = true;
            label_R2.Dock = System.Windows.Forms.DockStyle.Fill;
            label_R2.Location = new System.Drawing.Point(3, 3);
            label_R2.Margin = new System.Windows.Forms.Padding(3);
            label_R2.Name = "label_R2";
            label_R2.Size = new System.Drawing.Size(92, 21);
            label_R2.TabIndex = 3;
            label_R2.Text = "R平方值";
            label_R2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            #endregion

            #region tableLayoutPanel_Param
            tableLayoutPanel_Param.ColumnCount = 4;
            tableLayoutPanel_Param.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel_Param.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel_Param.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel_Param.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel_Param.Controls.Add(textBox_B, 3, 1);
            tableLayoutPanel_Param.Controls.Add(textBox_A, 1, 1);
            tableLayoutPanel_Param.Controls.Add(textBox_R2, 1, 0);
            tableLayoutPanel_Param.Controls.Add(label_B, 2, 1);
            tableLayoutPanel_Param.Controls.Add(label_A, 0, 1);
            tableLayoutPanel_Param.Controls.Add(label_R2, 0, 0);
            tableLayoutPanel_Param.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel_Param.Location = new System.Drawing.Point(3, 3);
            tableLayoutPanel_Param.Name = "tableLayoutPanel_Param";
            tableLayoutPanel_Param.RowCount = 2;
            tableLayoutPanel_Param.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel_Param.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel_Param.Size = new System.Drawing.Size(393, 54);
            tableLayoutPanel_Param.TabIndex = 2;
            #endregion
            #endregion

            #region DataGirdView
 
            #region dataGridView1
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            NO,
            Ground,
            Vision,
            AfterRegression,
            Diff});
            dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView1.Location = new System.Drawing.Point(-239, 189);
            dataGridView1.Name = "RegressionDataGridView";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowTemplate.Height = 24;
            dataGridView1.Size = new System.Drawing.Size(464, 150);
            dataGridView1.TabIndex = 2;
            dataGridView1.CellClick+= new System.Windows.Forms.DataGridViewCellEventHandler(RegressionDataGridView_CellClick);
            #endregion

            #region NO
            NO.FillWeight = 50F;
            NO.HeaderText = "No.";
            NO.Name = "NO";
            NO.ReadOnly = true;
            #endregion
            #region Ground
            Ground.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            Ground.DefaultCellStyle.Format = "N3";
            Ground.HeaderText = "精測數據";
            Ground.Name = "Ground";
            Ground.ReadOnly = true;
            #endregion
            #region Vision
            Vision.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            Vision.DefaultCellStyle.Format = "N3";
            Vision.HeaderText = "視覺量測";
            Vision.Name = "Vision";
            Vision.ReadOnly = true;
            #endregion
            #region AfterRegression 
            AfterRegression.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            AfterRegression.DefaultCellStyle.Format = "N3";
            AfterRegression.HeaderText = "線性轉換";
            AfterRegression.Name = "AfterRegression";
            AfterRegression.ReadOnly = true;
            #endregion
            #region Diff 
            Diff.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            Diff.DefaultCellStyle.Format = "N3";
            Diff.HeaderText = "與精測差";
            Diff.Name = "Diff";
            Diff.ReadOnly = true;
            #endregion

            #endregion

            #region tableLayoutPanel_Result
            tableLayoutPanel_Result.ColumnCount = 1;
            tableLayoutPanel_Result.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Result.Controls.Add(tableLayoutPanel_Param, 0, 0);
            tableLayoutPanel_Result.Controls.Add(dataGridView1, 0, 1);
            tableLayoutPanel_Result.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel_Result.Location = new System.Drawing.Point(535, 3);
            tableLayoutPanel_Result.Name = "tableLayoutPanel_Result";
            tableLayoutPanel_Result.RowCount = 2;
            tableLayoutPanel_Result.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            tableLayoutPanel_Result.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Result.Size = new System.Drawing.Size(399, 577);
            tableLayoutPanel_Result.TabIndex = 1;
            #endregion

            #endregion

            #region chart1
            chartArea1.AxisX.Title = "Pixel";
            chartArea1.AxisY.Title = "mm";
            chartArea1.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea1);
            chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Alignment = System.Drawing.StringAlignment.Far;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend1.Name = "Legend1";
            chart1.Legends.Add(legend1);
            chart1.Location = new System.Drawing.Point(3, 3);
            chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series1.Legend = "Legend1";
            series1.Name = "視覺量測-精測";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series2.Legend = "Legend1";
            series2.Name = "視覺量測-迴歸後";
            chart1.Series.Add(series1);
            chart1.Series.Add(series2);
            chart1.Size = new System.Drawing.Size(526, 577);
            chart1.TabIndex = 2;
            chart1.Text = "chart1";
            #endregion

            #region tableLayoutPanel_Base
            tableLayoutPanel_Base.ColumnCount = 2;
            tableLayoutPanel_Base.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Base.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 405F));
            tableLayoutPanel_Base.Controls.Add(tableLayoutPanel_Result, 1, 0);
            tableLayoutPanel_Base.Controls.Add(chart1, 0, 0);
            tableLayoutPanel_Base.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel_Base.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel_Base.Name = "tableLayoutPanel_Base";
            tableLayoutPanel_Base.RowCount = 1;
            tableLayoutPanel_Base.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Base.Size = new System.Drawing.Size(937, 583);
            tableLayoutPanel_Base.TabIndex = 0;
            #endregion

            #endregion

            newTabPage.Controls.Add(tableLayoutPanel_Base);

            return newTabPage;
        }

        private void IniTagPage()
        {
            int Category = GroupDataBox.Count;
            //刪除現有 TabPage 至只剩 Data TabPage
            while (this.tabControl1.TabCount>1)
            {
                this.tabControl1.TabPages.RemoveAt(1);
            }

            #region OverviewDataGrid
            System.Windows.Forms.DataGridView OverViewdataGridView = new System.Windows.Forms.DataGridView();
            List<System.Windows.Forms.DataGridViewTextBoxColumn> FAIs = new List<DataGridViewTextBoxColumn>();

            OverViewdataGridView.AllowUserToAddRows = false;
            OverViewdataGridView.AllowUserToDeleteRows = false;
            OverViewdataGridView.AllowUserToResizeColumns = false;
            OverViewdataGridView.AllowUserToResizeRows = false;
            OverViewdataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            OverViewdataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            OverViewdataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            OverViewdataGridView.Name = "OverViewdataGridView";
            OverViewdataGridView.ReadOnly = true;
            OverViewdataGridView.RowHeadersVisible = false;
            OverViewdataGridView.RowTemplate.Height = 24;

            System.Windows.Forms.DataGridViewTextBoxColumn No = new DataGridViewTextBoxColumn();
            No.FillWeight = 50F;
            No.HeaderText = "No.";
            No.Name = "NO";
            No.ReadOnly = true;
            FAIs.Add(No);

            #endregion

            //新增 FAI TabPage
            for (int i = 0; i < Category; i++)
            {
                this.tabControl1.TabPages.Add(CustomTabPage(GroupDataBox[i].name));
                System.Windows.Forms.DataGridViewTextBoxColumn FAI = new DataGridViewTextBoxColumn();
                FAI.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                FAI.DefaultCellStyle.Format = "N3";
                FAI.HeaderText = GroupDataBox[i].name;
                FAI.Name = GroupDataBox[i].name;
                FAI.ReadOnly = true;
                FAIs.Add(FAI);
            }


            //新增 Overview TabPage
            OverViewdataGridView.Columns.AddRange(FAIs.ToArray());
            TabPage overviewTabPage = new TabPage("Overview");
            overviewTabPage.Controls.Add(OverViewdataGridView);
            this.tabControl1.TabPages.Add(overviewTabPage);
        }

        private void DoRegression()
        {
            Console.WriteLine(string.Join(",", RegressionIndex));
            int Category = GroupDataBox.Count;
            int Row = RegressionIndex.Count;

            if (Row>0)
            {
                for (int i = 0; i < Category; i++)
                {
                    double[] ratio;
                    List<double> _AfterRegression = new List<double>();
                    List<double> _Diff = new List<double>();
                    if (this.isInterceptZero.Checked)
                    {
                        ratio = RegressionWizard.LinearInterceptZero(GroupDataBox[i].GroundTrue, GroupDataBox[i].VisionPixel);
                        GroupDataBox[i].A = ratio[0];
                        GroupDataBox[i].B = 0;
                    }
                    else
                    {
                        ratio = RegressionWizard.Linear(GroupDataBox[i].GroundTrue, GroupDataBox[i].VisionPixel);
                        GroupDataBox[i].A = ratio[1];
                        GroupDataBox[i].B = ratio[0];
                    }

                    for (int r = 0; r < Row; r++)
                    {
                        double afterregression = GroupDataBox[i].VisionPixel[r] * GroupDataBox[i].A + GroupDataBox[i].B;
                        _Diff.Add(GroupDataBox[i].GroundTrue[r] - afterregression);
                        _AfterRegression.Add(afterregression);
                    }

                    GroupDataBox[i].AfterRegression = _AfterRegression.ToArray();
                    GroupDataBox[i].Diff = _Diff.ToArray();
                    GroupDataBox[i].R2 = RegressionWizard.Pearson(GroupDataBox[i].AfterRegression, GroupDataBox[i].GroundTrue);
                }
            }
        }

        private void FillupTabPage()
        {
            int Category = GroupDataBox.Count;
            int RowCount = RegressionIndex.Count;

            if (RowCount >0)
            {
                System.Windows.Forms.DataGridView OverViewdataGridView = (System.Windows.Forms.DataGridView)this.tabControl1.TabPages[Category + 1].Controls.Find("OverViewdataGridView", true)[0];
                OverViewdataGridView.Rows.Clear();
                OverViewdataGridView.Rows.Add(RowCount);
                for (int r = 0; r < RowCount; r++)
                    OverViewdataGridView.Rows[r].Cells[0].Value = RegressionIndex[r];

                for (int i = 0; i < Category; i++)
                {
                    System.Windows.Forms.DataVisualization.Charting.Chart TabPageChart = (System.Windows.Forms.DataVisualization.Charting.Chart)this.tabControl1.TabPages[i + 1].Controls.Find("chart1", true)[0];
                    TabPageChart.Series[0].Points.Clear();
                    TabPageChart.Series[1].Points.Clear();
                    TabPageChart.Series[0].Points.DataBindXY(GroupDataBox[i].VisionPixel, GroupDataBox[i].GroundTrue);
                    TabPageChart.Series[1].Points.DataBindXY(GroupDataBox[i].VisionPixel, GroupDataBox[i].AfterRegression);

                    double DeltaAxisX = (GroupDataBox[i].VisionPixel.Max() - GroupDataBox[i].VisionPixel.Min()) / 1000;
                    TabPageChart.ChartAreas[0].AxisX.Maximum = GroupDataBox[i].VisionPixel.Max() + DeltaAxisX;
                    TabPageChart.ChartAreas[0].AxisX.Minimum = GroupDataBox[i].VisionPixel.Min() - DeltaAxisX;
                    TabPageChart.ChartAreas[0].AxisX.LabelStyle.Format = "{0: 0.000}";

                    double MaxY = GroupDataBox[i].GroundTrue.Max() > GroupDataBox[i].AfterRegression.Max() ? GroupDataBox[i].GroundTrue.Max() : GroupDataBox[i].AfterRegression.Max();
                    double MinY = GroupDataBox[i].GroundTrue.Min() < GroupDataBox[i].AfterRegression.Min() ? GroupDataBox[i].GroundTrue.Min() : GroupDataBox[i].AfterRegression.Min();
                    double DeltaAxisY = (MaxY - MinY) / 1000;
                    TabPageChart.ChartAreas[0].AxisY.Maximum = MaxY + DeltaAxisY;
                    TabPageChart.ChartAreas[0].AxisY.Minimum = MinY - DeltaAxisY;
                    TabPageChart.ChartAreas[0].AxisY.LabelStyle.Format = "{0: 0.000}";

                    System.Windows.Forms.TextBox textBox_A = (System.Windows.Forms.TextBox)this.tabControl1.TabPages[i + 1].Controls.Find("textBox_A", true)[0];
                    System.Windows.Forms.TextBox textBox_B = (System.Windows.Forms.TextBox)this.tabControl1.TabPages[i + 1].Controls.Find("textBox_B", true)[0];
                    System.Windows.Forms.TextBox textBox_R2 = (System.Windows.Forms.TextBox)this.tabControl1.TabPages[i + 1].Controls.Find("textBox_R2", true)[0];

                    textBox_A.Text = Math.Round(GroupDataBox[i].A, 8).ToString();
                    textBox_B.Text = Math.Round(GroupDataBox[i].B, 8).ToString();
                    textBox_R2.Text = Math.Round(GroupDataBox[i].R2, 8).ToString();

                    System.Windows.Forms.DataGridView RegressionDataGridView = (System.Windows.Forms.DataGridView)this.tabControl1.TabPages[i + 1].Controls.Find("RegressionDataGridView", true)[0];

                    RegressionDataGridView.Rows.Clear();
                    RegressionDataGridView.Rows.Add(RowCount);
                    for (int r = 0; r < RowCount; r++)
                    {
                        RegressionDataGridView.Rows[r].Cells[0].Value = RegressionIndex[r];
                        RegressionDataGridView.Rows[r].Cells[1].Value = GroupDataBox[i].GroundTrue[r];
                        RegressionDataGridView.Rows[r].Cells[2].Value = GroupDataBox[i].VisionPixel[r];
                        RegressionDataGridView.Rows[r].Cells[3].Value = GroupDataBox[i].AfterRegression[r];
                        RegressionDataGridView.Rows[r].Cells[4].Value = GroupDataBox[i].Diff[r];
                        OverViewdataGridView.Rows[r].Cells[i + 1].Value = GroupDataBox[i].Diff[r];
                        if (GroupDataBox[i].Diff[r] >= 0.01 || GroupDataBox[i].Diff[r] <= -0.01)
                        {
                            RegressionDataGridView.Rows[r].Cells[4].Style.BackColor = Color.LightPink;
                            OverViewdataGridView.Rows[r].Cells[i + 1].Style.BackColor = Color.LightPink;
                        }
                        else
                        {
                            RegressionDataGridView.Rows[r].Cells[4].Style.BackColor = Color.LightGreen;
                            OverViewdataGridView.Rows[r].Cells[i + 1].Style.BackColor = Color.LightGreen;
                        }
                    }
                }
            }
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                if (e.RowIndex ==0)
                {
                    if ((bool)this.dataGridView1.Rows[0].Cells[0].Value)
                        for (int r = 0; r < this.dataGridView1.RowCount; r++)
                            this.dataGridView1.Rows[r].Cells[0].Value = false;
                    else
                        for (int r = 0; r < this.dataGridView1.RowCount; r++)
                            this.dataGridView1.Rows[r].Cells[0].Value = true;
                }
                else
                {
                    if ((bool)this.dataGridView1.Rows[e.RowIndex].Cells[0].Value)
                        this.dataGridView1.Rows[e.RowIndex].Cells[0].Value = false;
                    else
                        this.dataGridView1.Rows[e.RowIndex].Cells[0].Value = true;
                    
                }

                if (this.dataGridView1.RowCount == this.dataGridView2.RowCount)
                {
                    for (int r = 0; r < this.dataGridView1.RowCount; r++)
                        this.dataGridView2.Rows[r].Cells[0].Value = this.dataGridView1.Rows[r].Cells[0].Value;
                }
            }
        }

        private void DataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                if (e.RowIndex == 0)
                {
                    if ((bool)this.dataGridView2.Rows[0].Cells[0].Value)
                        for (int r = 0; r < this.dataGridView2.RowCount; r++)
                            this.dataGridView2.Rows[r].Cells[0].Value = false;
                    else
                        for (int r = 0; r < this.dataGridView2.RowCount; r++)
                            this.dataGridView2.Rows[r].Cells[0].Value = true;
                }
                else
                {
                    if ((bool)this.dataGridView2.Rows[e.RowIndex].Cells[0].Value)
                        this.dataGridView2.Rows[e.RowIndex].Cells[0].Value = false;
                    else
                        this.dataGridView2.Rows[e.RowIndex].Cells[0].Value = true;

                }

                if (this.dataGridView1.RowCount == this.dataGridView2.RowCount)
                {
                    for (int r = 0; r < this.dataGridView2.RowCount; r++)
                        this.dataGridView1.Rows[r].Cells[0].Value = this.dataGridView2.Rows[r].Cells[0].Value;
                }
            }
        }

        private void RegressionDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                System.Windows.Forms.DataVisualization.Charting.Chart TabPageChart = (System.Windows.Forms.DataVisualization.Charting.Chart)this.tabControl1.TabPages[this.tabControl1.SelectedIndex].Controls.Find("chart1", true)[0];
                System.Windows.Forms.DataVisualization.Charting.StripLine sl1 = new System.Windows.Forms.DataVisualization.Charting.StripLine();
                System.Windows.Forms.DataGridView RegressionDataGridView = (System.Windows.Forms.DataGridView)this.tabControl1.TabPages[this.tabControl1.SelectedIndex].Controls.Find("RegressionDataGridView", true)[0];


                sl1.BackColor = System.Drawing.Color.Red;
                sl1.StripWidth = 0.0005;
                int No = (int)RegressionDataGridView.Rows[e.RowIndex].Cells[0].Value;
                for (int i = 0; i < RegressionIndex.Count; i++)
                {
                    if (RegressionIndex[i] == No)
                    {
                        sl1.IntervalOffset = GroupDataBox[this.tabControl1.SelectedIndex - 1].VisionPixel[i];
                        break;
                    }
                }

                TabPageChart.ChartAreas[0].AxisX.StripLines.Clear();
                TabPageChart.ChartAreas[0].AxisX.StripLines.Add(sl1);
            }
        }
    }
}



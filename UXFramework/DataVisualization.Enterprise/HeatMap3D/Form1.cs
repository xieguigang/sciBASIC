using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeatMap3D
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        

        private void chkShowData_CheckedChanged(object sender, EventArgs e)
        {

            graphControl1.ShowValuesInMap = chkShowData.Checked;
        }

        private void chkShowUnit_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowUnit.Checked == false)
            {
                graphControl1.Unit = "";
            }
            else
            {
                graphControl1.Unit = txtUnit.Text;
            }
           
           
        }

        private void chkShowBottom_CheckedChanged(object sender, EventArgs e)
        {
            graphControl1.ShowLowerPanel = chkShowBottom.Checked; 
        }

        private void chkShowLabelOnMouseEnter_CheckedChanged(object sender, EventArgs e)
        {
            graphControl1.ShowValueLabelOnMuseEnter = chkShowLabelOnMouseEnter.Checked;
        }

        private void chkShowFixedColor_CheckedChanged(object sender, EventArgs e)
        {
            graphControl1.FixedColorScale = chkShowFixedColorScale.Checked;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        Random randomnumber = new System.Random();
        private float Random()
        {

            return (float)randomnumber.Next(0, 5);
            
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            float[,] testval = {
               {Random(),Random(),Random(),Random()},
                {Random(),Random(),Random(),Random()},
                {Random(),Random(),Random(),Random()},
                {Random(),Random(),Random(),Random()}
                                };


            graphControl1.ScaleMax = Convert.ToDouble(txtMax.Text.Trim());
            graphControl1.ScaleMin = Convert.ToDouble(txtMin.Text.Trim());

            graphControl1.Draw3DMap(testval);
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            float[,] testval = {
               {1,3.5f,2,1},
                {1,2,1,0},
                {2,1,0,1},
                {1,0.5f,0,1}
                                };


            graphControl1.ScaleMax = Convert.ToDouble(txtMax.Text.Trim());
            graphControl1.ScaleMin = Convert.ToDouble(txtMin.Text.Trim());

            graphControl1.Draw3DMap(testval);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            timer1.Start();

        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }
    }
}

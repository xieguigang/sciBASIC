using Microsoft.VisualBasic.DataVisualization.Enterprise.Gradiant3D;

namespace HeatMap3D
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnStart = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnShow = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtMin = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkShowFixedColorScale = new System.Windows.Forms.CheckBox();
            this.txtMax = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkShowUnit = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUnit = new System.Windows.Forms.TextBox();
            this.chkShowLabelOnMouseEnter = new System.Windows.Forms.CheckBox();
            this.chkShowBottom = new System.Windows.Forms.CheckBox();
            this.chkShowData = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.graphControl1 = new GraphControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(8, 19);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "&Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.MinimumSize = new System.Drawing.Size(200, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox3);
            this.splitContainer1.Panel1.Controls.Add(this.btnShow);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Controls.Add(this.chkShowLabelOnMouseEnter);
            this.splitContainer1.Panel1.Controls.Add(this.chkShowBottom);
            this.splitContainer1.Panel1.Controls.Add(this.chkShowData);
            this.splitContainer1.Panel1.Margin = new System.Windows.Forms.Padding(10);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(10);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.elementHost1);
            this.splitContainer1.Size = new System.Drawing.Size(866, 488);
            this.splitContainer1.SplitterDistance = 198;
            this.splitContainer1.TabIndex = 2;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnStart);
            this.groupBox3.Controls.Add(this.btnStop);
            this.groupBox3.Location = new System.Drawing.Point(12, 354);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(167, 53);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Timer";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(89, 19);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 10;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnShow
            // 
            this.btnShow.Location = new System.Drawing.Point(15, 25);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(75, 23);
            this.btnShow.TabIndex = 9;
            this.btnShow.Text = "Show";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtMin);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.chkShowFixedColorScale);
            this.groupBox2.Controls.Add(this.txtMax);
            this.groupBox2.Location = new System.Drawing.Point(12, 230);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(164, 118);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Scale";
            // 
            // txtMin
            // 
            this.txtMin.Location = new System.Drawing.Point(49, 84);
            this.txtMin.Name = "txtMin";
            this.txtMin.Size = new System.Drawing.Size(61, 20);
            this.txtMin.TabIndex = 9;
            this.txtMin.Text = "1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Min";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Max";
            // 
            // chkShowFixedColorScale
            // 
            this.chkShowFixedColorScale.AutoSize = true;
            this.chkShowFixedColorScale.Location = new System.Drawing.Point(8, 19);
            this.chkShowFixedColorScale.Name = "chkShowFixedColorScale";
            this.chkShowFixedColorScale.Size = new System.Drawing.Size(108, 17);
            this.chkShowFixedColorScale.TabIndex = 3;
            this.chkShowFixedColorScale.Text = "Fixed Color Scale";
            this.chkShowFixedColorScale.UseVisualStyleBackColor = true;
            this.chkShowFixedColorScale.CheckedChanged += new System.EventHandler(this.chkShowFixedColor_CheckedChanged);
            // 
            // txtMax
            // 
            this.txtMax.Location = new System.Drawing.Point(49, 58);
            this.txtMax.Name = "txtMax";
            this.txtMax.Size = new System.Drawing.Size(61, 20);
            this.txtMax.TabIndex = 10;
            this.txtMax.Text = "10";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkShowUnit);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtUnit);
            this.groupBox1.Location = new System.Drawing.Point(15, 154);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(161, 70);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Unit";
            // 
            // chkShowUnit
            // 
            this.chkShowUnit.AutoSize = true;
            this.chkShowUnit.Location = new System.Drawing.Point(6, 19);
            this.chkShowUnit.Name = "chkShowUnit";
            this.chkShowUnit.Size = new System.Drawing.Size(75, 17);
            this.chkShowUnit.TabIndex = 4;
            this.chkShowUnit.Text = "Show Unit";
            this.chkShowUnit.UseVisualStyleBackColor = true;
            this.chkShowUnit.CheckedChanged += new System.EventHandler(this.chkShowUnit_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Unit";
            // 
            // txtUnit
            // 
            this.txtUnit.Location = new System.Drawing.Point(50, 44);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new System.Drawing.Size(55, 20);
            this.txtUnit.TabIndex = 7;
            this.txtUnit.Text = "°C";
            // 
            // chkShowLabelOnMouseEnter
            // 
            this.chkShowLabelOnMouseEnter.AutoSize = true;
            this.chkShowLabelOnMouseEnter.Location = new System.Drawing.Point(27, 96);
            this.chkShowLabelOnMouseEnter.Name = "chkShowLabelOnMouseEnter";
            this.chkShowLabelOnMouseEnter.Size = new System.Drawing.Size(162, 17);
            this.chkShowLabelOnMouseEnter.TabIndex = 5;
            this.chkShowLabelOnMouseEnter.Text = "Show Label On Mouse Enter";
            this.chkShowLabelOnMouseEnter.UseVisualStyleBackColor = true;
            this.chkShowLabelOnMouseEnter.CheckedChanged += new System.EventHandler(this.chkShowLabelOnMouseEnter_CheckedChanged);
            // 
            // chkShowBottom
            // 
            this.chkShowBottom.AutoSize = true;
            this.chkShowBottom.Location = new System.Drawing.Point(27, 119);
            this.chkShowBottom.Name = "chkShowBottom";
            this.chkShowBottom.Size = new System.Drawing.Size(89, 17);
            this.chkShowBottom.TabIndex = 3;
            this.chkShowBottom.Text = "Show Bottom";
            this.chkShowBottom.UseVisualStyleBackColor = true;
            this.chkShowBottom.CheckedChanged += new System.EventHandler(this.chkShowBottom_CheckedChanged);
            // 
            // chkShowData
            // 
            this.chkShowData.AutoSize = true;
            this.chkShowData.Location = new System.Drawing.Point(27, 73);
            this.chkShowData.Name = "chkShowData";
            this.chkShowData.Size = new System.Drawing.Size(79, 17);
            this.chkShowData.TabIndex = 2;
            this.chkShowData.Text = "Show Data";
            this.chkShowData.UseVisualStyleBackColor = true;
            this.chkShowData.CheckedChanged += new System.EventHandler(this.chkShowData_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(664, 488);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.ChildChanged += new System.EventHandler<System.Windows.Forms.Integration.ChildChangedEventArgs>(this.elementHost1_ChildChanged);
            this.elementHost1.Child = this.graphControl1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 488);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "3D HeatMap";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private     GraphControl graphControl1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox chkShowLabelOnMouseEnter;
        private System.Windows.Forms.CheckBox chkShowUnit;
        private System.Windows.Forms.CheckBox chkShowBottom;
        private System.Windows.Forms.CheckBox chkShowData;
        private System.Windows.Forms.TextBox txtUnit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtMin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMax;
        private System.Windows.Forms.CheckBox chkShowFixedColorScale;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnShow;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}


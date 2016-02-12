<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class XPPanel
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose( disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim Checkbox2 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.Checkbox = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.Checkbox()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(XPPanel))
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Checkbox1 = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Checkbox()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Panel1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.Checkbox1)
        Me.Panel1.Controls.Add(Me.PictureBox1)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(245, 36)
        Me.Panel1.TabIndex = 0
        '
        'Checkbox1
        '
        Me.Checkbox1.BackColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(48, Byte), Integer))
        Me.Checkbox1.Checked = False
        Me.Checkbox1.Integral = False
        Me.Checkbox1.LabelText = ""
        Me.Checkbox1.Location = New System.Drawing.Point(203, 5)
        Me.Checkbox1.Name = "Checkbox1"
        Me.Checkbox1.RatioMode = False
        Me.Checkbox1.Size = New System.Drawing.Size(21, 18)
        Me.Checkbox1.TabIndex = 2
        Checkbox2.AutoSize = True
        Checkbox2.BackgroundColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(48, Byte), Integer))
        Checkbox2.Check = CType(resources.GetObject("Checkbox2.Check"), System.Drawing.Image)
        Checkbox2.CheckboxMargin = New System.Drawing.Rectangle(2, 1, 0, 0)
        Checkbox2.CheckPreLight = CType(resources.GetObject("Checkbox2.CheckPreLight"), System.Drawing.Image)
        Checkbox2.Disable = Nothing
        Checkbox2.ForeColor = System.Drawing.Color.Gray
        Checkbox2.LabelMargin = 5
        Checkbox2.PrelightColor = System.Drawing.Color.White
        Checkbox2.UnCheck = CType(resources.GetObject("Checkbox2.UnCheck"), System.Drawing.Image)
        Checkbox2.UncheckPreLight = CType(resources.GetObject("Checkbox2.UncheckPreLight"), System.Drawing.Image)
        Me.Checkbox1.UI = Checkbox2
        '
        'PictureBox1
        '
        Me.PictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.PictureBox1.Location = New System.Drawing.Point(10, 2)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(32, 32)
        Me.PictureBox1.TabIndex = 1
        Me.PictureBox1.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft YaHei", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(50, 8)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(53, 19)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Label1"
        '
        'Panel2
        '
        Me.Panel2.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel2.BackColor = System.Drawing.Color.FromArgb(CType(CType(239, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Panel2.Location = New System.Drawing.Point(1, 60)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(242, 115)
        Me.Panel2.TabIndex = 1
        '
        'XPPanel
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(88, Byte), Integer), CType(CType(200, Byte), Integer))
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "XPPanel"
        Me.Size = New System.Drawing.Size(245, 176)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Panel1 As Panel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Checkbox1 As Windows.Forms.Controls.Checkbox
End Class

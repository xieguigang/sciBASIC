<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form6
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
        Me.XpPanel2 = New Microsoft.VisualBasic.MolkPlusTheme.XPPanel()
        Me.CheckedListBox1 = New System.Windows.Forms.CheckedListBox()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.XpPanel3 = New Microsoft.VisualBasic.MolkPlusTheme.XPPanel()
        Me.PagerNavigator1 = New Microsoft.VisualBasic.MolkPlusTheme.PagerNavigator()
        Me.JumpNavigator1 = New Microsoft.VisualBasic.MolkPlusTheme.JumpNavigator()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'XpPanel2
        '
        Me.XpPanel2.BackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(88, Byte), Integer), CType(CType(200, Byte), Integer))
        Me.XpPanel2.Control = Nothing
        Me.XpPanel2.Expanded = False
        Me.XpPanel2.Icon = Nothing
        Me.XpPanel2.LabelFont = New System.Drawing.Font("Microsoft YaHei", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XpPanel2.LabelForeColor = System.Drawing.SystemColors.ControlText
        Me.XpPanel2.LabelText = "Label1"
        Me.XpPanel2.Location = New System.Drawing.Point(3, 3)
        Me.XpPanel2.Name = "XpPanel2"
        Me.XpPanel2.Size = New System.Drawing.Size(308, 176)
        Me.XpPanel2.Speed = 25
        Me.XpPanel2.TabIndex = 0
        '
        'CheckedListBox1
        '
        Me.CheckedListBox1.FormattingEnabled = True
        Me.CheckedListBox1.Items.AddRange(New Object() {"123", "1", "asfasf", "qw", "deqw", "eq", "we", "qw", "d", "as", "d", "as", "das", "dfasfasfasfasf"})
        Me.CheckedListBox1.Location = New System.Drawing.Point(499, 92)
        Me.CheckedListBox1.Name = "CheckedListBox1"
        Me.CheckedListBox1.Size = New System.Drawing.Size(153, 244)
        Me.CheckedListBox1.TabIndex = 1
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.Controls.Add(Me.XpPanel2)
        Me.FlowLayoutPanel1.Controls.Add(Me.XpPanel3)
        Me.FlowLayoutPanel1.Location = New System.Drawing.Point(23, 12)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        Me.FlowLayoutPanel1.Size = New System.Drawing.Size(354, 645)
        Me.FlowLayoutPanel1.TabIndex = 2
        '
        'XpPanel3
        '
        Me.XpPanel3.BackColor = System.Drawing.Color.FromArgb(CType(CType(32, Byte), Integer), CType(CType(88, Byte), Integer), CType(CType(200, Byte), Integer))
        Me.XpPanel3.Control = Nothing
        Me.XpPanel3.Expanded = False
        Me.XpPanel3.Icon = Nothing
        Me.XpPanel3.LabelFont = New System.Drawing.Font("Microsoft YaHei", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.XpPanel3.LabelForeColor = System.Drawing.SystemColors.ControlText
        Me.XpPanel3.LabelText = "Label1"
        Me.XpPanel3.Location = New System.Drawing.Point(3, 185)
        Me.XpPanel3.Name = "XpPanel3"
        Me.XpPanel3.Size = New System.Drawing.Size(308, 176)
        Me.XpPanel3.Speed = 25
        Me.XpPanel3.TabIndex = 1
        '
        'PagerNavigator1
        '
        Me.PagerNavigator1.DataPager = Nothing
        Me.PagerNavigator1.Location = New System.Drawing.Point(423, 416)
        Me.PagerNavigator1.Name = "PagerNavigator1"
        Me.PagerNavigator1.Size = New System.Drawing.Size(864, 212)
        Me.PagerNavigator1.TabIndex = 3
        '
        'JumpNavigator1
        '
        Me.JumpNavigator1.IndexList = Nothing
        Me.JumpNavigator1.IndexSize = New System.Drawing.Size(0, 0)
        Me.JumpNavigator1.Location = New System.Drawing.Point(1113, 117)
        Me.JumpNavigator1.Name = "JumpNavigator1"
        Me.JumpNavigator1.Size = New System.Drawing.Size(150, 476)
        Me.JumpNavigator1.TabIndex = 4
        '
        'Form6
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1380, 695)
        Me.Controls.Add(Me.JumpNavigator1)
        Me.Controls.Add(Me.PagerNavigator1)
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Controls.Add(Me.CheckedListBox1)
        Me.Name = "Form6"
        Me.Text = "Form6"
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents XpPanel1 As MolkPlusTheme.XPPanel
    Friend WithEvents XpPanel2 As MolkPlusTheme.XPPanel
    Friend WithEvents CheckedListBox1 As CheckedListBox
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents XpPanel3 As MolkPlusTheme.XPPanel
    Friend WithEvents PagerNavigator1 As MolkPlusTheme.PagerNavigator
    Friend WithEvents JumpNavigator1 As MolkPlusTheme.JumpNavigator
    ' Friend WithEvents JumpNavigator1 As MolkPlusTheme.JumpNavigator
End Class

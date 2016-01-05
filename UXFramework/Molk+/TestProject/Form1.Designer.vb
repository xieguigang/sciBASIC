<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.TabControl1 = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabLabel.TabControl()
        Me.MultipleTabpagePanel1 = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.TransparentLinkLable1 = New Microsoft.VisualBasic.MolkPlusTheme.TransparentLinkLable()
        Me.FormHooksModernBlack1 = New Microsoft.VisualBasic.MolkPlusTheme.FormHooksModernBlack()
        Me.ShadowPanel1 = New Microsoft.VisualBasic.MolkPlusTheme.ShadowPanel()
        Me.FormHooksModernBlack1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabControl1
        '
        Me.TabControl1.Location = New System.Drawing.Point(212, 358)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.Size = New System.Drawing.Size(761, 514)
        Me.TabControl1.TabIndex = 1
        Me.TabControl1.TabLabelWidth = 0
        '
        'MultipleTabpagePanel1
        '
        Me.MultipleTabpagePanel1.DisabledCloseControl = True
        Me.MultipleTabpagePanel1.EnableMenu = False
        Me.MultipleTabpagePanel1.Location = New System.Drawing.Point(55, 162)
        Me.MultipleTabpagePanel1.Name = "MultipleTabpagePanel1"
        Me.MultipleTabpagePanel1.Size = New System.Drawing.Size(994, 385)
        Me.MultipleTabpagePanel1.SizeMode = Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel.TabpageSizeModes.UIResourceAutoSize
        Me.MultipleTabpagePanel1.TabIndex = 0
        Me.MultipleTabpagePanel1.UIResource = Nothing
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(956, 586)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 25)
        Me.Button1.TabIndex = 2
        Me.Button1.Text = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'TransparentLinkLable1
        '
        Me.TransparentLinkLable1.BackgroundImage = CType(resources.GetObject("TransparentLinkLable1.BackgroundImage"), System.Drawing.Image)
        Me.TransparentLinkLable1.LabelText = "测试标签"
        Me.TransparentLinkLable1.Location = New System.Drawing.Point(1047, 689)
        Me.TransparentLinkLable1.Name = "TransparentLinkLable1"
        Me.TransparentLinkLable1.Size = New System.Drawing.Size(54, 14)
        Me.TransparentLinkLable1.TabIndex = 3
        '
        'FormHooksModernBlack1
        '
        Me.FormHooksModernBlack1.Controls.Add(Me.TabControl1)
        Me.FormHooksModernBlack1.Controls.Add(Me.ShadowPanel1)
        Me.FormHooksModernBlack1.Controls.Add(Me.MultipleTabpagePanel1)
        Me.FormHooksModernBlack1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.FormHooksModernBlack1.FixedSingle = False
        Me.FormHooksModernBlack1.Location = New System.Drawing.Point(0, 0)
        Me.FormHooksModernBlack1.Name = "FormHooksModernBlack1"
        Me.FormHooksModernBlack1.Size = New System.Drawing.Size(1155, 753)
        Me.FormHooksModernBlack1.Stretch = False
        Me.FormHooksModernBlack1.TabIndex = 4
        '
        'ShadowPanel1
        '
        'Me.ShadowPanel1.BorderColor = System.Drawing.Color.LightGray
        Me.ShadowPanel1.Location = New System.Drawing.Point(440, 44)
        Me.ShadowPanel1.Name = "ShadowPanel1"
        Me.ShadowPanel1.PanelColor = System.Drawing.Color.Empty
        Me.ShadowPanel1.Size = New System.Drawing.Size(200, 100)
        Me.ShadowPanel1.TabIndex = 2
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1155, 753)
        Me.Controls.Add(Me.TransparentLinkLable1)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.FormHooksModernBlack1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "Form1"
        Me.Opacity = 0.97999999999999976R
        Me.Text = "Form1"
        Me.FormHooksModernBlack1.ResumeLayout(False)
        Me.FormHooksModernBlack1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents MultipleTabpagePanel1 As Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel
    Friend WithEvents TabControl1 As Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabLabel.TabControl
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents TransparentLinkLable1 As Microsoft.VisualBasic.MolkPlusTheme.TransparentLinkLable
    Friend WithEvents FormHooksModernBlack1 As Microsoft.VisualBasic.MolkPlusTheme.FormHooksModernBlack
    Friend WithEvents ShadowPanel1 As Microsoft.VisualBasic.MolkPlusTheme.ShadowPanel
    '  Friend WithEvents BusyIndicator1 As Microsoft.VisualBasic.MolkPlusTheme.BusyIndicator
  
End Class

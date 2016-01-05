<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form4
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form4))
        Me.ProcessingBar1 = New Microsoft.VisualBasic.MolkPlusTheme.ProcessingBar()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.MultipleTabpagePanel1 = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel()
        Me.SuspendLayout()
        '
        'ProcessingBar1
        '
        Me.ProcessingBar1.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.ProcessingBar1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.ProcessingBar1.Location = New System.Drawing.Point(12, 12)
        Me.ProcessingBar1.Name = "ProcessingBar1"
        Me.ProcessingBar1.PercentageValue = 0
        Me.ProcessingBar1.Render = CType(resources.GetObject("ProcessingBar1.Render"), System.Drawing.Image)
        Me.ProcessingBar1.Size = New System.Drawing.Size(650, 16)
        Me.ProcessingBar1.TabIndex = 0
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        Me.Timer1.Interval = 1000
        '
        'MultipleTabpagePanel1
        '
        Me.MultipleTabpagePanel1.DisabledCloseControl = False
        Me.MultipleTabpagePanel1.EnableMenu = True
        Me.MultipleTabpagePanel1.Location = New System.Drawing.Point(34, 57)
        Me.MultipleTabpagePanel1.Name = "MultipleTabpagePanel1"
        Me.MultipleTabpagePanel1.PageInterval = 0
        Me.MultipleTabpagePanel1.Renderer = Nothing
        Me.MultipleTabpagePanel1.Size = New System.Drawing.Size(592, 347)
        Me.MultipleTabpagePanel1.SizeMode = Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel.TabpageSizeModes.UIResourceAutoSize
        Me.MultipleTabpagePanel1.TabIndex = 1
        Me.MultipleTabpagePanel1.UIResource = Nothing
        '
        'Form4
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(674, 436)
        Me.Controls.Add(Me.MultipleTabpagePanel1)
        Me.Controls.Add(Me.ProcessingBar1)
        Me.Name = "Form4"
        Me.Text = "Form4"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ProcessingBar1 As MolkPlusTheme.ProcessingBar
    Friend WithEvents Timer1 As Timer
    Friend WithEvents MultipleTabpagePanel1 As MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel
End Class

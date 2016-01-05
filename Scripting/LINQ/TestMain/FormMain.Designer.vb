<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormMain
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
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

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意:  以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormMain))
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RegistryExternalModuleToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExampleToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.QueryXMLToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SequencePatternSearchToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.StringCollectionToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuStrip1.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TextBox1
        '
        Me.TextBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TextBox1.Location = New System.Drawing.Point(0, 49)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TextBox1.Size = New System.Drawing.Size(710, 438)
        Me.TextBox1.TabIndex = 0
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.ExampleToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.MenuStrip1.Size = New System.Drawing.Size(710, 24)
        Me.MenuStrip1.TabIndex = 1
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.RegistryExternalModuleToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(36, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'RegistryExternalModuleToolStripMenuItem
        '
        Me.RegistryExternalModuleToolStripMenuItem.Name = "RegistryExternalModuleToolStripMenuItem"
        Me.RegistryExternalModuleToolStripMenuItem.Size = New System.Drawing.Size(191, 22)
        Me.RegistryExternalModuleToolStripMenuItem.Text = "Registry External Module"
        '
        'ExampleToolStripMenuItem
        '
        Me.ExampleToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.QueryXMLToolStripMenuItem, Me.SequencePatternSearchToolStripMenuItem, Me.StringCollectionToolStripMenuItem})
        Me.ExampleToolStripMenuItem.Name = "ExampleToolStripMenuItem"
        Me.ExampleToolStripMenuItem.Size = New System.Drawing.Size(59, 20)
        Me.ExampleToolStripMenuItem.Text = "Example"
        '
        'QueryXMLToolStripMenuItem
        '
        Me.QueryXMLToolStripMenuItem.Name = "QueryXMLToolStripMenuItem"
        Me.QueryXMLToolStripMenuItem.Size = New System.Drawing.Size(191, 22)
        Me.QueryXMLToolStripMenuItem.Text = "Query XML"
        '
        'SequencePatternSearchToolStripMenuItem
        '
        Me.SequencePatternSearchToolStripMenuItem.Name = "SequencePatternSearchToolStripMenuItem"
        Me.SequencePatternSearchToolStripMenuItem.Size = New System.Drawing.Size(191, 22)
        Me.SequencePatternSearchToolStripMenuItem.Text = "Sequence Pattern Search"
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButton1})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 24)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.ToolStrip1.Size = New System.Drawing.Size(710, 25)
        Me.ToolStrip1.TabIndex = 2
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'ToolStripButton1
        '
        Me.ToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton1.Image = CType(resources.GetObject("ToolStripButton1.Image"), System.Drawing.Image)
        Me.ToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton1.Name = "ToolStripButton1"
        Me.ToolStripButton1.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton1.Text = "Execute"
        '
        'StringCollectionToolStripMenuItem
        '
        Me.StringCollectionToolStripMenuItem.Name = "StringCollectionToolStripMenuItem"
        Me.StringCollectionToolStripMenuItem.Size = New System.Drawing.Size(191, 22)
        Me.StringCollectionToolStripMenuItem.Text = "String Collection"
        '
        'FormMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(710, 487)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "FormMain"
        Me.Text = "Form1"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RegistryExternalModuleToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ExampleToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents QueryXMLToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SequencePatternSearchToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents StringCollectionToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class

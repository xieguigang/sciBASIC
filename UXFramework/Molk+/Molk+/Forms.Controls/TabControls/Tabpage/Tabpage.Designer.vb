Namespace Windows.Forms.Controls.TabControl.TabSwitch

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class TabPage : Inherits Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.ITabPage

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer


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

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(TabPage))
            Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
            Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.SaveCurrentDocument = New System.Windows.Forms.ToolStripMenuItem()
            Me.CloseTabPageCommand = New System.Windows.Forms.ToolStripMenuItem()
            Me.CloseAllTabpage = New System.Windows.Forms.ToolStripMenuItem()
            Me.CloseAllButThisTabpage = New System.Windows.Forms.ToolStripMenuItem()
            Me.ContextMenuStrip1.SuspendLayout()
            Me.SuspendLayout()
            '
            'ContextMenuStrip1
            '
            Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SaveCurrentDocument, Me.CloseTabPageCommand, Me.CloseAllTabpage, Me.CloseAllButThisTabpage})
            Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
            Me.ContextMenuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional
            Me.ContextMenuStrip1.Size = New System.Drawing.Size(185, 92)
            '
            'SaveCurrentDocument
            '
            Me.SaveCurrentDocument.BackColor = System.Drawing.Color.FromArgb(CType(CType(214, Byte), Integer), CType(CType(219, Byte), Integer), CType(CType(233, Byte), Integer))
            Me.SaveCurrentDocument.Image = CType(resources.GetObject("SaveCurrentDocument.Image"), System.Drawing.Image)
            Me.SaveCurrentDocument.Name = "SaveCurrentDocument"
            Me.SaveCurrentDocument.Size = New System.Drawing.Size(184, 22)
            Me.SaveCurrentDocument.Text = "Save Document"
            '
            'CloseTabPageCommand
            '
            Me.CloseTabPageCommand.BackColor = System.Drawing.Color.FromArgb(CType(CType(214, Byte), Integer), CType(CType(219, Byte), Integer), CType(CType(233, Byte), Integer))
            Me.CloseTabPageCommand.Image = CType(resources.GetObject("CloseTabPageCommand.Image"), System.Drawing.Image)
            Me.CloseTabPageCommand.Name = "CloseTabPageCommand"
            Me.CloseTabPageCommand.Size = New System.Drawing.Size(184, 22)
            Me.CloseTabPageCommand.Text = "Close"
            '
            'CloseAllTabpage
            '
            Me.CloseAllTabpage.BackColor = System.Drawing.Color.FromArgb(CType(CType(214, Byte), Integer), CType(CType(219, Byte), Integer), CType(CType(233, Byte), Integer))
            Me.CloseAllTabpage.Name = "CloseAllTabpage"
            Me.CloseAllTabpage.Size = New System.Drawing.Size(184, 22)
            Me.CloseAllTabpage.Text = "Close All Documents"
            '
            'CloseAllButThisTabpage
            '
            Me.CloseAllButThisTabpage.BackColor = System.Drawing.Color.FromArgb(CType(CType(214, Byte), Integer), CType(CType(219, Byte), Integer), CType(CType(233, Byte), Integer))
            Me.CloseAllButThisTabpage.Name = "CloseAllButThisTabpage"
            Me.CloseAllButThisTabpage.Size = New System.Drawing.Size(184, 22)
            Me.CloseAllButThisTabpage.Text = "Close All But This"
            '
            'TabPage
            '
            Me.ContextMenuStrip1.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
        Friend WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
        Friend WithEvents CloseTabPageCommand As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents CloseAllButThisTabpage As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents SaveCurrentDocument As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents CloseAllTabpage As System.Windows.Forms.ToolStripMenuItem
    End Class
End Namespace
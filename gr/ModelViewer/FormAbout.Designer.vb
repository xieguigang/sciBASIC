<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormAbout
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormAbout))
        RenderPanel1 = New RenderPanel()
        SuspendLayout()
        ' 
        ' RenderPanel1
        ' 
        RenderPanel1.Dock = DockStyle.Bottom
        RenderPanel1.Location = New Point(0, 83)
        RenderPanel1.Name = "RenderPanel1"
        RenderPanel1.Size = New Size(662, 531)
        RenderPanel1.TabIndex = 0
        ' 
        ' FormAbout
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(662, 614)
        Controls.Add(RenderPanel1)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "FormAbout"
        StartPosition = FormStartPosition.CenterParent
        Text = "关于"
        ResumeLayout(False)
    End Sub

    Friend WithEvents RenderPanel1 As RenderPanel
End Class

Namespace Windows.Forms.Controls.TabControl.TabPage

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class MultipleTabpagePanel
        Inherits Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.ITabControl(Of Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.TabPage)

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
            Me.Separable = New System.Windows.Forms.PictureBox()
            CType(Me.Separable, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'Separable
            '
            Me.Separable.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
            Me.Separable.Location = New System.Drawing.Point(135, 174)
            Me.Separable.Name = "Separable"
            Me.Separable.Size = New System.Drawing.Size(100, 50)
            Me.Separable.TabIndex = 0
            Me.Separable.TabStop = False
            '
            'MultipleTabpagePanel
            '
            Me.Controls.Add(Me.Separable)
            Me.DoubleBuffered = True
            Me.Name = "MultipleTabpagePanel"
            Me.Size = New System.Drawing.Size(592, 523)
            CType(Me.Separable, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub

        Friend WithEvents Separable As System.Windows.Forms.PictureBox

    End Class
End Namespace
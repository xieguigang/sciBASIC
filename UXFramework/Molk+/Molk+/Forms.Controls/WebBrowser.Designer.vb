Imports System.ComponentModel

Namespace Windows.Forms.Controls

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial MustInherit Class WebBrowser
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
            Me.WebBrowser1 = New System.Windows.Forms.WebBrowser()
            Me.TextBox1 = New System.Windows.Forms.TextBox()
            Me.SuspendLayout()
            '
            'WebBrowser1
            '
            Me.WebBrowser1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.WebBrowser1.Location = New System.Drawing.Point(0, 21)
            Me.WebBrowser1.MinimumSize = New System.Drawing.Size(20, 20)
            Me.WebBrowser1.Name = "WebBrowser1"
            Me.WebBrowser1.Size = New System.Drawing.Size(451, 327)
            Me.WebBrowser1.TabIndex = 0
            '
            'TextBox1
            '
            Me.TextBox1.Dock = System.Windows.Forms.DockStyle.Top
            Me.TextBox1.Location = New System.Drawing.Point(0, 0)
            Me.TextBox1.Name = "TextBox1"
            Me.TextBox1.Size = New System.Drawing.Size(451, 21)
            Me.TextBox1.TabIndex = 1
            '
            'Browser
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.WebBrowser1)
            Me.Controls.Add(Me.TextBox1)
            Me.Name = "Browser"
            Me.Size = New System.Drawing.Size(451, 348)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Protected Friend WithEvents WebBrowser1 As System.Windows.Forms.WebBrowser
        Friend WithEvents TextBox1 As System.Windows.Forms.TextBox

    End Class
End Namespace
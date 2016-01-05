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
        Me.MessageBubble1 = New WindowsApplication1.MessageBubble()
        Me.HtmlUserControl1 = New Microsoft.VisualBasic.MolkPlusTheme.HtmlUserControl()
        Me.SuspendLayout()
        '
        'MessageBubble1
        '
        Me.MessageBubble1.Location = New System.Drawing.Point(28, 38)
        Me.MessageBubble1.Name = "MessageBubble1"
        Me.MessageBubble1.Size = New System.Drawing.Size(393, 139)
        Me.MessageBubble1.TabIndex = 0
        '
        'HtmlUserControl1
        '
        Me.HtmlUserControl1.Control = Nothing
        Me.HtmlUserControl1.Location = New System.Drawing.Point(185, 186)
        Me.HtmlUserControl1.Name = "HtmlUserControl1"
        Me.HtmlUserControl1.Size = New System.Drawing.Size(593, 349)
        Me.HtmlUserControl1.TabIndex = 1
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(891, 547)
        Me.Controls.Add(Me.HtmlUserControl1)
        Me.Controls.Add(Me.MessageBubble1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents MessageBubble1 As MessageBubble
    Friend WithEvents HtmlUserControl1 As MolkPlusTheme.HtmlUserControl
End Class

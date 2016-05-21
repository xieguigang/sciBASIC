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
        Me.tbInputs = New System.Windows.Forms.TextBox()
        Me.tbRegex = New System.Windows.Forms.TextBox()
        Me.lbResults = New System.Windows.Forms.ListBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'tbInputs
        '
        Me.tbInputs.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbInputs.Location = New System.Drawing.Point(12, 12)
        Me.tbInputs.Multiline = True
        Me.tbInputs.Name = "tbInputs"
        Me.tbInputs.Size = New System.Drawing.Size(1069, 308)
        Me.tbInputs.TabIndex = 0
        '
        'tbRegex
        '
        Me.tbRegex.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbRegex.Location = New System.Drawing.Point(12, 326)
        Me.tbRegex.Multiline = True
        Me.tbRegex.Name = "tbRegex"
        Me.tbRegex.Size = New System.Drawing.Size(464, 237)
        Me.tbRegex.TabIndex = 1
        '
        'lbResults
        '
        Me.lbResults.FormattingEnabled = True
        Me.lbResults.Location = New System.Drawing.Point(482, 326)
        Me.lbResults.Name = "lbResults"
        Me.lbResults.Size = New System.Drawing.Size(498, 238)
        Me.lbResults.TabIndex = 2
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(986, 326)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(95, 23)
        Me.Button1.TabIndex = 3
        Me.Button1.Text = "Test"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1093, 575)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.lbResults)
        Me.Controls.Add(Me.tbRegex)
        Me.Controls.Add(Me.tbInputs)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents tbInputs As TextBox
    Friend WithEvents tbRegex As TextBox
    Friend WithEvents lbResults As ListBox
    Friend WithEvents Button1 As Button
End Class

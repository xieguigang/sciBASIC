<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form5
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
        Me.ClosableLabel1 = New Microsoft.VisualBasic.MolkPlusTheme.ClosableLabel()
        Me.ClosableLabel2 = New Microsoft.VisualBasic.MolkPlusTheme.ClosableLabel()
        Me.UserPicker1 = New Microsoft.VisualBasic.MolkPlusTheme.UserPicker()
        Me.SuspendLayout()
        '
        'ClosableLabel1
        '
        Me.ClosableLabel1.Location = New System.Drawing.Point(179, 286)
        Me.ClosableLabel1.Name = "ClosableLabel1"
        Me.ClosableLabel1.Size = New System.Drawing.Size(168, 27)
        Me.ClosableLabel1.TabIndex = 1
        '
        'ClosableLabel2
        '
        Me.ClosableLabel2.Location = New System.Drawing.Point(150, 275)
        Me.ClosableLabel2.Name = "ClosableLabel2"
        Me.ClosableLabel2.Size = New System.Drawing.Size(168, 27)
        Me.ClosableLabel2.TabIndex = 0
        '
        'UserPicker1
        '
        Me.UserPicker1.BackColor = System.Drawing.Color.White
        Me.UserPicker1.DockSide = Microsoft.VisualBasic.MolkPlusTheme.Unity3.Controls.DropDownControl.DockSides.Left
        Me.UserPicker1.Location = New System.Drawing.Point(213, 52)
        Me.UserPicker1.Name = "UserPicker1"
        Me.UserPicker1.SelectedIndex = 0
        Me.UserPicker1.SelectedItem = Nothing
        Me.UserPicker1.Size = New System.Drawing.Size(440, 26)
        Me.UserPicker1.TabIndex = 1
        '
        'Form5
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(665, 356)
        Me.Controls.Add(Me.UserPicker1)
        Me.Controls.Add(Me.ClosableLabel2)
        Me.Name = "Form5"
        Me.Text = "Form5"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents EmployeePicker1 As MolkPlusTheme.UserPicker
    Friend WithEvents ClosableLabel1 As MolkPlusTheme.ClosableLabel
    Friend WithEvents ClosableLabel2 As MolkPlusTheme.ClosableLabel
    Friend WithEvents UserPicker1 As MolkPlusTheme.UserPicker
End Class

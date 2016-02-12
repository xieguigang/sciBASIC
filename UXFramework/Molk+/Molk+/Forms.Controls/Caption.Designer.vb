Namespace Windows.Forms.Controls

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class Caption
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
            Me.components = New System.ComponentModel.Container()
            Me.InformationArea = New System.Windows.Forms.PictureBox()
            Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
            Me.Close = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button()
            Me.Maximize = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button()
            Me.Minimize = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button()
            Me.Controlbox = New System.Windows.Forms.Panel()
            CType(Me.InformationArea, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.Controlbox.SuspendLayout()
            Me.SuspendLayout()
            '
            'InformationArea
            '
            Me.InformationArea.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
            Me.InformationArea.Location = New System.Drawing.Point(31, 14)
            Me.InformationArea.Name = "InformationArea"
            Me.InformationArea.Size = New System.Drawing.Size(100, 54)
            Me.InformationArea.TabIndex = 0
            Me.InformationArea.TabStop = False
            '
            'Close
            '
            Me.Close.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.Close.BackColor = System.Drawing.Color.Red
            Me.Close.Font = New System.Drawing.Font("SimSun", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
            Me.Close.ForeColor = System.Drawing.SystemColors.ControlText
            Me.Close.Location = New System.Drawing.Point(165, 3)
            Me.Close.MyFont = New System.Drawing.Font("SimSun", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
            Me.Close.MyFontColor = System.Drawing.SystemColors.ControlText
            Me.Close.MyText = ""
            Me.Close.Name = "Close"
            Me.Close.Render = Nothing
            Me.Close.Size = New System.Drawing.Size(32, 36)
            Me.Close.TabIndex = 3
            Me.Close.TabStop = False
            Me.ToolTip1.SetToolTip(Me.Close, "Close")
            Me.Close.UI = Nothing
            '
            'Maximize
            '
            Me.Maximize.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.Maximize.BackColor = System.Drawing.Color.Cyan
            Me.Maximize.Font = New System.Drawing.Font("SimSun", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
            Me.Maximize.ForeColor = System.Drawing.SystemColors.ControlText
            Me.Maximize.Location = New System.Drawing.Point(127, 3)
            Me.Maximize.MyFont = New System.Drawing.Font("SimSun", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
            Me.Maximize.MyFontColor = System.Drawing.SystemColors.ControlText
            Me.Maximize.MyText = ""
            Me.Maximize.Name = "Maximize"
            Me.Maximize.Render = Nothing
            Me.Maximize.Size = New System.Drawing.Size(32, 36)
            Me.Maximize.TabIndex = 2
            Me.Maximize.TabStop = False
            Me.ToolTip1.SetToolTip(Me.Maximize, "Maximum")
            Me.Maximize.UI = Nothing
            '
            'Minimize
            '
            Me.Minimize.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.Minimize.BackColor = System.Drawing.Color.Lime
            Me.Minimize.Font = New System.Drawing.Font("SimSun", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
            Me.Minimize.ForeColor = System.Drawing.SystemColors.ControlText
            Me.Minimize.Location = New System.Drawing.Point(89, 3)
            Me.Minimize.MyFont = New System.Drawing.Font("SimSun", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
            Me.Minimize.MyFontColor = System.Drawing.SystemColors.ControlText
            Me.Minimize.MyText = ""
            Me.Minimize.Name = "Minimize"
            Me.Minimize.Render = Nothing
            Me.Minimize.Size = New System.Drawing.Size(32, 36)
            Me.Minimize.TabIndex = 1
            Me.Minimize.TabStop = False
            Me.ToolTip1.SetToolTip(Me.Minimize, "Minimize")
            Me.Minimize.UI = Nothing
            '
            'Controlbox
            '
            Me.Controlbox.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.Controlbox.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
            Me.Controlbox.Controls.Add(Me.Close)
            Me.Controlbox.Controls.Add(Me.Minimize)
            Me.Controlbox.Controls.Add(Me.Maximize)
            Me.Controlbox.Location = New System.Drawing.Point(342, 0)
            Me.Controlbox.Name = "Controlbox"
            Me.Controlbox.Size = New System.Drawing.Size(200, 65)
            Me.Controlbox.TabIndex = 4
            '
            'Caption
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.Controlbox)
            Me.Controls.Add(Me.InformationArea)
            Me.Name = "Caption"
            Me.Size = New System.Drawing.Size(545, 97)
            CType(Me.InformationArea, System.ComponentModel.ISupportInitialize).EndInit()
            Me.Controlbox.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents InformationArea As System.Windows.Forms.PictureBox
        Friend WithEvents Minimize As Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button
        Friend WithEvents Maximize As Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button
        Friend WithEvents Close As Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button
        Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
        Friend WithEvents Controlbox As Panel
    End Class
End Namespace
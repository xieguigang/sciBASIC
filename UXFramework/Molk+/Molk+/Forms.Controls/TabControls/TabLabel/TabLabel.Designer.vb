Namespace Windows.Forms.Controls.TabControl.TabLabel

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class TabLabel
        Inherits Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.ITabPage

        'UserControl 重写 Dispose，以清理组件列表。
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

        'Windows 窗体设计器所必需的
        Private components As System.ComponentModel.IContainer

        '注意:  以下过程是 Windows 窗体设计器所必需的
        '可以使用 Windows 窗体设计器修改它。  
        '不要使用代码编辑器修改它。
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.LabelBase = New System.Windows.Forms.PictureBox()
            Me.LabelHead = New System.Windows.Forms.PictureBox()
            CType(Me.LabelBase, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.LabelHead, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'LabelBase
            '
            Me.LabelBase.BackgroundImage = Global.Microsoft.VisualBasic.MolkPlusTheme.My.Resources.Resources.LabelBase
            Me.LabelBase.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
            Me.LabelBase.Dock = System.Windows.Forms.DockStyle.Fill
            Me.LabelBase.Location = New System.Drawing.Point(0, 0)
            Me.LabelBase.Name = "LabelBase"
            Me.LabelBase.Size = New System.Drawing.Size(349, 24)
            Me.LabelBase.TabIndex = 0
            Me.LabelBase.TabStop = False
            '
            'LabelHead
            '
            Me.LabelHead.BackgroundImage = Global.Microsoft.VisualBasic.MolkPlusTheme.My.Resources.Resources.LabelHead
            Me.LabelHead.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
            Me.LabelHead.Dock = System.Windows.Forms.DockStyle.Right
            Me.LabelHead.Location = New System.Drawing.Point(349, 0)
            Me.LabelHead.Name = "LabelHead"
            Me.LabelHead.Size = New System.Drawing.Size(11, 24)
            Me.LabelHead.TabIndex = 1
            Me.LabelHead.TabStop = False
            '
            'TabLabel
            '
            Me.Controls.Add(Me.LabelBase)
            Me.Controls.Add(Me.LabelHead)
            Me.Size = New System.Drawing.Size(360, 24)
            CType(Me.LabelBase, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.LabelHead, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents LabelBase As System.Windows.Forms.PictureBox
        Friend WithEvents LabelHead As System.Windows.Forms.PictureBox

    End Class
End Namespace
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class PagerNavigator
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
        Dim Checkbox1 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.Checkbox = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.Checkbox()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PagerNavigator))
        Dim Checkbox2 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.Checkbox = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.Checkbox()
        Dim Checkbox3 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.Checkbox = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.Checkbox()
        Me.tbSummary = New System.Windows.Forms.TextBox()
        Me.btnFirstPage = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button()
        Me.btnPrevious = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button()
        Me.btnNext = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button()
        Me.btnLastPage = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.cbOne = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Checkbox()
        Me.cbTwo = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Checkbox()
        Me.cbThree = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Checkbox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'tbSummary
        '
        Me.tbSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.tbSummary.Font = New System.Drawing.Font("Microsoft YaHei", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbSummary.Location = New System.Drawing.Point(3, 7)
        Me.tbSummary.Name = "tbSummary"
        Me.tbSummary.ReadOnly = True
        Me.tbSummary.Size = New System.Drawing.Size(147, 23)
        Me.tbSummary.TabIndex = 0
        Me.tbSummary.Text = "共11900条数据，第1 / 3页"
        '
        'btnFirstPage
        '
        Me.btnFirstPage.BackColor = System.Drawing.Color.Red
        Me.btnFirstPage.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnFirstPage.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnFirstPage.Location = New System.Drawing.Point(160, 3)
        Me.btnFirstPage.MyFont = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnFirstPage.MyFontColor = System.Drawing.SystemColors.ControlText
        Me.btnFirstPage.MyText = "第一页"
        Me.btnFirstPage.Name = "btnFirstPage"
        Me.btnFirstPage.Render = Nothing
        Me.btnFirstPage.Size = New System.Drawing.Size(72, 29)
        Me.btnFirstPage.TabIndex = 1
        Me.btnFirstPage.UI = Nothing
        '
        'btnPrevious
        '
        Me.btnPrevious.BackColor = System.Drawing.Color.Red
        Me.btnPrevious.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnPrevious.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnPrevious.Location = New System.Drawing.Point(238, 3)
        Me.btnPrevious.MyFont = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnPrevious.MyFontColor = System.Drawing.SystemColors.ControlText
        Me.btnPrevious.MyText = "上一页"
        Me.btnPrevious.Name = "btnPrevious"
        Me.btnPrevious.Render = Nothing
        Me.btnPrevious.Size = New System.Drawing.Size(72, 29)
        Me.btnPrevious.TabIndex = 2
        Me.btnPrevious.UI = Nothing
        '
        'btnNext
        '
        Me.btnNext.BackColor = System.Drawing.Color.Red
        Me.btnNext.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnNext.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnNext.Location = New System.Drawing.Point(449, 3)
        Me.btnNext.MyFont = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnNext.MyFontColor = System.Drawing.SystemColors.ControlText
        Me.btnNext.MyText = "下一页"
        Me.btnNext.Name = "btnNext"
        Me.btnNext.Render = Nothing
        Me.btnNext.Size = New System.Drawing.Size(72, 29)
        Me.btnNext.TabIndex = 6
        Me.btnNext.UI = Nothing
        '
        'btnLastPage
        '
        Me.btnLastPage.BackColor = System.Drawing.Color.Red
        Me.btnLastPage.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnLastPage.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnLastPage.Location = New System.Drawing.Point(527, 3)
        Me.btnLastPage.MyFont = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnLastPage.MyFontColor = System.Drawing.SystemColors.ControlText
        Me.btnLastPage.MyText = "尾页"
        Me.btnLastPage.Name = "btnLastPage"
        Me.btnLastPage.Render = Nothing
        Me.btnLastPage.Size = New System.Drawing.Size(72, 29)
        Me.btnLastPage.TabIndex = 7
        Me.btnLastPage.UI = Nothing
        '
        'LinkLabel1
        '
        Me.LinkLabel1.AutoSize = True
        Me.LinkLabel1.Font = New System.Drawing.Font("Microsoft YaHei", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LinkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline
        Me.LinkLabel1.Location = New System.Drawing.Point(418, 7)
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.Size = New System.Drawing.Size(21, 19)
        Me.LinkLabel1.TabIndex = 8
        Me.LinkLabel1.TabStop = True
        Me.LinkLabel1.Text = "..."
        '
        'cbOne
        '
        Me.cbOne.BackColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(48, Byte), Integer))
        Me.cbOne.Checked = False
        Me.cbOne.Integral = False
        Me.cbOne.LabelText = "1"
        Me.cbOne.Location = New System.Drawing.Point(3, 3)
        Me.cbOne.Name = "cbOne"
        Me.cbOne.RatioMode = False
        Me.cbOne.Size = New System.Drawing.Size(34, 18)
        Me.cbOne.TabIndex = 9
        Checkbox1.AutoSize = True
        Checkbox1.BackgroundColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(48, Byte), Integer))
        Checkbox1.Check = CType(resources.GetObject("Checkbox1.Check"), System.Drawing.Image)
        Checkbox1.CheckboxMargin = New System.Drawing.Rectangle(2, 1, 0, 0)
        Checkbox1.CheckPreLight = CType(resources.GetObject("Checkbox1.CheckPreLight"), System.Drawing.Image)
        Checkbox1.Disable = Nothing
        Checkbox1.ForeColor = System.Drawing.Color.Gray
        Checkbox1.LabelMargin = 5
        Checkbox1.PrelightColor = System.Drawing.Color.White
        Checkbox1.UnCheck = CType(resources.GetObject("Checkbox1.UnCheck"), System.Drawing.Image)
        Checkbox1.UncheckPreLight = CType(resources.GetObject("Checkbox1.UncheckPreLight"), System.Drawing.Image)
        Me.cbOne.UI = Checkbox1
        '
        'cbTwo
        '
        Me.cbTwo.BackColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(48, Byte), Integer))
        Me.cbTwo.Checked = False
        Me.cbTwo.Integral = False
        Me.cbTwo.LabelText = "2"
        Me.cbTwo.Location = New System.Drawing.Point(33, 3)
        Me.cbTwo.Name = "cbTwo"
        Me.cbTwo.RatioMode = False
        Me.cbTwo.Size = New System.Drawing.Size(34, 18)
        Me.cbTwo.TabIndex = 10
        Checkbox2.AutoSize = True
        Checkbox2.BackgroundColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(48, Byte), Integer))
        Checkbox2.Check = CType(resources.GetObject("Checkbox2.Check"), System.Drawing.Image)
        Checkbox2.CheckboxMargin = New System.Drawing.Rectangle(2, 1, 0, 0)
        Checkbox2.CheckPreLight = CType(resources.GetObject("Checkbox2.CheckPreLight"), System.Drawing.Image)
        Checkbox2.Disable = Nothing
        Checkbox2.ForeColor = System.Drawing.Color.Gray
        Checkbox2.LabelMargin = 5
        Checkbox2.PrelightColor = System.Drawing.Color.White
        Checkbox2.UnCheck = CType(resources.GetObject("Checkbox2.UnCheck"), System.Drawing.Image)
        Checkbox2.UncheckPreLight = CType(resources.GetObject("Checkbox2.UncheckPreLight"), System.Drawing.Image)
        Me.cbTwo.UI = Checkbox2
        '
        'cbThree
        '
        Me.cbThree.BackColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(48, Byte), Integer))
        Me.cbThree.Checked = False
        Me.cbThree.Integral = False
        Me.cbThree.LabelText = "3"
        Me.cbThree.Location = New System.Drawing.Point(63, 3)
        Me.cbThree.Name = "cbThree"
        Me.cbThree.RatioMode = False
        Me.cbThree.Size = New System.Drawing.Size(34, 18)
        Me.cbThree.TabIndex = 11
        Checkbox3.AutoSize = True
        Checkbox3.BackgroundColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(32, Byte), Integer), CType(CType(48, Byte), Integer))
        Checkbox3.Check = CType(resources.GetObject("Checkbox3.Check"), System.Drawing.Image)
        Checkbox3.CheckboxMargin = New System.Drawing.Rectangle(2, 1, 0, 0)
        Checkbox3.CheckPreLight = CType(resources.GetObject("Checkbox3.CheckPreLight"), System.Drawing.Image)
        Checkbox3.Disable = Nothing
        Checkbox3.ForeColor = System.Drawing.Color.Gray
        Checkbox3.LabelMargin = 5
        Checkbox3.PrelightColor = System.Drawing.Color.White
        Checkbox3.UnCheck = CType(resources.GetObject("Checkbox3.UnCheck"), System.Drawing.Image)
        Checkbox3.UncheckPreLight = CType(resources.GetObject("Checkbox3.UncheckPreLight"), System.Drawing.Image)
        Me.cbThree.UI = Checkbox3
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.cbOne)
        Me.Panel1.Controls.Add(Me.cbThree)
        Me.Panel1.Controls.Add(Me.cbTwo)
        Me.Panel1.Location = New System.Drawing.Point(316, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(96, 42)
        Me.Panel1.TabIndex = 12
        '
        'PagerNavigator
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.LinkLabel1)
        Me.Controls.Add(Me.btnLastPage)
        Me.Controls.Add(Me.btnNext)
        Me.Controls.Add(Me.btnPrevious)
        Me.Controls.Add(Me.btnFirstPage)
        Me.Controls.Add(Me.tbSummary)
        Me.Name = "PagerNavigator"
        Me.Size = New System.Drawing.Size(608, 40)
        Me.Panel1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents tbSummary As TextBox
    Friend WithEvents btnFirstPage As Windows.Forms.Controls.Button
    Friend WithEvents btnPrevious As Windows.Forms.Controls.Button
    Friend WithEvents btnNext As Windows.Forms.Controls.Button
    Friend WithEvents btnLastPage As Windows.Forms.Controls.Button
    Friend WithEvents LinkLabel1 As LinkLabel
    Friend WithEvents cbOne As Windows.Forms.Controls.Checkbox
    Friend WithEvents cbTwo As Windows.Forms.Controls.Checkbox
    Friend WithEvents cbThree As Windows.Forms.Controls.Checkbox
    Friend WithEvents Panel1 As Panel
End Class

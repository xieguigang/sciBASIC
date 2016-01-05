<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormWin10

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormWin10))
        Dim CaptionResources1 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.CaptionResources = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.CaptionResources()
        Dim ImageResource1 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ImageResource = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ImageResource()
        Dim ButtonResource1 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource()
        Dim ImageResource2 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ImageResource = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ImageResource()
        Dim ImageResource3 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ImageResource = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ImageResource()
        Dim ButtonResource2 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource()
        Dim ButtonResource3 As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource = New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource()
        Me.Back = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button()
        Me.Caption1 = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Caption()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.UserAvatarMenu1 = New Microsoft.VisualBasic.UWP.UserAvatarMenu()
        Me.SuspendLayout()
        '
        'Back
        '
        Me.Back.BackColor = System.Drawing.Color.Silver
        Me.Back.BackgroundImage = Global.Microsoft.VisualBasic.UWP.My.Resources.Resources.BackNormal
        Me.Back.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.Back.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Back.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Back.Location = New System.Drawing.Point(1, 1)
        Me.Back.MyFont = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Back.MyFontColor = System.Drawing.SystemColors.ControlText
        Me.Back.MyText = ""
        Me.Back.Name = "Back"
        Me.Back.Render = Nothing
        Me.Back.Size = New System.Drawing.Size(48, 31)
        Me.Back.TabIndex = 0
        Me.ToolTip1.SetToolTip(Me.Back, "Back")
        Me.Back.UI = Nothing
        '
        'Caption1
        '
        Me.Caption1.AutoHandleFormCloseEvent = True
        Me.Caption1.AutoHandleFormMaximizeEvent = True
        Me.Caption1.AutoHandleFormMinimizeEvent = True
        Me.Caption1.BackColor = System.Drawing.Color.Black
        Me.Caption1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Caption1.Icon = CType(resources.GetObject("Caption1.Icon"), System.Drawing.Image)
        Me.Caption1.Location = New System.Drawing.Point(0, 0)
        Me.Caption1.Name = "Caption1"
        Me.Caption1.ShowCaptionText = True
        Me.Caption1.ShowIcon = True
        Me.Caption1.ShowMaximizeButton = True
        Me.Caption1.ShowMinimizedButton = True
        Me.Caption1.Size = New System.Drawing.Size(928, 43)
        Me.Caption1.SubCaption = Nothing
        Me.Caption1.TabIndex = 1
        Me.Caption1.TransparentToParentForm = False
        ImageResource1.BackColor = System.Drawing.Color.Black
        ImageResource1.BackgroundImage = Nothing
        CaptionResources1.BackgroundImage = ImageResource1
        CaptionResources1.BorderPen = Nothing
        ButtonResource1.Active = Nothing
        ButtonResource1.ActiveTextColor = System.Drawing.Color.Black
        ButtonResource1.BorderColor = Nothing
        ButtonResource1.Cursor = System.Windows.Forms.Cursors.Arrow
        ButtonResource1.DisableTextColor = System.Drawing.Color.DarkGray
        ButtonResource1.HighLightTextColor = System.Drawing.Color.Black
        ButtonResource1.InSensitive = Nothing
        ButtonResource1.Normal = CType(resources.GetObject("ButtonResource1.Normal"), System.Drawing.Image)
        ButtonResource1.NormalTextColor = System.Drawing.Color.White
        ButtonResource1.PreLight = Nothing
        ButtonResource1.TextAlign = Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource.TextAlignments.Left
        ButtonResource1.TextoffSets = New System.Drawing.Point(3, 2)
        CaptionResources1.Close = ButtonResource1
        CaptionResources1.CloseButtonTooltip = "Close Window"
        ImageResource2.BackColor = System.Drawing.Color.Black
        ImageResource2.BackgroundImage = Nothing
        CaptionResources1.ControlBox = ImageResource2
        CaptionResources1.ControlboxButtonSize = New System.Drawing.Size(30, 18)
        CaptionResources1.HeightLimits = 44
        CaptionResources1.Icon = CType(resources.GetObject("CaptionResources1.Icon"), System.Drawing.Image)
        ImageResource3.BackColor = System.Drawing.Color.Black
        ImageResource3.BackgroundImage = Nothing
        CaptionResources1.InformationArea = ImageResource3
        CaptionResources1.MaxButtonTooltip = "Maximize"
        ButtonResource2.Active = Nothing
        ButtonResource2.ActiveTextColor = System.Drawing.Color.Black
        ButtonResource2.BorderColor = Nothing
        ButtonResource2.Cursor = System.Windows.Forms.Cursors.Arrow
        ButtonResource2.DisableTextColor = System.Drawing.Color.DarkGray
        ButtonResource2.HighLightTextColor = System.Drawing.Color.Black
        ButtonResource2.InSensitive = Nothing
        ButtonResource2.Normal = CType(resources.GetObject("ButtonResource2.Normal"), System.Drawing.Image)
        ButtonResource2.NormalTextColor = System.Drawing.Color.White
        ButtonResource2.PreLight = Nothing
        ButtonResource2.TextAlign = Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource.TextAlignments.Left
        ButtonResource2.TextoffSets = New System.Drawing.Point(3, 2)
        CaptionResources1.Maximum = ButtonResource2
        CaptionResources1.MinButtonTooltip = "Minimize"
        ButtonResource3.Active = Nothing
        ButtonResource3.ActiveTextColor = System.Drawing.Color.Black
        ButtonResource3.BorderColor = Nothing
        ButtonResource3.Cursor = System.Windows.Forms.Cursors.Arrow
        ButtonResource3.DisableTextColor = System.Drawing.Color.DarkGray
        ButtonResource3.HighLightTextColor = System.Drawing.Color.Black
        ButtonResource3.InSensitive = Nothing
        ButtonResource3.Normal = CType(resources.GetObject("ButtonResource3.Normal"), System.Drawing.Image)
        ButtonResource3.NormalTextColor = System.Drawing.Color.White
        ButtonResource3.PreLight = Nothing
        ButtonResource3.TextAlign = Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource.TextAlignments.Left
        ButtonResource3.TextoffSets = New System.Drawing.Point(3, 2)
        CaptionResources1.Minimize = ButtonResource3
        CaptionResources1.ShowIcon = True
        CaptionResources1.ShowText = True
        CaptionResources1.SubCaptionFont = New System.Drawing.Font("Microsoft YaHei", 10.0!)
        CaptionResources1.TitleFont = New System.Drawing.Font("Microsoft YaHei", 16.0!)
        Me.Caption1.UIThemes = CaptionResources1
        '
        'UserAvatarMenu1
        '
        Me.UserAvatarMenu1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.UserAvatarMenu1.Avatar = Nothing
        Me.UserAvatarMenu1.EMail = "Label2"
        Me.UserAvatarMenu1.Location = New System.Drawing.Point(731, 63)
        Me.UserAvatarMenu1.Name = "UserAvatarMenu1"
        Me.UserAvatarMenu1.Size = New System.Drawing.Size(185, 86)
        Me.UserAvatarMenu1.TabIndex = 2
        '
        'FormWin10
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(928, 709)
        Me.Controls.Add(Me.UserAvatarMenu1)
        Me.Controls.Add(Me.Back)
        Me.Controls.Add(Me.Caption1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "FormWin10"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Back As MolkPlusTheme.Windows.Forms.Controls.Button
    Friend WithEvents Caption1 As MolkPlusTheme.Windows.Forms.Controls.Caption
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents UserAvatarMenu1 As UserAvatarMenu
End Class

Imports Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage

Namespace Visualise.Elements

    Public Class MarginBlue : Inherits Visualise.Elements.MultipleTabPage

        Sub New()
            Me.LabelHeight = 32
            Me.Font = New Font(FONT_FAMILY_MICROSOFT_YAHEI, 12)
            Me.BorderColor = New Pen(New SolidBrush(color:=Color.FromArgb(222, 222, 222)))

            Dim Gr = New Size(10000, 3).CreateGDIDevice(Color.FromArgb(2, 111, 194))
            Me.SeperatorBarResource = Gr.ImageResource
            Me.UIResource = New ButtonResource With {
                .TextoffSets = New Point(20, 10),
                .Active = New Size(500, 200).CreateGDIDevice(Color.FromArgb(4, 111, 191)).ImageAddFrame(BorderColor).ImageResource,
                .Cursor = Cursors.Arrow,
                .InSensitive = New Size(500, 200).CreateGDIDevice(Color.White).ImageAddFrame(BorderColor).ImageResource,
                .Normal = New Size(500, 200).CreateGDIDevice(Color.White).ImageAddFrame(BorderColor).ImageResource,
                .PreLight = New Size(500, 200).CreateGDIDevice(Color.FromArgb(149, 187, 222)).ImageAddFrame(BorderColor).ImageResource,
                .BorderColor = Me.BorderColor,
                .TextAlign = ButtonResource.TextAlignments.Middle,
                .ActiveTextColor = Color.White,
                .DisableTextColor = Color.Gray,
                .HighLightTextColor = Color.Black,
                .NormalTextColor = Color.Black
            }
            Me.TabSpacing = 8
        End Sub

        Public Overrides Sub SetupResource(ByRef ctrl As MultipleTabpagePanel)
            MyBase.SetupResource(ctrl)
            ctrl.ForeColor = Color.White
            ctrl.DisabledCloseControl = True
            ctrl.EnableMenu = False
            ctrl.SizeMode = MultipleTabpagePanel.TabpageSizeModes.CaptionTextLengthAutoSize
            ctrl.PageInterval = 5
            ctrl.Renderer = Me
        End Sub
    End Class
End Namespace
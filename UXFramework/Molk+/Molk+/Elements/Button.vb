#Region "Microsoft.VisualBasic::b40fff33d6a85d751a1b5ea0eeb0d601, ..\VisualBasic_AppFramework\UXFramework\Molk+\Molk+\Elements\Button.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region


Imports Microsoft.VisualBasic.Imaging

Namespace Visualise.Elements

    Public Class ButtonResource

        Public Property Cursor As Cursor = Cursors.Arrow
        Public Property TextAlign As TextAlignments = TextAlignments.Left
        Public Property TextoffSets As Point = New Point(3, 2)
        Public Property Normal As Image
        Public Property PreLight As Image
        Public Property Active As Image
        Public Property InSensitive As Image
        Public Property BorderColor As Pen
        Public Property ActiveTextColor As Color = Color.Black
        Public Property NormalTextColor As Color = Color.White
        Public Property HighLightTextColor As Color = Color.Black
        Public Property DisableTextColor As Color = Color.DarkGray

        Public Function IsNull() As Boolean
            Return Normal Is Nothing
        End Function

        Public Function Clone() As ButtonResource
            Dim Nres As Image = If(Normal Is Nothing, Nothing, DirectCast(Normal.Clone, Image))
            Dim Pres As Image = If(PreLight Is Nothing, Nothing, DirectCast(PreLight.Clone, Image))
            Dim Ares As Image = If(Active Is Nothing, Nothing, DirectCast(Active.Clone, Image))
            Dim Ires As Image = If(InSensitive Is Nothing, Nothing, DirectCast(InSensitive.Clone, Image))

            Return New ButtonResource With
                {
                    .Active = Ares,
                    .InSensitive = Ires,
                    .Normal = Nres,
                    .PreLight = Pres,
                    .ActiveTextColor = ActiveTextColor,
                    .BorderColor = BorderColor,
                    .Cursor = Cursor,
                    .DisableTextColor = DisableTextColor,
                    .HighLightTextColor = HighLightTextColor,
                    .NormalTextColor = NormalTextColor,
                    .TextAlign = TextAlign,
                    .TextoffSets = TextoffSets
            }
        End Function

        Public Enum TextAlignments
            Left
            Right
            Middle
            Top
            Bottom
        End Enum
    End Class

    Public Class ButtonRender

        Dim _HighlightColor As Color,
            _PressColor As Color,
            _NormalColor As Color,
            _TextColor As SolidBrush

        Public ReadOnly Property Font As Font

        Sub New(HighlightColor As Color,
                PressColor As Color,
                NormalColor As Color,
                TextColor As Color,
                Font As Font)

            _HighlightColor = HighlightColor
            _PressColor = PressColor
            _NormalColor = NormalColor
            _TextColor = New SolidBrush(TextColor)
            _Font = Font
        End Sub

        ''' <summary>
        ''' 假若复选按钮当作普通按钮来使用的话，可以使用这个方法来渲染。
        ''' 请注意这个方法不会复选控件的处理文本标签部分，仅仅处理左边的图标部分的UI
        ''' </summary>
        ''' <param name="Checkbox"></param>
        Public Sub RenderButton(ByRef Checkbox As MolkPlusTheme.Windows.Forms.Controls.Checkbox,
                                cbSize As Size,
                                Optional Font As Font = Nothing)

            Dim res As Checkbox = New Checkbox With
                {
                    .BackgroundColor = Checkbox.BackColor,
                    .ForeColor = _TextColor.Color,
                    .PrelightColor = _TextColor.Color,
                    .CheckboxMargin = New Drawing.Rectangle(New Point, cbSize),
                    .LabelMargin = 0
            }

            Dim btnRender = Render(Checkbox.LabelText, cbSize, Font)

            res.Check = btnRender.Active
            res.CheckPreLight = btnRender.Active
            res.Disable = btnRender.InSensitive
            res.UnCheck = btnRender.Normal
            res.UncheckPreLight = btnRender.PreLight

            Checkbox.Text = Checkbox.LabelText
            Checkbox.LabelText = ""
            Checkbox.UI = res
        End Sub

        Public Sub RenderButton(Button As Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Button,
                                Optional Font As Font = Nothing)

            Dim res = Render(Button.MyText, Button.Size, Font)
            Button.UI = res
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Text"></param>
        ''' <param name="Size">按钮的大小</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Render(Text As String,
                               Size As Size,
                               Optional Font As Font = Nothing) As ButtonResource
            Dim res As ButtonResource = New ButtonResource

            If Font Is Nothing Then Font = Me._Font

            Dim Gr = InternalCreateButton(Size, _NormalColor)
            Gr.Graphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            Gr.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
            Call Gr.Graphics.DrawRectangle(New Pen(Color.FromArgb(229, 229, 229), 1),
                                            New Rectangle(New Point, New Size(Gr.Width - 1, Gr.Height - 1)))

            Call InternalTextRender(Gr, Text, _TextColor, Font)
            res.Normal = Gr.ImageResource

            Gr = InternalCreateButton(Size, _HighlightColor)
            Gr.Graphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            Gr.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
            Call InternalTextRender(Gr, Text, Brushes.White, Font)
            res.PreLight = Gr.ImageResource

            Gr = InternalCreateButton(Size, _PressColor)
            Gr.Graphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            Gr.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
            Call InternalTextRender(Gr, Text, Brushes.White, Font)
            res.Active = Gr.ImageResource

            Gr = InternalCreateButton(Size, Color.Gray)
            Gr.Graphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            Gr.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
            Call Gr.Graphics.DrawRectangle(New Pen(Color.FromArgb(229, 229, 229), 1),
                                            New Rectangle(New Point, New Size(Gr.Width - 1, Gr.Height - 1)))

            Call InternalTextRender(Gr, Text, _TextColor, Font)
            res.InSensitive = Gr.ImageResource

            Return res
        End Function

        Private Shared Function InternalCreateButton(Size As Size, FilledColor As Color) As GDIPlusDeviceHandle
            Dim Gr = Size.CreateGDIDevice(FilledColor)
            Return Gr
        End Function

        Private Shared Sub InternalTextRender(ByRef Gr As GDIPlusDeviceHandle,
                                              Text As String,
                                              TextColor As SolidBrush,
                                              Font As Font)
            Dim Size = Gr.Graphics.MeasureString(Text, Font)

            Call Gr.Graphics.DrawString(Text,
                                         Font,
                                         TextColor,
                                         New Point((Gr.Width - Size.Width) / 2, (Gr.Height - Size.Height) / 2))
        End Sub
    End Class

    Public Class Checkbox

#Region "Image resources"

        Public Property Check As Image
        Public Property CheckPreLight As Image
        Public Property UnCheck As Image
        Public Property UncheckPreLight As Image
        Public Property Disable As Image
#End Region

        Public Property CheckboxMargin As Rectangle

        Public Property BackgroundColor As Color = Color.White
        ''' <summary>
        ''' 标签字符串在普通状态下的颜色
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ForeColor As Color = Color.Black
        ''' <summary>
        ''' 标签字符串的高亮颜色
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PrelightColor As Color = Color.Gray
        ''' <summary>
        ''' 标签和图标之间的距离
        ''' </summary>
        ''' <returns></returns>
        Public Property LabelMargin As Integer = 5
        Public Property AutoSize As Boolean = True

        Public Shared Function GetDefault() As Checkbox
            Return New Checkbox With {
                .Check = My.Resources.Check,
                .CheckPreLight = My.Resources.CheckPreLight,
                .UnCheck = My.Resources.Uncheck,
                .UncheckPreLight = My.Resources.UncheckPreLight,
                .BackgroundColor = Color.FromArgb(22, 32, 48),
                .PrelightColor = Color.White,
                .ForeColor = Color.Gray,
                .CheckboxMargin = New Drawing.Rectangle(New Point With {.X = 2, .Y = 1}, New Size)}
        End Function

        Public Sub Render(ByRef Checkbox As MolkPlusTheme.Windows.Forms.Controls.Checkbox)
            Dim res As Checkbox = New Checkbox With
                {
                    .BackgroundColor = BackgroundColor,
                    .ForeColor = ForeColor,
                    .PrelightColor = PrelightColor
            }
            If Not Check Is Nothing Then res.Check = DirectCast(Check.Clone, Image)
            If Not CheckPreLight Is Nothing Then res.CheckPreLight = DirectCast(CheckPreLight.Clone, Image)
            If Not Disable Is Nothing Then res.Disable = DirectCast(Disable, Image)
            If Not UnCheck Is Nothing Then res.UnCheck = DirectCast(UnCheck, Image)
            If Not UncheckPreLight Is Nothing Then res.UncheckPreLight = DirectCast(UncheckPreLight, Image)

            Checkbox.UI = res
        End Sub
    End Class

    Public Class TabLabel
        Public Check, UnCheck As Image
        Public Head As Image
        Public ColorSchema As Drawing.Brush = Brushes.AliceBlue
    End Class
End Namespace


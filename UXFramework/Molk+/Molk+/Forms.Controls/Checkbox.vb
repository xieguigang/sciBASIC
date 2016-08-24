#Region "Microsoft.VisualBasic::544eb2811628228dfac1782af5710780, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\Checkbox.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports System.ComponentModel

Namespace Windows.Forms.Controls

    Public Class Checkbox

        ''' <summary>
        ''' 单选模式，这个属性只是表示本控件不会在选中之后再次通过点击而取消选中状态，选中状态的取消需要通过外部事件来完成
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Description("單選模式")>
        Public Property RatioMode As Boolean = False
        ''' <summary>
        ''' 复选框的文本标签是否也可以成为工作动作的一个触发点
        ''' </summary>
        ''' <returns></returns>
        Public Property Integral As Boolean

        <Description("复选框上面的提示信息")> <Localizable(True)>
        Public Property LabelText As String
            Get
                Return Label1.Text
            End Get
            Set(value As String)
                Label1.Text = value
                Call Resize2()
            End Set
        End Property

        <Localizable(True)>
        Public Overrides Property Text As String

        ''' <summary>
        ''' Create a checkbox ui with text and then assign the new ui to variable [_UI]
        ''' </summary>
        ''' <remarks></remarks>
        Dim __uiElement As MolkPlusTheme.Visualise.Elements.Checkbox
        Dim _checked As Boolean

        ''' <summary>
        ''' 用户使用鼠标修改了控件的状态
        ''' </summary>
        ''' <param name="Checked"></param>
        ''' <remarks></remarks>
        Public Event CheckStateChanged(Checked As Boolean)

        Public Overrides Property BackColor As Color
            Get
                Return MyBase.BackColor
            End Get
            Set(value As Color)
                MyBase.BackColor = value
                Label1.BackColor = value
            End Set
        End Property

        ''' <summary>
        ''' 开发者使用代码修改状态，这个操作不会触发<see cref="CheckStateChanged"></see>控件事件
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Description("控件的选中状态")>
        Public Property Checked As Boolean
            Get
                Return _checked
            End Get
            Set(value As Boolean)
                _checked = value
                _PreviousCheckState = value
                If Not UI Is Nothing Then PictureBox1.BackgroundImage = IIf(_checked, UI.Check, UI.UnCheck)
            End Set
        End Property

        Public Property UI As MolkPlusTheme.Visualise.Elements.Checkbox
            Get
                Return __uiElement
            End Get
            Set(value As MolkPlusTheme.Visualise.Elements.Checkbox)
                __uiElement = value
                PictureBox1.BackgroundImage = IIf(_checked, value.Check, value.UnCheck)
                If value.AutoSize AndAlso Not PictureBox1.BackgroundImage Is Nothing Then
                    PictureBox1.Size = PictureBox1.BackgroundImage.Size
                ElseIf Not value.AutoSize
                    PictureBox1.BackgroundImageLayout = ImageLayout.Stretch
                End If
                BackColor = value.BackgroundColor

                Call Resize2()
            End Set
        End Property

        Private Overloads Sub Resize2()
            If PictureBox1.BackgroundImage Is Nothing Then Return

            Size = New Size With {
                .Width = PictureBox1.BackgroundImage.Width + UI.LabelMargin + Label1.Width,
                .Height = PictureBox1.BackgroundImage.Height + UI.LabelMargin / 2}

            PictureBox1.Location = UI.CheckboxMargin.Location
            Label1.Location = New Point With {.X = PictureBox1.Width + UI.LabelMargin, .Y = (Height - Label1.Height) / 2 + 2}
        End Sub

        Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click
            If Integral Then
                Call Check(sender, Nothing)
            End If
        End Sub

        Private Sub Checkbox_MouseEnter(sender As Object, e As EventArgs) Handles Me.MouseEnter, PictureBox1.MouseEnter, Label1.MouseLeave
            PictureBox1.BackgroundImage = IIf(_checked, UI.CheckPreLight, UI.UncheckPreLight)
            Label1.ForeColor = UI.PrelightColor
        End Sub

        Private Sub Checkbox_MouseLeave(sender As Object, e As EventArgs) Handles Me.MouseLeave, PictureBox1.MouseLeave, Label1.MouseLeave
            PictureBox1.BackgroundImage = IIf(_checked, UI.Check, UI.UnCheck)
            Label1.ForeColor = UI.ForeColor
        End Sub

        Dim _PreviousCheckState As Boolean = False

        Private Sub Check(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseUp
            If _PreviousCheckState = True AndAlso RatioMode Then
                Return
            End If
            Checked = Not _checked
            RaiseEvent CheckStateChanged(_checked)
        End Sub

        Private Sub Checkbox_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            UI = MolkPlusTheme.Visualise.Elements.Checkbox.GetDefault
            BackColor = UI.BackgroundColor
        End Sub
    End Class
End Namespace

#Region "Microsoft.VisualBasic::dc48d78535faad64bd6feb9cdce27f73, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\Button.vb"

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
Imports Microsoft.VisualBasic.Imaging

Namespace Windows.Forms.Controls

    ''' <summary>
    ''' 一个可以自定义样式的按钮控件
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Button : Inherits System.Windows.Forms.UserControl

        Dim _visualizeUIRes As Visualise.Elements.ButtonResource
        Dim InternalRenderer As Visualise.Elements.ButtonRender

        ''' <summary>
        ''' The UI style definition data for this button control.(这个按钮控件的UI样式定义数据)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Category("")>
        <Description("The UI style definition data for this button control.(这个按钮控件的UI样式定义数据)")>
        Public Property UI As Visualise.Elements.ButtonResource
            Get
                Return _visualizeUIRes
            End Get
            Set(value As Visualise.Elements.ButtonResource)
                OriginalUIRes = value

                If Not value Is Nothing Then
                    Me.Cursor = value.Cursor
                    Call __setUIRes(value.Clone)
                End If

            End Set
        End Property

        Public Property Render As Visualise.Elements.ButtonRender
            Get
                Return InternalRenderer
            End Get
            Set(value As Visualise.Elements.ButtonRender)
                InternalRenderer = value
                If Not InternalRenderer Is Nothing Then
                    Call InternalRenderer.RenderButton(Me)
                End If
            End Set
        End Property

        Public Overrides Property BackgroundImage As Drawing.Image
            Get
                Return MyBase.BackgroundImage
            End Get
            Set(value As Drawing.Image)
                MyBase.BackgroundImage = value
                If _AutoSize Then
                    If Not value Is Nothing Then Size = value.Size
                Else
                    BackgroundImageLayout = ImageLayout.Stretch
                End If
            End Set
        End Property

        Dim OriginalUIRes As MolkPlusTheme.Visualise.Elements.ButtonResource

        <Localizable(True)>
        Public Property MyText As String
            Get
                Return MyBase.Text
            End Get
            Set(value As String)
                MyBase.Text = value
                Call __updateResource()
            End Set
        End Property

        Public Property MyFont As Font
            Get
                Return MyBase.Font
            End Get
            Set(value As Font)
                MyBase.Font = value
                Call __updateResource()
            End Set
        End Property

        Public Property MyFontColor As Color
            Get
                Return MyBase.ForeColor
            End Get
            Set(value As Color)
                MyBase.ForeColor = value
                Call __updateResource()
            End Set
        End Property

        Public Sub SetEnabled(value As Boolean)
            If Enabled Then
                BackgroundImage = _visualizeUIRes.Normal
            Else
                If _visualizeUIRes.InSensitive Is Nothing Then
                    BackgroundImage = _visualizeUIRes.Normal
                Else
                    BackgroundImage = _visualizeUIRes.InSensitive
                End If
            End If

            Me.Enabled = value
        End Sub

        Private Sub __updateResource()
            If Not Render Is Nothing Then
                Call Render.RenderButton(Me)
                Return
            End If

            If OriginalUIRes Is Nothing Then Return

            Dim res = OriginalUIRes.Clone

            Dim InternalRedraw = Sub(ByRef resImage As System.Drawing.Image, Color As Color)
                                     If resImage Is Nothing Then
                                         Return
                                     End If

                                     Dim Gr = GDIPlusExtensions.GdiFromImage(resImage)
                                     Dim size = Gr.Graphics.MeasureString(Text, Font)
                                     Dim Loci As Point = New Point((Width - size.Width) / 2, (Height - size.Height) / 2) '‘文字剧中显示

                                     Call Gr.Graphics.DrawString(Text, Font, New SolidBrush(Color), Loci)
                                 End Sub

            Call InternalRedraw(res.Active, Color.Black)
            Call InternalRedraw(res.InSensitive, Color.Gray)
            Call InternalRedraw(res.Normal, ForeColor)
            Call InternalRedraw(res.PreLight, ForeColor)

            Call __setUIRes(res)
        End Sub

        ''' <summary>
        ''' 这个函数不会修改原始数据
        ''' </summary>
        ''' <param name="value"></param>
        ''' <remarks></remarks>
        Private Sub __setUIRes(value As Visualise.Elements.ButtonResource)
            If value Is Nothing Then Return

            _visualizeUIRes = value

            If _visualizeUIRes Is Nothing OrElse _visualizeUIRes.IsNull Then
                Me.Visible = False
                Me.Hide()
            Else
                Me.Show()
                Visible = True
                BackgroundImage = value.Normal
            End If
        End Sub

        Public Overrides Property AutoSize As Boolean = False

        Public Sub ResetToNormal()
            If Not UI Is Nothing Then
                BackgroundImage = UI.Normal
            End If
        End Sub

        Private Sub Button_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
            If Not (UI Is Nothing OrElse UI.Active Is Nothing) Then BackgroundImage = UI.Active
        End Sub

        Dim pctrl As Control

        Private Sub Button_MouseEnter(sender As Object, e As EventArgs) Handles Me.MouseEnter
            If pctrl Is Nothing Then
                pctrl = Me.Parent
                AddHandler pctrl.MouseEnter, Sub() Call ResetToNormal()
            End If

            If Not (UI Is Nothing OrElse UI.PreLight Is Nothing) Then BackgroundImage = UI.PreLight
        End Sub

        Private Sub Button_MouseLeave(sender As Object, e As EventArgs) Handles Me.MouseLeave
            If Not UI Is Nothing Then
                BackgroundImage = UI.Normal
            End If
        End Sub

        ''' <summary>
        ''' Occurs when this <see cref="Button"/> control is clicked.
        ''' </summary>
        Public Event ButtonClick()

        Private Sub Button_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
            If Not UI Is Nothing Then
                BackgroundImage = UI.PreLight
            End If

            RaiseEvent ButtonClick()
        End Sub
    End Class
End Namespace

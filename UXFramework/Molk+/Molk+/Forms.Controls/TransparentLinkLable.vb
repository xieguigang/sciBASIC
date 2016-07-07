#Region "Microsoft.VisualBasic::e5ac100f26cec5acfb602ef88c46d4ee, ..\VisualBasic_AppFramework\UXFramework\Molk+\Molk+\Forms.Controls\TransparentLinkLable.vb"

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

Public Class TransparentLinkLable

    Public Property LabelText As String
        Get
            Return MyBase.Text
        End Get
        Set(value As String)
            MyBase.Text = value
            Call Update()
        End Set
    End Property

    Public Overrides Property Text As String
        Get
            Return MyBase.Text
        End Get
        Set(value As String)
            MyBase.Text = value
            Call Update()
        End Set
    End Property

    Public Overrides Property Font As Font
        Get
            Return MyBase.Font
        End Get
        Set(value As Font)
            MyBase.Font = value
            Call Update()
        End Set
    End Property

    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(value As Color)
            MyBase.ForeColor = value
            Call Update()
        End Set
    End Property

    Public Overloads Sub Update()
        If String.IsNullOrEmpty(Text) Then
            Return
        End If

        If Me.Parent Is Nothing Then
            Return
        End If

        On Error Resume Next

        Dim Size = Text.MeasureString(Me.Font)
        Dim Bitmap As New Bitmap(CInt(Size.Width), CInt(Size.Height))
        Dim Gr = Graphics.FromImage(Bitmap)

        Me.Size = New Size(Size.Width, Size.Height)

        Gr.CompositingQuality = Drawing2D.CompositingQuality.HighQuality

        If Parent.BackgroundImage Is Nothing Then
            Call Gr.FillRectangle(New SolidBrush(Parent.BackColor), New Rectangle(New Point, Size))
        Else
            Dim Crop = Me.Parent.BackgroundImage.ImageCrop(Me.Location, Me.Size)
            Call Gr.DrawImage(Crop, New Point)
        End If

        Call Gr.DrawString(Text, Font, New SolidBrush(ForeColor), New Point)

        Me.BackgroundImage = Bitmap
    End Sub

    Private Sub TransparentLinkLable_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call Update()
    End Sub
End Class


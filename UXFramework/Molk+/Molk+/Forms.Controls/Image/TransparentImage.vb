#Region "Microsoft.VisualBasic::43c7925769ad56682983913e3c791553, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\Image\TransparentImage.vb"

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

Imports Microsoft.VisualBasic.Imaging
''' <summary>
''' 当显示不规则图形的时候，使用本控件提供窗体透明的效果，即本控件可以显出不规则区域的底图
''' </summary>
''' <remarks></remarks>
Public Class TransparentImage

    Dim _BackImage As Image
    Dim _OffSet As Point

    Public Property ImageOffSet As Point
        Get
            Return _OffSet
        End Get
        Set(value As Point)
            _OffSet = value
            Call Update()
        End Set
    End Property

    Public ReadOnly Property BackgroundImageResource As Image
        Get
            If MyBase.BackgroundImage Is Nothing Then
                '没有背景图，则使用父控件的背景色作为图片返回
                Dim Gr = Me.Size.CreateGDIDevice(Me.Parent.BackColor)
                Return Gr.ImageResource
            Else
                Return MyBase.BackgroundImage
            End If
        End Get
    End Property

    Public Property Image As Image
        Get
            Return _BackImage
        End Get
        Set(value As Image)
            _BackImage = value
            Call Update()
        End Set
    End Property

    Private Sub TransparentImage_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call Update()
    End Sub

    Public Overloads Function Update() As Image
        If Me.Parent Is Nothing Then
            Return Nothing
        End If

        If Me._BackImage Is Nothing Then
            Return Nothing
        End If

        Dim resParent As Image = Me.Parent.BackgroundImage

        If resParent Is Nothing Then
            Dim Grd = Me.Parent.Size.CreateGDIDevice(Me.Parent.BackColor)
            resParent = Grd.ImageResource
        End If

        Dim Corp = resParent.ImageCrop(Me.Location, Me.Size)    '通过剪裁获取底图
        Dim Gr As Graphics = Graphics.FromImage(Corp)

        Call Gr.DrawImage(_BackImage, _OffSet)

        Me.BackgroundImage = Corp
        Return Corp
    End Function

    Private Sub TransparentImage_LocationChanged(sender As Object, e As EventArgs) Handles Me.LocationChanged
        Call Update()
    End Sub

    Private Sub TransparentImage_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Call Update()
    End Sub
End Class

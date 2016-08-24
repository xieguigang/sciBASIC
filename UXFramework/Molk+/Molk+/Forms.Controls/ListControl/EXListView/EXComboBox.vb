#Region "Microsoft.VisualBasic::905e0922185484560d3dcef5c0a1f48f, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\EXListView\EXComboBox.vb"

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

Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections

Namespace Windows.Forms.Controls.ListControl

    Class EXComboBox
        Inherits ComboBox

        Private _highlightbrush As Brush
        'color of highlighted items
        Public Sub New()
            _highlightbrush = SystemBrushes.Highlight
            Me.DrawMode = DrawMode.OwnerDrawFixed
            AddHandler Me.DrawItem, New DrawItemEventHandler(AddressOf this_DrawItem)
        End Sub

        Public Property MyHighlightBrush() As Brush
            Get
                Return _highlightbrush
            End Get
            Set
                _highlightbrush = Value
            End Set
        End Property

        Private Sub this_DrawItem(sender As Object, e As DrawItemEventArgs)
            If e.Index = -1 Then
                Return
            End If
            e.DrawBackground()
            If (e.State And DrawItemState.Selected) <> 0 Then
                e.Graphics.FillRectangle(_highlightbrush, e.Bounds)
            End If
            Dim item As EXItem = DirectCast(Me.Items(e.Index), EXItem)
            Dim bounds As Rectangle = e.Bounds
            Dim x As Integer = bounds.X + 2
            If item.[GetType]() Is GetType(EXImageItem) Then
                Dim imgitem As EXImageItem = DirectCast(item, EXImageItem)
                If imgitem.MyImage IsNot Nothing Then
                    Dim img As System.Drawing.Image = imgitem.MyImage
                    Dim y As Integer = bounds.Y + CInt(bounds.Height \ 2) - CInt(img.Height \ 2) + 1
                    e.Graphics.DrawImage(img, x, y, img.Width, img.Height)
                    x += img.Width + 2
                End If
            ElseIf item.[GetType]() Is GetType(EXMultipleImagesItem) Then
                Dim imgitem As EXMultipleImagesItem = DirectCast(item, EXMultipleImagesItem)
                If imgitem.MyImages IsNot Nothing Then
                    For i As Integer = 0 To imgitem.MyImages.Count - 1
                        Dim img As System.Drawing.Image = DirectCast(imgitem.MyImages(i), System.Drawing.Image)
                        Dim y As Integer = bounds.Y + CInt(bounds.Height \ 2) - CInt(img.Height \ 2) + 1
                        e.Graphics.DrawImage(img, x, y, img.Width, img.Height)
                        x += img.Width + 2
                    Next
                End If
            End If
            Dim fonty As Integer = bounds.Y + CInt(bounds.Height \ 2) - CInt(e.Font.Height \ 2)
            e.Graphics.DrawString(item.Text, e.Font, New SolidBrush(e.ForeColor), x, fonty)
            e.DrawFocusRectangle()
        End Sub

        Public Class EXItem

            Private _text As String = ""
            Private _value As String = ""


            Public Sub New()
            End Sub

            Public Sub New(text As String)
                _text = text
            End Sub

            Public Property Text() As String
                Get
                    Return _text
                End Get
                Set
                    _text = Value
                End Set
            End Property

            Public Property MyValue() As String
                Get
                    Return _value
                End Get
                Set
                    _value = Value
                End Set
            End Property

            Public Overrides Function ToString() As String
                Return _text
            End Function

        End Class

        Public Class EXImageItem
            Inherits EXItem

            Private _image As System.Drawing.Image


            Public Sub New()
            End Sub

            Public Sub New(text As String)
                Me.Text = text
            End Sub

            Public Sub New(image As System.Drawing.Image)
                _image = image
            End Sub

            Public Sub New(text As String, image As System.Drawing.Image)
                Me.Text = text
                _image = image
            End Sub

            Public Sub New(image As System.Drawing.Image, value As String)
                _image = image
                Me.MyValue = value
            End Sub

            Public Sub New(text As String, image As System.Drawing.Image, value As String)
                Me.Text = text
                _image = image
                Me.MyValue = value
            End Sub

            Public Property MyImage() As System.Drawing.Image
                Get
                    Return _image
                End Get
                Set
                    _image = Value
                End Set
            End Property

        End Class

        Public Class EXMultipleImagesItem
            Inherits EXItem

            Private _images As ArrayList


            Public Sub New()
            End Sub

            Public Sub New(text As String)
                Me.Text = text
            End Sub

            Public Sub New(images As ArrayList)
                _images = images
            End Sub

            Public Sub New(text As String, images As ArrayList)
                Me.Text = text
                _images = images
            End Sub

            Public Sub New(images As ArrayList, value As String)
                _images = images
                Me.MyValue = value
            End Sub

            Public Sub New(text As String, images As ArrayList, value As String)
                Me.Text = text
                _images = images
                Me.MyValue = value
            End Sub

            Public Property MyImages() As ArrayList
                Get
                    Return _images
                End Get
                Set
                    _images = Value
                End Set
            End Property

        End Class

    End Class

End Namespace

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Text

Namespace HTML.CSS.Render

    Public Class CssRectangle
#Region "Fields"
        Private _left As Single
        Private _top As Single
        Private _width As Single
        Private _height As Single


#End Region

#Region "Props"



        ''' <summary>
        ''' Left of the rectangle
        ''' </summary>
        Public Property Left() As Single
            Get
                Return _left
            End Get
            Set
                _left = Value
            End Set
        End Property

        ''' <summary>
        ''' Top of the rectangle
        ''' </summary>
        Public Property Top() As Single
            Get
                Return _top
            End Get
            Set
                _top = Value
            End Set
        End Property

        ''' <summary>
        ''' Width of the rectangle
        ''' </summary>
        Public Property Width() As Single
            Get
                Return _width
            End Get
            Set
                _width = Value
            End Set
        End Property

        ''' <summary>
        ''' Height of the rectangle
        ''' </summary>
        Public Property Height() As Single
            Get
                Return _height
            End Get
            Set
                _height = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the right of the rectangle. When setting, it only affects the Width of the rectangle.
        ''' </summary>
        Public Property Right() As Single
            Get
                Return Bounds.Right
            End Get
            Set
                Width = Value - Left
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the bottom of the rectangle. When setting, it only affects the Height of the rectangle.
        ''' </summary>
        Public Property Bottom() As Single
            Get
                Return Bounds.Bottom
            End Get
            Set
                Height = Value - Top
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the bounds of the rectangle
        ''' </summary>
        Public Property Bounds() As RectangleF
            Get
                Return New RectangleF(Left, Top, Width, Height)
            End Get
            Set
                Left = Value.Left
                Top = Value.Top
                Width = Value.Width
                Height = Value.Height
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the location of the rectangle
        ''' </summary>
        Public Property Location() As PointF
            Get
                Return New PointF(Left, Top)
            End Get
            Set
                Left = Value.X
                Top = Value.Y
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the size of the rectangle
        ''' </summary>
        Public Property Size() As SizeF
            Get
                Return New SizeF(Width, Height)
            End Get
            Set
                Width = Value.Width
                Height = Value.Height
            End Set
        End Property

#End Region
    End Class
End Namespace
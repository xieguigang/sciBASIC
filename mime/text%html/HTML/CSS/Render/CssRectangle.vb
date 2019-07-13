#Region "Microsoft.VisualBasic::d672a5eba1e49c71f50a09c4f92225f9, mime\text%html\HTML\CSS\Render\CssRectangle.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class CssRectangle
    ' 
    '         Properties: Bottom, Bounds, Height, Left, Location
    '                     Right, Size, Top, Width
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace HTML.CSS.Render

    Public Class CssRectangle

#Region "Props"

        ''' <summary>
        ''' Left of the rectangle
        ''' </summary>
        Public Property Left() As Single

        ''' <summary>
        ''' Top of the rectangle
        ''' </summary>
        Public Property Top() As Single

        ''' <summary>
        ''' Width of the rectangle
        ''' </summary>
        Public Property Width() As Single

        ''' <summary>
        ''' Height of the rectangle
        ''' </summary>
        Public Property Height() As Single

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

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::53a2cab73731d25e6cfa01e9a741b402, mime\text%html\Render\CSS\CssRectangle.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 100
    '    Code Lines: 60 (60.00%)
    ' Comment Lines: 27 (27.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (13.00%)
    '     File Size: 2.75 KB


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

Namespace Render.CSS

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

#Region "Microsoft.VisualBasic::88b4c3070681b1f02b3f49f41409a948, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\String.vb"

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

Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Namespace Drawing2D.Vector.Text

    Public Class [String] : Inherits VectorObject

        Public Property Text As String
        Public Property Pen As Brush
        Public Property Font As Font

        Sub New(locat As Point, size As Size)
            Call MyBase.New(locat, size)
        End Sub

        Sub New(rect As Rectangle)
            Call MyBase.New(rect)
        End Sub

        Sub New(text As TextString, rect As Rectangle)
            Call Me.New(text.Text, text.Font, rect)
        End Sub

        Sub New(text As TextString)
            Call Me.New(text, Nothing)
        End Sub

        Sub New(text As String, font As Font, rect As Rectangle)
            Call MyBase.New(rect)

            Me.Text = text
            Me.Font = font
            Me.Pen = New SolidBrush(Color.Black)
        End Sub

        Sub New(text As [String], rect As Rectangle)
            Call Me.New(text.Text, text.Font, rect)
        End Sub

        Public Overrides Function ToString() As String
            Return Text
        End Function

        ''' <summary>
        ''' Measures the specified string when drawn with the specified System.Drawing.Font.
        ''' </summary>
        ''' <returns>This method returns a System.Drawing.SizeF structure that represents the size,
        ''' in the units specified by the System.Drawing.Graphics.PageUnit property, of the
        ''' string specified by the text parameter as drawn with the font parameter.</returns>
        Public Function MeasureString(gdi As Graphics2D) As SizeF
            Return gdi.MeasureString(Text, Font)
        End Function

        ''' <summary>
        ''' Draws the specified text string in the specified rectangle with the specified
        ''' System.Drawing.Brush and System.Drawing.Font objects.
        ''' </summary>
        Public Overrides Sub Draw(gdi As Graphics2D)
            Call Draw(gdi, RECT)
        End Sub

        ''' <summary>
        ''' Draws the specified text string in the specified rectangle with the specified
        ''' System.Drawing.Brush and System.Drawing.Font objects.
        ''' </summary>
        Public Overrides Sub Draw(gdi As Graphics2D, loci As Rectangle)
            Call gdi.DrawString(Text, Font, Pen, loci)
        End Sub
    End Class
End Namespace

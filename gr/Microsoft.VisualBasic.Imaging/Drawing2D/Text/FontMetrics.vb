#Region "Microsoft.VisualBasic::7efbb902579954f4254e686efbde6f59, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\FontMetrics.vb"

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

    '   Total Lines: 77
    '    Code Lines: 51
    ' Comment Lines: 14
    '   Blank Lines: 12
    '     File Size: 2.75 KB


    '     Class FontMetrics
    ' 
    '         Properties: Font, Graphics, Height
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) GetStringBounds
    ' 
    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) FontMetrics
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports stdNum = System.Math

Namespace Drawing2D.Text

    Public Class FontMetrics

        Public ReadOnly Property Font As Font
        ''' <summary>
        ''' The default gdi+ graphics context
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Graphics As Graphics
        ''' <summary>
        ''' 在当前的字体条件下面的，使用默认的<see cref="Graphics"/>上下文的文本行高
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Height As Single

        Sub New(font As Font, g As Graphics)
            Me.Font = font

            Height = g.MeasureString("1", font).Height
            Graphics = g
        End Sub

        Sub New(font As CSSFont, g As Graphics)
            Me.New(font.GDIObject(stdNum.Max(g.DpiX, g.DpiY)), g)
        End Sub

        ''' <summary>
        ''' Using another graphics context
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="g"></param>
        ''' <returns></returns>
        Public Shared Function GetStringBounds(s As String, font As Font, g As Graphics) As RectangleF
            Return New RectangleF(New Point, g.MeasureString(s, font))
        End Function

        Public Function GetStringBounds(s As String) As RectangleF
            Return GetStringBounds(s, Font, Graphics)
        End Function

        Public Shared Narrowing Operator CType(f As FontMetrics) As Font
            Return f.Font
        End Operator
    End Class

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FontMetrics(g As Graphics2D) As FontMetrics
            Return New FontMetrics(g.Font, g.Graphics)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FontMetrics(g As IGraphics, font As Font) As FontMetrics
            Select Case g.GetType
                Case GetType(Graphics2D)
                    Return New FontMetrics(font, DirectCast(g, Graphics2D).Graphics)
                Case Else
                    If g.GetType.IsInheritsFrom(GetType(MockGDIPlusGraphics)) Then
                        Return DirectCast(g, MockGDIPlusGraphics).FontMetrics(font)
                    Else
                        Throw New InvalidCastException(g.GetType.FullName)
                    End If
            End Select
        End Function
    End Module
End Namespace

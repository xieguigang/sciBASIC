#Region "Microsoft.VisualBasic::0f006d77c7deb3ba8bbb9b94157400ea, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\d3js\labeler\Label.vb"

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

Namespace d3js.Layout

    Public Class Label

        ''' <summary>
        ''' the x-coordinate of the label.
        ''' </summary>
        ''' <returns></returns>
        Public Property X As Double
        ''' <summary>
        ''' the y-coordinate of the label.
        ''' </summary>
        ''' <returns></returns>
        Public Property Y As Double
        ''' <summary>
        ''' the width of the label (approximating the label as a rectangle).
        ''' </summary>
        ''' <returns></returns>
        Public Property width As Double
        ''' <summary>
        ''' the height of the label (same approximation).
        ''' </summary>
        ''' <returns></returns>
        Public Property height As Double
        ''' <summary>
        ''' the label text.
        ''' </summary>
        ''' <returns></returns>
        Public Property text As String

        Public Overrides Function ToString() As String
            Return $"{text}@({X.ToString("F2")},{Y.ToString("F2")})"
        End Function

        Public Shared Narrowing Operator CType(label As Label) As PointF
            Return New PointF With {
                .X = label.X,
                .Y = label.Y
            }
        End Operator
    End Class
End Namespace

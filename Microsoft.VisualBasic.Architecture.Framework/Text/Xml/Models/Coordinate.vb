#Region "Microsoft.VisualBasic::b11fa38f5bf8be23371d77736a3dd836, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\Xml\Models\Coordinate.vb"

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
Imports System.Xml.Serialization

Namespace Text.Xml.Models

    ''' <summary>
    ''' Improvements on the xml format layout compare with <see cref="Point"/> type.
    ''' </summary>
    Public Structure Coordinate

        <XmlAttribute("x")> Public Property X As Integer
        <XmlAttribute("y")> Public Property Y As Integer

        Sub New(pt As Point)
            Call Me.New(pt.X, pt.Y)
        End Sub

        Sub New(x%, y%)
            Me.X = x
            Me.Y = y
        End Sub

        Public Shared Function FromPointF(x!, y!, multiply%) As Coordinate
            Return New Coordinate With {
                .X = x * multiply,
                .Y = y * multiply
            }
        End Function

        Public Function ToPointF(multiply%) As PointF
            Return New PointF(X / multiply, Y / multiply)
        End Function

        Public Overrides Function ToString() As String
            Return $"[{X}, {Y}]"
        End Function

        Public Shared Operator =(c As Coordinate, pt As Point) As Boolean
            Return c.X = pt.X AndAlso c.Y = pt.Y
        End Operator

        Public Shared Operator <>(c As Coordinate, pt As Point) As Boolean
            Return Not c = pt
        End Operator

        Public Shared Widening Operator CType(pt As Point) As Coordinate
            Return New Coordinate With {.X = pt.X, .Y = pt.Y}
        End Operator

        Public Shared Narrowing Operator CType(x As Coordinate) As Point
            Return New Point(x.X, x.Y)
        End Operator

        Public Shared Widening Operator CType(pt As Integer()) As Coordinate
            Return New Coordinate(pt.FirstOrDefault, pt.LastOrDefault)
        End Operator
    End Structure
End Namespace

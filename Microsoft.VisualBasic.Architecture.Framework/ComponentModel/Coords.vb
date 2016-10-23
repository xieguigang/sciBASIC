#Region "Microsoft.VisualBasic::65e4cf08042bc58db0e0fe6cc6f1da97, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Coords.vb"

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

Namespace ComponentModel

    Public Class Coords

        <XmlAttribute("x")> Public Property X As Integer
        <XmlAttribute("y")> Public Property Y As Integer

        Sub New()
        End Sub

        Sub New(pt As Point)
            Call Me.New(pt.X, pt.Y)
        End Sub

        Sub New(x As Integer, y As Integer)
            Me.X = x
            Me.Y = y
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X}, {Y}]"
        End Function

        Public Shared Widening Operator CType(pt As Point) As Coords
            Return New Coords With {.X = pt.X, .Y = pt.Y}
        End Operator

        Public Shared Narrowing Operator CType(x As Coords) As Point
            Return New Point(x.X, x.Y)
        End Operator

        Public Shared Widening Operator CType(pt As Integer()) As Coords
            Return New Coords(pt.FirstOrDefault, pt.LastOrDefault)
        End Operator
    End Class
End Namespace

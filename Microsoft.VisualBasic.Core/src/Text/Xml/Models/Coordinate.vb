#Region "Microsoft.VisualBasic::e437f1e97ae03e2ffad19a618184450d, Microsoft.VisualBasic.Core\src\Text\Xml\Models\Coordinate.vb"

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

    '   Total Lines: 69
    '    Code Lines: 50
    ' Comment Lines: 6
    '   Blank Lines: 13
    '     File Size: 2.45 KB


    '     Structure Coordinate
    ' 
    '         Properties: ID, X, Y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Text.Xml.Models

    ''' <summary>
    ''' Improvements on the xml format layout compare with <see cref="PointF"/> type.
    ''' </summary>
    Public Structure Coordinate : Implements ILayoutCoordinate

        ' 2017-6-22
        ' 因为double类型可以兼容Integer类型，所以在这里改为double类型
        ' 所以从pointf构建可以不再经过转换了

        <XmlAttribute("x")> Public Property X As Double Implements ILayoutCoordinate.X
        <XmlAttribute("y")> Public Property Y As Double Implements ILayoutCoordinate.Y
        <XmlAttribute>
        Public Property ID As String Implements ILayoutCoordinate.ID

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(pt As Point)
            Call Me.New(pt.X, pt.Y)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(pt As PointF)
            Call Me.New(pt.X, pt.Y)
        End Sub

        Sub New(x#, y#)
            Me.X = x
            Me.Y = y
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X}, {Y}]"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(c As Coordinate, pt As Point) As Boolean
            Return c.X = pt.X AndAlso c.Y = pt.Y
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(c As Coordinate, pt As Point) As Boolean
            Return Not c = pt
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(pt As Point) As Coordinate
            Return New Coordinate With {
                .X = pt.X,
                .Y = pt.Y
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(x As Coordinate) As Point
            Return New Point(x.X, x.Y)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(pt As Integer()) As Coordinate
            Return New Coordinate(pt.FirstOrDefault, pt.LastOrDefault)
        End Operator
    End Structure
End Namespace

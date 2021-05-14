#Region "Microsoft.VisualBasic::bc27bc9c60f7442636caa239ab5ce94f, Microsoft.VisualBasic.Core\src\Extensions\Image\Math\Models\Polygon.vb"

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

    '     Structure Polygon
    ' 
    '         Properties: Length
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Imaging.Math2D

    Public Structure Polygon : Implements IEnumerable(Of PointF)

        Dim Points As PointF()

        Public ReadOnly Property Length() As Integer
            Get
                Return Points.Length
            End Get
        End Property

        Default Public Property Item(index As Integer) As PointF
            Get
                Return Points(index)
            End Get
            Set
                Points(index) = Value
            End Set
        End Property

        Public Sub New(points As PointF())
            Me.Points = points
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(rect As Rectangle)
            Call Me.New(New RectangleF(rect.Left, rect.Top, rect.Width, rect.Height))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rect">
        ''' 四个顶点是具有前后顺序的，按照顺序构建出一个四方形
        ''' </param>
        Sub New(rect As RectangleF)
            Call Me.New({
                New PointF(rect.Left, rect.Top),
                New PointF(rect.Right, rect.Top),
                New PointF(rect.Right, rect.Bottom),
                New PointF(rect.Left, rect.Bottom)
            })
        End Sub

        Public Shared Widening Operator CType(polygon As Polygon) As PointF()
            Return polygon.Points
        End Operator

        Public Shared Widening Operator CType(points As PointF()) As Polygon
            Return New Polygon(points)
        End Operator

        Private Function IEnumerable_GetEnumerator() As IEnumerator(Of PointF) Implements IEnumerable(Of PointF).GetEnumerator
            Return DirectCast(Points.GetEnumerator(), IEnumerator(Of PointF))
        End Function

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Points.GetEnumerator()
        End Function
    End Structure
End Namespace

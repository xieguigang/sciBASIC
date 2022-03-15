#Region "Microsoft.VisualBasic::b63cce605f44a24371200da47f0df874, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\Orthogonal\Math.vb"

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

    '   Total Lines: 85
    '    Code Lines: 59
    ' Comment Lines: 17
    '   Blank Lines: 9
    '     File Size: 3.63 KB


    '     Module Math
    ' 
    '         Function: distance, GridCellSize, height, width
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports stdNum = System.Math

Namespace Layouts.Orthogonal

    ''' <summary>
    ''' The algorithm math helper module
    ''' </summary>
    <HideModuleName>
    Public Module Math

        Public Function distance(vi As Node, vj As Node, c#, delta#) As Double
            Dim xic = vi.data.initialPostion.x + 1 / 2 * vi.data.size(0)
            Dim xjc = vj.data.initialPostion.x + 1 / 2 * vj.data.size(0)
            Dim yic = vi.data.initialPostion.y + 1 / 2 * vi.data.size(1)
            Dim yjc = vj.data.initialPostion.y + 1 / 2 * vj.data.size(1)
            Dim ed = GeomTransform.Distance(
                vi.data.initialPostion.x, vi.data.initialPostion.y,
                vj.data.initialPostion.x, vj.data.initialPostion.y
            )
            Dim wi = vi.width(c, delta)
            Dim wj = vj.width(c, delta)
            Dim hi = vi.height(c, delta)
            Dim hj = vj.height(c, delta)

            Return ed + 1 / 20 * stdNum.Min(stdNum.Abs(xic - xjc) / (wi + wj), stdNum.Abs(yic - yjc) / (hi + hj))
        End Function

        ' When nodes are placed in the grid, they are given integer coordinates and sizes. 
        ' The top-left corner Of a node vi In the grid will be denoted by (x′i, y′i). 
        ' Its width in the grid w′i Is calculated as ⌈(wi+d)/c⌉. 
        ' Its height in the grid h′i Is calculated as ⌈(hi+d)/c⌉.

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function width(node As Node, c#, delta#) As Double
            Return (node.data.size(0) + delta) / c
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function height(node As Node, c#, delta#) As Double
            Return (node.data.size(1) + delta) / c
        End Function

        ''' <summary>
        ''' 在算法中单元格为正方形单元格，利用这个函数计算出单元格的最合适的大小
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' To deal with nodes of different sizes (relevant only in the second stage of the
        ''' algorithm) we need to calculate the size of a grid cell. We assume the grid cell
        ''' to be a square of side length c which Is calculated as this function.
        ''' </remarks>
        ''' 
        <Extension>
        Public Function GridCellSize(nodes As IEnumerable(Of Node)) As Double
            Dim nodeVector = nodes.ToArray
            Dim w = nodeVector.Select(Function(n) n.data.size(0)).ToArray
            Dim h As Double() = nodeVector _
                .Select(Function(n)
                            If n.data.size.Length < 2 Then
                                Return n.data.size(Scan0)
                            Else
                                Return n.data.size(1)
                            End If
                        End Function) _
                .ToArray
            Dim Lmin = stdNum.Min(w.Min, h.Min)
            Dim Lmax = stdNum.Max(w.Max, h.Max)

            If Lmax < 3 * Lmin Then
                Return Lmax
            ElseIf 3 * Lmin <= Lmax AndAlso Lmax < 15 * Lmin Then
                Return 3 * Lmin / 2
            ElseIf 15 * Lmin <= Lmax Then
                Return Lmax / 30
            Else
                Throw New NotImplementedException
            End If
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::5f52e114438a78ca9db1048040ae5e44, gr\network-visualization\Visualizer\MingleRender\RenderHelpers.vb"

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

    '   Total Lines: 106
    '    Code Lines: 82
    ' Comment Lines: 10
    '   Blank Lines: 14
    '     File Size: 3.79 KB


    ' Module RenderHelpers
    ' 
    '     Function: cloneEdge, createPosItem
    ' 
    '     Sub: expandEdgesHelper, expandEdgesRichHelper, unshift
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling.Mingle
Imports Microsoft.VisualBasic.Linq
Imports number = System.Double

Module RenderHelpers

    Friend Function cloneEdge(json As PosItem()) As PosItem()
        Dim l = json.Length, ans = New PosItem(json.Length - 1) {}
        For i As Integer = 0 To l - 1
            ans(i) = New PosItem With {
            .node = json(i).node,
            .pos = json(i).pos,
            .normal = json(i).normal.SafeQuery.ToArray
        }
        Next
        Return ans
    End Function

    Private Function createPosItem(node As Node, pos As Double(), Index As Integer, total As Double) As PosItem
        Return New PosItem With {
     .node = node,
     .pos = pos,
      .normal = Nothing
    }
    End Function

    ''' <summary>
    ''' Extend generic Graph class with bundle methods And rendering options
    ''' </summary>
    ''' <param name="node"></param>
    Friend Sub expandEdgesHelper(node As Node, Array As List(Of number()), collect As List(Of number()()))
        Dim coords = DirectCast(node.data, MingleNodeData).coords
        Dim ps As Node()

        If Array.IsNullOrEmpty Then
            Array.Add({(coords(0) + coords(2)) / 2,
        (coords(1) + coords(3)) / 2})
        End If

        Array.unshift({coords(0), coords(1)})
        Array.Add({coords(2), coords(3)})
        ps = DirectCast(node.data, MingleNodeData).parents
        If Not ps.IsNullOrEmpty Then
            For i As Integer = 0 To ps.Length - 1
                expandEdgesHelper(ps(i), Array.ToList, collect)
            Next
        Else
            collect.Add(Array.ToArray)
        End If
    End Sub

    ''' <summary>
    ''' Extend generic Graph class with bundle methods And rendering options
    ''' </summary>
    ''' <param name="node"></param>
    ''' <param name="Array"></param>
    ''' <param name="collect"></param>
    Friend Sub expandEdgesRichHelper(node As Node, Array As PosItem(), collect As PosItem()())
        Dim coords = DirectCast(node.data, MingleNodeData).coords
        Dim l As Integer, p As Double()
        Dim ps As Node() = DirectCast(node.data, MingleNodeData).parents
        Dim a As List(Of PosItem)
        Dim posItem As PosItem

        If Not ps.IsNullOrEmpty Then
            l = ps.Length

            For i As Integer = 0 To ps.Length - 1
                a = Array.ToList
                If a.Count <> 0 Then
                    p = {(coords(0) + coords(2)) / 2, (coords(1) + coords(3)) / 2}
                    posItem = createPosItem(node, p, i, l)
                    a.Add(posItem)
                End If

                posItem = createPosItem(node, {coords(0), coords(1)}, i, l)
                a.unshift(posItem)
                posItem = createPosItem(node, {coords(2), coords(3)}, i, l)
                a.Add(posItem)

                expandEdgesRichHelper(ps(i), a.ToArray, collect)
            Next
        Else
            a = Array.ToList
            If a.Count <> 0 Then
                p = {(coords(0) + coords(2)) / 2, (coords(1) + coords(3)) / 2}
                posItem = createPosItem(node, p, 0, 1)
                a.Add(posItem)
            End If

            posItem = createPosItem(node, {coords(0), coords(1)}, 0, 1)
            a.unshift(posItem)
            posItem = createPosItem(node, {coords(2), coords(3)}, 0, 1)
            a.Add(posItem)

            collect.Add(a.ToArray)
        End If
    End Sub

    <Extension>
    Private Sub unshift(Of T)(list As List(Of T), x As T)
        Call list.Insert(Scan0, x)
    End Sub
End Module

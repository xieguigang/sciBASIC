#Region "Microsoft.VisualBasic::a778b3ef61208e0dc64c8bbfb10ab93c, gr\network-visualization\network_layout\Orthogonal\optimization\OrthographicEmbeddingBoardSizeOptimizer.vb"

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

    '   Total Lines: 147
    '    Code Lines: 115 (78.23%)
    ' Comment Lines: 16 (10.88%)
    '    - Xml Docs: 18.75%
    ' 
    '   Blank Lines: 16 (10.88%)
    '     File Size: 5.62 KB


    '     Class OrthographicEmbeddingBoardSizeOptimizer
    ' 
    '         Function: findPathToTheRight, (+2 Overloads) optimize, removeAHorizontalPath
    ' 
    '         Sub: removePath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.util
Imports std = System.Math

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 
Namespace Orthogonal.optimization

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class OrthographicEmbeddingBoardSizeOptimizer
        Public Shared DEBUG As Integer = 0


        Public Shared Function optimize(o As OrthographicEmbeddingResult, graph As Integer()()) As OrthographicEmbeddingResult
            Return optimize(o, graph, New SegmentLengthEmbeddingComparator())
        End Function


        Public Shared Function optimize(o As OrthographicEmbeddingResult, graph As Integer()(), comparator As EmbeddingComparator) As OrthographicEmbeddingResult
            o.gridAlign(1)
            While removeAHorizontalPath(o, graph, comparator)
            End While
            Return o
        End Function


        Public Shared Function removeAHorizontalPath(o As OrthographicEmbeddingResult, graph As Integer()(), comparator As EmbeddingComparator) As Boolean

            ' 1) construct the board:
            Dim dx = 0
            Dim dy = 0
            For i = 0 To o.x.Length - 1
                If o.x(i) > dx Then
                    dx = CInt(o.x(i))
                End If
                If o.y(i) > dy Then
                    dy = CInt(o.y(i))
                End If
            Next
            dx += 1
            dy += 1

            Dim map = RectangularArray.Matrix(Of Integer)(dx, dy)

            For i = 0 To o.x.Length - 1
                map(o.x(i))(o.y(i)) = 1
                For j = 0 To o.x.Length - 1
                    If o.edges(i)(j) OrElse o.edges(j)(i) Then
                        ' draw path:
                        Dim x As Integer = o.x(i)
                        Dim y As Integer = o.y(i)
                        If x = CInt(o.x(j)) Then
                            Continue For
                        End If
                        While x <> CInt(o.x(j))
                            map(x)(y) = 1
                            If x < CInt(o.x(j)) Then
                                x += 1
                            End If
                            If x > CInt(o.x(j)) Then
                                x -= 1
                            End If
                        End While
                    End If
                Next
            Next
            If DEBUG >= 1 Then
                Console.WriteLine("")
                For i = 0 To dy - 1
                    For j = 0 To dx - 1
                        Console.Write(map(j)(i))
                    Next
                    Console.WriteLine("")
                Next
            End If

            ' 2) find a path of 0s from left to right:
            Dim path = findPathToTheRight(map, 0, dy - 1, 0)
            If path IsNot Nothing Then
                removePath(o, path)
                Return True
            End If

            Return False
        End Function

        Private Shared Function findPathToTheRight(map As Integer()(), y1 As Integer, y2 As Integer, x As Integer) As Pair(Of Integer, IList(Of Integer))
            If x = map.Length Then
                Return New Pair(Of Integer, IList(Of Integer))(map(0).Length, New List(Of Integer)())
            End If
            Dim last0 = -1
            While y1 > 0 AndAlso map(x)(y1 - 1) = 0
                y1 -= 1
            End While
            While y2 < map(0).Length - 1 AndAlso map(x)(y2 + 1) = 0
                y2 += 1
            End While
            For i = y1 To y2 + 1 - 1
                If map(x)(i) = 1 Then
                    If last0 <> -1 Then
                        ' found one from last0 -> i-1:
                        Dim path = findPathToTheRight(map, last0, i - 1, x + 1)
                        If path IsNot Nothing Then
                            path.m_a = std.Min(path.m_a, i - 1 - last0 + 1)
                            path.m_b.Insert(0, last0)
                            Return path
                        End If
                        last0 = -1
                    End If
                Else
                    If last0 = -1 Then
                        last0 = i
                    End If
                End If
            Next
            If last0 <> -1 Then
                ' found one from last0 -> i-1:
                Dim path = findPathToTheRight(map, last0, y2, x + 1)
                If path IsNot Nothing Then
                    path.m_a = std.Min(path.m_a, y2 - last0 + 1)
                    path.m_b.Insert(0, last0)
                    Return path
                End If
            End If
            Return Nothing
        End Function

        Private Shared Sub removePath(o As OrthographicEmbeddingResult, path As Pair(Of Integer, IList(Of Integer)))
            For x = 0 To path.m_b.Count - 1
                For j = 0 To o.y.Length - 1
                    If CInt(o.x(j)) = x AndAlso CInt(o.y(j)) >= path.m_b(x) Then
                        o.y(j) -= path.m_a
                    End If
                Next
            Next
        End Sub
    End Class

End Namespace

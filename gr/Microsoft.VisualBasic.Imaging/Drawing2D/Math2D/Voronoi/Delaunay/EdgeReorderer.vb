#Region "Microsoft.VisualBasic::fd000eaa42bff7e7acf74c7987c32c3c, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Delaunay\EdgeReorderer.vb"

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

    '   Total Lines: 115
    '    Code Lines: 103 (89.57%)
    ' Comment Lines: 1 (0.87%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (9.57%)
    '     File Size: 4.31 KB


    '     Class EdgeReorderer
    ' 
    '         Properties: EdgeOrientations, Edges
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ReorderEdges
    ' 
    '         Sub: Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class EdgeReorderer

        Private edgesField As List(Of Edge)
        Private edgeOrientationsField As List(Of LR)

        Public ReadOnly Property Edges As List(Of Edge)
            Get
                Return edgesField
            End Get
        End Property
        Public ReadOnly Property EdgeOrientations As List(Of LR)
            Get
                Return edgeOrientationsField
            End Get
        End Property

        Public Sub New(origEdges As List(Of Edge), criterion As Type)
            edgesField = New List(Of Edge)()
            edgeOrientationsField = New List(Of LR)()
            If origEdges.Count > 0 Then
                edgesField = ReorderEdges(origEdges, criterion)
            End If
        End Sub

        Public Sub Dispose()
            edgesField = Nothing
            edgeOrientationsField = Nothing
        End Sub

        Private Function ReorderEdges(origEdges As List(Of Edge), criterion As Type) As List(Of Edge)
            Dim i As Integer
            Dim n = origEdges.Count
            Dim edge As Edge
            ' We're going to reorder the edges in order of traversal
            Dim done As List(Of Boolean) = New List(Of Boolean)()
            Dim nDone = 0
            For b = 0 To n - 1
                done.Add(False)
            Next
            Dim newEdges As List(Of Edge) = New List(Of Edge)()

            i = 0
            edge = origEdges(i)
            newEdges.Add(edge)
            edgeOrientationsField.Add(LR.LEFT)
            Dim firstPoint As ICoord
            Dim lastPoint As ICoord
            If criterion Is GetType(Vertex) Then
                firstPoint = edge.LeftVertex
                lastPoint = edge.RightVertex
            Else
                firstPoint = edge.LeftSite
                lastPoint = edge.RightSite
            End If

            If firstPoint Is Vertex.VERTEX_AT_INFINITY OrElse lastPoint Is Vertex.VERTEX_AT_INFINITY Then
                Return New List(Of Edge)()
            End If

            done(i) = True
            nDone += 1

            While nDone < n
                For i = 1 To n - 1
                    If done(i) Then
                        Continue For
                    End If
                    edge = origEdges(i)
                    Dim leftPoint As ICoord
                    Dim rightPoint As ICoord
                    If criterion Is GetType(Vertex) Then
                        leftPoint = edge.LeftVertex
                        rightPoint = edge.RightVertex
                    Else
                        leftPoint = edge.LeftSite
                        rightPoint = edge.RightSite
                    End If
                    If leftPoint Is Vertex.VERTEX_AT_INFINITY OrElse rightPoint Is Vertex.VERTEX_AT_INFINITY Then
                        Return New List(Of Edge)()
                    End If
                    If leftPoint Is lastPoint Then
                        lastPoint = rightPoint
                        edgeOrientationsField.Add(LR.LEFT)
                        newEdges.Add(edge)
                        done(i) = True
                    ElseIf rightPoint Is firstPoint Then
                        firstPoint = leftPoint
                        edgeOrientationsField.Insert(0, LR.LEFT)
                        newEdges.Insert(0, edge)
                        done(i) = True
                    ElseIf leftPoint Is firstPoint Then
                        firstPoint = rightPoint
                        edgeOrientationsField.Insert(0, LR.RIGHT)
                        newEdges.Insert(0, edge)
                        done(i) = True
                    ElseIf rightPoint Is lastPoint Then
                        lastPoint = leftPoint
                        edgeOrientationsField.Add(LR.RIGHT)
                        newEdges.Add(edge)
                        done(i) = True
                    End If
                    If done(i) Then
                        nDone += 1
                    End If
                Next
            End While
            Return newEdges
        End Function
    End Class
End Namespace

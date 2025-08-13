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

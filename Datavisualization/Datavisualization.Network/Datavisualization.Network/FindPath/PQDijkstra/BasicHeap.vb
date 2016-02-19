'by Tolga Birdal

Imports System.Collections
Imports System.Collections.Generic
Imports System.Text

Namespace Dijkstra.PQDijkstra

    Public Class BasicHeap

        Dim InnerList As New List(Of HeapNode)()

        Public Sub New()
        End Sub

        Public ReadOnly Property Count() As Integer
            Get
                Return InnerList.Count
            End Get
        End Property

        Public Sub Push(index As Integer, weight As Single)
            Dim p As Integer = InnerList.Count, p2 As Integer
            InnerList.Add(New HeapNode(index, weight))
            ' E[p] = O
            Do
                If p = 0 Then
                    Exit Do
                End If

                p2 = (p - 1) >> 1
                If InnerList(p).weight < InnerList(p2).weight Then
                    Dim h As HeapNode = InnerList(p)
                    InnerList(p) = InnerList(p2)
                    InnerList(p2) = h

                    p = p2
                Else
                    Exit Do
                End If
            Loop While True
        End Sub

        Public Function Pop() As Integer
            Dim result As HeapNode = InnerList(0)
            Dim p As Integer = 0, p1 As Integer, p2 As Integer, pn As Integer
            InnerList(0) = InnerList(InnerList.Count - 1)
            InnerList.RemoveAt(InnerList.Count - 1)
            Do
                pn = p
                p1 = (p << 1) + 1
                p2 = (p << 1) + 2
                If InnerList.Count > p1 AndAlso InnerList(p).weight > InnerList(p1).weight Then
                    p = p1
                End If
                If InnerList.Count > p2 AndAlso InnerList(p).weight > InnerList(p2).weight Then
                    p = p2
                End If

                If p = pn Then
                    Exit Do
                End If

                Dim h As HeapNode = InnerList(p)
                InnerList(p) = InnerList(pn)

                InnerList(pn) = h
            Loop While True
            Return result.index
        End Function
    End Class
End Namespace
Namespace DownSampling.LargestTriangleBucket


    ''' <summary>
    ''' Tow-way linked list to perform bucket split and merge.
    ''' </summary>
    Public Class LinkedBucketNode

        Private last_Conflict As LinkedBucketNode
        Private next_Conflict As LinkedBucketNode
        Private end_Conflict As LinkedBucketNode
        Private value_Conflict As LTWeightedBucket
        Private size As Integer

        ''' <summary>
        ''' split this node into 2 new nodes,each contains a new bucket with half events.
        ''' </summary>
        ''' <returns> if bucket contains more than 2 events, return the last node, else return this. </returns>
        Public Overridable Function split() As LinkedBucketNode
            Dim size As Integer = value_Conflict.size()
            If size < 2 Then
                Return Me
            End If
            Dim b0 As New LTWeightedBucket(size \ 2)
            Dim b1 As New LTWeightedBucket(size - size \ 2)
            For i As Integer = 0 To size - 1
                Call (If(i < size \ 2, b0, b1)).add(value_Conflict.get(i))
            Next i
            Dim n0 As New LinkedBucketNode(b0)
            Dim n1 As New LinkedBucketNode(b1)
            replace(Me, n0)
            insert(n0, n1)
            Return n1
        End Function

        ''' <summary>
        ''' merge this node and the next node into one.
        ''' </summary>
        ''' <returns> the merged node; </returns>
        Public Overridable Function merge() As LinkedBucketNode
            If next_Conflict Is Nothing Then
                Return Me
            End If
            Dim m As New LTWeightedBucket(value_Conflict.size() + next_Conflict.Value.size())
            For Each e As WeightedEvent In value_Conflict
                m.add(e)
            Next e
            For Each e As WeightedEvent In next_Conflict.Value
                m.add(e)
            Next e
            Dim n As New LinkedBucketNode(m)
            Dim tail As LinkedBucketNode = next_Conflict.Next
            concat(last_Conflict, n, tail)
            Return n
        End Function

        Public Shared Function fromList(arr As IList(Of LTWeightedBucket)) As LinkedBucketNode
            Dim head As New LinkedBucketNode(arr.Count)
            Dim last As LinkedBucketNode = head
            For i As Integer = 0 To arr.Count - 1
                Dim node As New LinkedBucketNode(arr(i))
                head.end_Conflict = node
                node.last_Conflict = last
                last.next_Conflict = node
                last = node
            Next i
            Return head
        End Function

        Public Shared Function toList(head As LinkedBucketNode) As IList(Of LTWeightedBucket)
            Dim arr As IList(Of LTWeightedBucket) = New List(Of LTWeightedBucket)(head.size)
            Dim node As LinkedBucketNode = head.next_Conflict
            Do While node IsNot Nothing
                arr.Add(node.value_Conflict)
                node = node.next_Conflict
            Loop
            Return arr
        End Function

        Public Shared Function fromArray(arr() As LTWeightedBucket) As LinkedBucketNode
            Dim head As New LinkedBucketNode(arr.Length)
            Dim last As LinkedBucketNode = head
            For i As Integer = 0 To arr.Length - 1
                Dim node As New LinkedBucketNode(arr(i))
                head.end_Conflict = node
                node.last_Conflict = last
                last.next_Conflict = node
                last = node
            Next i
            Return head
        End Function

        Public Shared Function toArray(head As LinkedBucketNode) As LTWeightedBucket()
            Dim arr(head.size - 1) As LTWeightedBucket
            Dim node As LinkedBucketNode = head.next_Conflict
            Dim index As Integer = 0
            Do While node IsNot Nothing
                arr(index) = node.value_Conflict
                index += 1
                node = node.next_Conflict
            Loop
            Return arr
        End Function

        Public Shared Sub insert(node As LinkedBucketNode, append As LinkedBucketNode)
            Dim [next] As LinkedBucketNode = node.next_Conflict
            node.next_Conflict = append
            append.last_Conflict = node
            append.next_Conflict = [next]
            If [next] IsNot Nothing Then
                [next].last_Conflict = append
            End If
        End Sub

        Public Shared Sub replace(node As LinkedBucketNode, rep As LinkedBucketNode)
            Dim [next] As LinkedBucketNode = node.next_Conflict
            Dim last As LinkedBucketNode = node.last_Conflict
            node.last_Conflict = Nothing
            node.next_Conflict = Nothing
            last.next_Conflict = rep
            rep.last_Conflict = last
            rep.next_Conflict = [next]
            If [next] IsNot Nothing Then
                [next].last_Conflict = rep
            End If
        End Sub

        Public Shared Sub concat(head As LinkedBucketNode, node As LinkedBucketNode, tail As LinkedBucketNode)
            head.next_Conflict = node
            node.last_Conflict = head
            node.next_Conflict = tail
            If tail IsNot Nothing Then
                tail.last_Conflict = node
            End If
        End Sub

        Public Sub New(size As Integer)
            Me.size = size
        End Sub

        Public Sub New(b As LTWeightedBucket)
            value_Conflict = b
        End Sub

        Public Overridable ReadOnly Property [End] As LinkedBucketNode
            Get
                Return end_Conflict
            End Get
        End Property

        Public Overridable Property Last As LinkedBucketNode
            Get
                Return last_Conflict
            End Get
            Set(last As LinkedBucketNode)
                Me.last_Conflict = last
            End Set
        End Property


        Public Overridable Property [Next] As LinkedBucketNode
            Get
                Return next_Conflict
            End Get
            Set([next] As LinkedBucketNode)
                Me.next_Conflict = [next]
            End Set
        End Property


        Public Overridable Property Value As LTWeightedBucket
            Get
                Return value_Conflict
            End Get
            Set(value As LTWeightedBucket)
                Me.value_Conflict = value
            End Set
        End Property

    End Class

End Namespace
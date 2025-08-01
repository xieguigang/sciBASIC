#Region "Microsoft.VisualBasic::c1214b14610866106bf30cf519f67e9c, Data_science\Mathematica\Math\Math\DownSampling\LargestTriangleBucket\LinkedBucketNode.vb"

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

    '   Total Lines: 150
    '    Code Lines: 120 (80.00%)
    ' Comment Lines: 11 (7.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (12.67%)
    '     File Size: 5.35 KB


    '     Class LinkedBucketNode
    ' 
    '         Properties: [End], [Next], Last, Value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: fromArray, fromList, merge, split, toArray
    '                   toList
    ' 
    '         Sub: concat, insert, replace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace DownSampling.LargestTriangleBucket


    ''' <summary>
    ''' Tow-way linked list to perform bucket split and merge.
    ''' </summary>
    Public Class LinkedBucketNode

        Private size As Integer

        ''' <summary>
        ''' split this node into 2 new nodes,each contains a new bucket with half events.
        ''' </summary>
        ''' <returns> if bucket contains more than 2 events, return the last node, else return this. </returns>
        Public Overridable Function split() As LinkedBucketNode
            Dim size As Integer = Value.size()
            If size < 2 Then
                Return Me
            End If
            Dim b0 As New LTWeightedBucket(size \ 2)
            Dim b1 As New LTWeightedBucket(size - size \ 2)
            For i As Integer = 0 To size - 1
                Call (If(i < size \ 2, b0, b1)).add(Value.get(i))
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
            If [Next] Is Nothing Then
                Return Me
            End If
            Dim m As New LTWeightedBucket(Value.size() + [Next].Value.size())
            For Each e As WeightedEvent In Value
                m.add(e)
            Next e
            For Each e As WeightedEvent In [Next].Value
                m.add(e)
            Next e
            Dim n As New LinkedBucketNode(m)
            Dim tail As LinkedBucketNode = [Next].Next
            concat(Last, n, tail)
            Return n
        End Function

        Public Shared Function fromList(arr As IList(Of LTWeightedBucket)) As LinkedBucketNode
            Dim head As New LinkedBucketNode(arr.Count)
            Dim last As LinkedBucketNode = head
            For i As Integer = 0 To arr.Count - 1
                Dim node As New LinkedBucketNode(arr(i))
                head._End = node
                node.Last = last
                last.Next = node
                last = node
            Next i
            Return head
        End Function

        Public Shared Function toList(head As LinkedBucketNode) As IList(Of LTWeightedBucket)
            Dim arr As New List(Of LTWeightedBucket)(head.size)
            Dim node As LinkedBucketNode = head.Next
            Do While node IsNot Nothing
                arr.Add(node.Value)
                node = node.Next
            Loop
            Return arr
        End Function

        Public Shared Function fromArray(arr() As LTWeightedBucket) As LinkedBucketNode
            Dim head As New LinkedBucketNode(arr.Length)
            Dim last As LinkedBucketNode = head
            For i As Integer = 0 To arr.Length - 1
                Dim node As New LinkedBucketNode(arr(i))
                head._End = node
                node.Last = last
                last.Next = node
                last = node
            Next i
            Return head
        End Function

        Public Shared Function toArray(head As LinkedBucketNode) As LTWeightedBucket()
            Dim arr(head.size - 1) As LTWeightedBucket
            Dim node As LinkedBucketNode = head.Next
            Dim index As Integer = 0
            Do While node IsNot Nothing
                arr(index) = node.Value
                index += 1
                node = node.Next
            Loop
            Return arr
        End Function

        Public Shared Sub insert(node As LinkedBucketNode, append As LinkedBucketNode)
            Dim [next] As LinkedBucketNode = node.Next
            node.Next = append
            append.Last = node
            append.Next = [next]
            If [next] IsNot Nothing Then
                [next].Last = append
            End If
        End Sub

        Public Shared Sub replace(node As LinkedBucketNode, rep As LinkedBucketNode)
            Dim [next] As LinkedBucketNode = node.Next
            Dim last As LinkedBucketNode = node.Last

            node.Last = Nothing
            node.Next = Nothing
            last.Next = rep
            rep.Last = last
            rep.Next = [next]

            If [next] IsNot Nothing Then
                [next].Last = rep
            End If
        End Sub

        Public Shared Sub concat(head As LinkedBucketNode, node As LinkedBucketNode, tail As LinkedBucketNode)
            head.Next = node
            node.Last = head
            node.Next = tail
            If tail IsNot Nothing Then
                tail.Last = node
            End If
        End Sub

        Public Sub New(size As Integer)
            Me.size = size
        End Sub

        Public Sub New(b As LTWeightedBucket)
            Value = b
        End Sub

        Public Overridable ReadOnly Property [End] As LinkedBucketNode
        Public Overridable Property Last As LinkedBucketNode
        Public Overridable Property [Next] As LinkedBucketNode
        Public Overridable Property Value As LTWeightedBucket

    End Class

End Namespace

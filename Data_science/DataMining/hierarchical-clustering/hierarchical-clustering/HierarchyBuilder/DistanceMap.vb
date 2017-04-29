Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Hierarchy


    ''' <summary>
    ''' Container for linkages
    ''' with the minimal methods needed in the package
    ''' Created by Alexandre Masselot on 7/18/14.
    ''' </summary>
    Public Class DistanceMap

        Private pairHash As Dictionary(Of String, Item)
        Private data As PriorityQueue(Of Item)

        Private Class Item
            Implements IComparable(Of Item)
            Implements IComparable

            Private ReadOnly outerInstance As DistanceMap

            Friend ReadOnly pair As HierarchyTreeNode
            Friend ReadOnly hash As String
            Friend removed As Boolean = False

            Friend Sub New(outerInstance As DistanceMap, p As HierarchyTreeNode)
                Me.outerInstance = outerInstance
                pair = p
                hash = outerInstance.hashCodePair(p)
            End Sub

            Public Function compareTo(o As Item) As Integer Implements IComparable(Of Item).CompareTo
                Return pair.compareTo(o.pair)
            End Function

            Public Overrides Function ToString() As String
                Return hash
            End Function

            Private Function __compareTo(obj As Object) As Integer Implements IComparable.CompareTo
                Return compareTo(obj)
            End Function
        End Class

        Public Sub New()
            data = New PriorityQueue(Of Item)
            pairHash = New Dictionary(Of String, Item)
        End Sub

        Public Function list() As IList(Of HierarchyTreeNode)
            Dim l As IList(Of HierarchyTreeNode) = New List(Of HierarchyTreeNode)
            For Each clusterPair As Item In data
                l.Add(clusterPair.pair)
            Next clusterPair
            Return l
        End Function

        Public Function findByCodePair(c1 As Cluster, c2 As Cluster) As HierarchyTreeNode
            Dim inCode As String = hashCodePair(c1, c2)
            Return pairHash(inCode).pair
        End Function

        Public Function removeFirst() As HierarchyTreeNode
            Dim poll As Item = data.Dequeue
            Do While poll IsNot Nothing AndAlso poll.removed
                poll = data.Dequeue
            Loop
            If poll Is Nothing Then Return Nothing
            Dim link As HierarchyTreeNode = poll.pair
            pairHash.Remove(poll.hash)
            Return link
        End Function

        Public Function remove(link As HierarchyTreeNode) As Boolean
            Dim ___remove As Item = pairHash.RemoveAndGet(hashCodePair(link))
            If ___remove Is Nothing Then Return False
            ___remove.removed = True
            data.Remove(___remove)
            Return True
        End Function


        Public Function add(link As HierarchyTreeNode) As Boolean
            Dim e As New Item(Me, link)
            Dim existingItem As Item = pairHash.TryGetValue(e.hash)
            If existingItem IsNot Nothing Then
                Console.Error.WriteLine("hashCode = " & existingItem.hash & " adding redundant link:" & link.ToString & " (exist:" & existingItem.ToString & ")")
                Return False
            Else
                pairHash(e.hash) = e
                data.Enqueue(e)
                Return True
            End If
        End Function

        ''' <summary>
        ''' Peak into the minimum distance
        ''' @return
        ''' </summary>
        Public Function minDist() As Double
            Dim peek As Item = data.Peek()
            If peek IsNot Nothing Then
                Return peek.pair.LinkageDistance
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Compute some kind of unique ID for a given cluster pair. </summary>
        ''' <returns> The ID </returns>
        Friend Function hashCodePair(link As HierarchyTreeNode) As String
            Return hashCodePair(link.lCluster(), link.rCluster())
        End Function

        Friend Function hashCodePair(lCluster As Cluster, rCluster As Cluster) As String
            Return hashCodePairNames(lCluster.Name, rCluster.Name)
        End Function

        Friend Function hashCodePairNames(lName As String, rName As String) As String
            If lName.CompareTo(rName) < 0 Then
                Return lName & "~~~" & rName 'getlCluster().hashCode() + 31 * (getrCluster().hashCode());
            Else
                Return rName & "~~~" & lName 'return getrCluster().hashCode() + 31 * (getlCluster().hashCode());
            End If
        End Function

        Public Overloads Function ToString() As String
            Return data.ToString()
        End Function
    End Class

End Namespace
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Hierarchy

    Public Class HierarchyLink : Implements IComparable, IComparable(Of HierarchyLink), IReadOnlyId

        Private ReadOnly outerInstance As DistanceMap

        Friend ReadOnly pair As HierarchyTreeNode
        Friend removed As Boolean = False

        Public ReadOnly Property hash As String Implements IReadOnlyId.Identity

        Friend Sub New(outerInstance As DistanceMap, p As HierarchyTreeNode)
            Me.outerInstance = outerInstance
            pair = p
            hash = p.hashCodePair
        End Sub

        Public Function compareTo(o As HierarchyLink) As Integer Implements IComparable(Of HierarchyLink).CompareTo
            Return pair.compareTo(o.pair)
        End Function

        Public Overrides Function ToString() As String
            Return hash
        End Function

        Private Function __compareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Return compareTo(obj)
        End Function
    End Class

    Module LinkHashCode

        ''' <summary>
        ''' Compute some kind of unique ID for a given cluster pair. </summary>
        ''' <returns> The ID </returns>
        <Extension> Public Function hashCodePair(link As HierarchyTreeNode) As String
            Return hashCodePair(link.lCluster(), link.rCluster())
        End Function

        Public Function hashCodePair(lCluster As Cluster, rCluster As Cluster) As String
            Dim lName = lCluster.Name
            Dim rName = rCluster.Name

            If lName.CompareTo(rName) < 0 Then
                Return lName & "~~~" & rName ' getlCluster().hashCode() + 31 * (getrCluster().hashCode());
            Else
                Return rName & "~~~" & lName ' return getrCluster().hashCode() + 31 * (getlCluster().hashCode());
            End If
        End Function
    End Module
End Namespace
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Hierarchy

    Public Class HierarchyLink : Implements IComparable, IComparable(Of HierarchyLink), IReadOnlyId

        Public ReadOnly Tree As HierarchyTreeNode
        Public ReadOnly Property HashKey As String Implements IReadOnlyId.Identity

        Friend removed As Boolean = False

        Sub New(p As HierarchyTreeNode)
            Tree = p
            HashKey = p.hashCodePair
        End Sub

        Public Function compareTo(o As HierarchyLink) As Integer Implements IComparable(Of HierarchyLink).CompareTo
            Return Tree.compareTo(o.Tree)
        End Function

        Public Overrides Function ToString() As String
            Return HashKey
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
            Return hashCodePair(link.Left(), link.Right())
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
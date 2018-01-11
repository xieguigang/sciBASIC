Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ComponentModel.DataStructures.BinaryTree

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Add(Of T)(tree As BinaryTree(Of NamedValue(Of T)), node As NamedValue(Of T)) As TreeNode(Of NamedValue(Of T))
            Return tree.insert(node.Name, node)
        End Function
    End Module
End Namespace
Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    Public Module Enumerable

        ''' <summary>
        ''' Populate an ASC sortted sequence from this binary tree 
        ''' 
        ''' ```
        ''' left -> me -> right
        ''' ```
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="tree"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function PopulateSequence(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of Map(Of K, V))
            If Not tree.Left Is Nothing Then
                For Each node In tree.Left.PopulateSequence
                    Yield node
                Next
            End If

            Yield New Map(Of K, V)(tree.Key, tree.Value)

            If Not tree.Right Is Nothing Then
                For Each node In tree.Right.PopulateSequence
                    Yield node
                Next
            End If
        End Function
    End Module
End Namespace
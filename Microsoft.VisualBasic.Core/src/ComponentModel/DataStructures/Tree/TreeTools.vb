Imports System.Runtime.CompilerServices

Namespace ComponentModel.DataStructures.Tree

    ''' <summary>
    ''' extension method tools for abstract tree model
    ''' </summary>
    Public Module TreeTools

        ''' <summary>
        ''' enumerates all childs in current tree node
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="tree"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function EnumerateAllChilds(Of T As ITreeNodeData(Of T))(tree As ITreeNodeData(Of T)) As IEnumerable(Of T)
            If tree.ChildNodes Is Nothing Then
                Return
            End If

            For Each child As T In tree.ChildNodes
                Yield child

                For Each subchild As T In child.EnumerateAllChilds
                    Yield subchild
                Next
            Next
        End Function
    End Module
End Namespace
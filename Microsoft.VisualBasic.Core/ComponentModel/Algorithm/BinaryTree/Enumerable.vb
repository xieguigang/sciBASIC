#Region "Microsoft.VisualBasic::a7cb59891d0578f90f7f4ec63df91fd5, Microsoft.VisualBasic.Core\ComponentModel\Algorithm\BinaryTree\Enumerable.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module Enumerable
    ' 
    '         Function: PopulateNodes, PopulateSequence, (+2 Overloads) Values
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        ''' <remarks>
        ''' 这个函数是直接将键名和对应的值取出来，如果是需要取出聚类的簇成员，
        ''' 应该使用<see cref="PopulateNodes(Of K, V)"/>方法
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PopulateSequence(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of Map(Of K, V))
            Return tree.PopulateNodes.Select(Function(n) New Map(Of K, V)(n.Key, n.Value))
        End Function

        ''' <summary>
        ''' 将一个给定的二叉树对象转换为一个数组序列
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="tree"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function PopulateNodes(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of BinaryTree(Of K, V))
            If Not tree.Left Is Nothing Then
                For Each node In tree.Left.PopulateNodes
                    Yield node
                Next
            End If

            Yield tree

            If Not tree.Right Is Nothing Then
                For Each node In tree.Right.PopulateNodes
                    Yield node
                Next
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Values(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of V)
            Return tree.PopulateNodes.Values
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Values(Of K, V)(nodes As IEnumerable(Of BinaryTree(Of K, V))) As IEnumerable(Of V)
            Return nodes.Select(Function(n) n.Value)
        End Function
    End Module
End Namespace

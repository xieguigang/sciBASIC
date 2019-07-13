#Region "Microsoft.VisualBasic::5ea4bf16488c3938884e26b6f9cf6e76, Microsoft.VisualBasic.Core\ComponentModel\Algorithm\BinaryTree\TreeBase.vb"

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

    '     Class TreeBase
    ' 
    '         Properties: root
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetAllNodes
    ' 
    '         Sub: Clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    ''' <summary>
    ''' 二叉树对象的通用模板
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
    ''' <typeparam name="V"></typeparam>
    Public MustInherit Class TreeBase(Of K, V)

        ''' <summary>
        ''' The root node of this binary tree
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property root As BinaryTree(Of K, V)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _root
            End Get
        End Property

        Protected _root As BinaryTree(Of K, V)

        Protected ReadOnly compares As Comparison(Of K)
        Protected ReadOnly views As Func(Of K, String)
        Protected ReadOnly stack As New List(Of BinaryTree(Of K, V))

        ''' <summary>
        ''' Create an instance of the AVL binary tree.
        ''' </summary>
        ''' <param name="compares">Compare between two keys.</param>
        ''' <param name="views">Display the key as string</param>
        Sub New(compares As Comparison(Of K), Optional views As Func(Of K, String) = Nothing)
            Me.compares = compares
            Me.views = views
        End Sub

        ''' <summary>
        ''' 将整棵树销毁
        ''' </summary>
        Public Overridable Sub Clear()
            _root = Nothing
            stack.Clear()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetAllNodes() As IEnumerable(Of BinaryTree(Of K, V))
            Return stack.AsEnumerable
        End Function
    End Class
End Namespace

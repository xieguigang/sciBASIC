Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Java

Namespace train.kernels

    ''' <summary>
    ''' 行分区器：维护「行 -&gt; 节点」映射以及每个节点当前持有的行集合。
    '''
    ''' 旧实现每一层都要对全量样本做三遍扫描：
    ''' <c>update_corresponding_tree_node</c> / <c>update_Grad_Hess_numsample_for_tree_node</c> /
    ''' <c>update_grad_hess_missing_for_tree_node</c>。三次扫描本质上都只做一件事——
    ''' 沿树下降一层。本类把「下降一层」压缩成一次「只重排父节点行集合」的原地分区，
    ''' 于是每层的总代价从 <c>3N</c> 降为 <c>N</c>（且常数更小：只访问整数数组）。
    ''' </summary>
    Public Class RowPartitioner

        ''' <summary>
        ''' 行号 -&gt; 节点编号，<c>-1</c> 表示未分配
        ''' </summary>
        Public ReadOnly Slot As Integer()

        Private ReadOnly rowSets As New List(Of List(Of Integer))()

        Public Sub New(rowCount As Integer)
            Me.Slot = New Integer(rowCount - 1) {}

            For i As Integer = 0 To rowCount - 1
                Me.Slot(i) = -1
            Next
        End Sub

        ''' <summary>
        ''' 取得某个节点当前持有的行集合；未分配时返回空集合
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Function RowsOf(node As Integer) As List(Of Integer)
            Call ensure(node)

            Dim rows = rowSets(node)

            If rows Is Nothing Then
                rows = New List(Of Integer)()
                rowSets(node) = rows
            End If

            Return rows
        End Function

        ''' <summary>
        ''' 把全部行分配给根节点
        ''' </summary>
        ''' <param name="root"></param>
        Public Sub AssignAll(root As Integer)
            Dim rows As New List(Of Integer)(Slot.Length)

            For i As Integer = 0 To Slot.Length - 1
                Slot(i) = root
                Call rows.Add(i)
            Next

            Call ensure(root)
            rowSets(root) = rows
        End Sub

        ''' <summary>
        ''' 按分裂条件把父节点的行集合拆分为子节点。
        ''' </summary>
        ''' <param name="parent">父节点编号</param>
        ''' <param name="left">左子节点编号</param>
        ''' <param name="right">右子节点编号</param>
        ''' <param name="nanNode">缺失值子节点编号，<c>-1</c> 表示不存在独立缺失子节点</param>
        ''' <param name="assign">返回行所属子节点编号的判定函数</param>
        Public Sub Split(parent As Integer, left As Integer, right As Integer, nanNode As Integer,
                         assign As Func(Of Integer, Integer))

            Dim source = RowsOf(parent)

            If source.Count = 0 Then
                Call ensure(left)
                Call ensure(right)
                rowSets(left) = New List(Of Integer)()
                rowSets(right) = New List(Of Integer)()

                If nanNode >= 0 Then
                    Call ensure(nanNode)
                    rowSets(nanNode) = New List(Of Integer)()
                End If

                Return
            End If

            Dim leftRows As New List(Of Integer)(source.Count \ 2 + 1)
            Dim rightRows As New List(Of Integer)(source.Count \ 2 + 1)
            Dim nanRows As List(Of Integer) = If(nanNode >= 0, New List(Of Integer)(), Nothing)

            For Each row As Integer In source
                Dim child As Integer = assign(row)

                If child = left Then
                    Call leftRows.Add(row)
                ElseIf child = right Then
                    Call rightRows.Add(row)
                ElseIf nanNode >= 0 AndAlso child = nanNode Then
                    Call nanRows.Add(row)
                End If
            Next

            source.Clear()

            ' 重新绑定「行 -> 节点」
            For Each row As Integer In leftRows
                Slot(row) = left
            Next

            For Each row As Integer In rightRows
                Slot(row) = right
            Next

            If nanRows IsNot Nothing Then

                For Each row As Integer In nanRows
                    Slot(row) = nanNode
                Next
            End If

            Call ensure(left)
            Call ensure(right)

            rowSets(left) = leftRows
            rowSets(right) = rightRows

            If nanRows IsNot Nothing Then
                Call ensure(nanNode)
                rowSets(nanNode) = nanRows
            End If
        End Sub

        ''' <summary>
        ''' 已经出现过的最大节点编号。
        '''
        ''' 注意：叶节点会继续持有自己的行（用于回填「行 -&gt; 叶节点」），
        ''' 因此某一层的 <c>Slot</c> 取值可能**大于**本层存活节点的最大编号，
        ''' 分裂查找器必须按本属性来界定索引数组的长度。
        ''' </summary>
        Public ReadOnly Property MaxNodeId As Integer
            Get
                Return rowSets.Count - 1
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub ensure(node As Integer)

            While rowSets.Count <= node
                Call rowSets.Add(Nothing)
            End While
        End Sub
    End Class
End Namespace

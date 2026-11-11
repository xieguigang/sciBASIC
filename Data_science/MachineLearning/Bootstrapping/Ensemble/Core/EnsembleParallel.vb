Imports System

Namespace Ensemble

    ''' <summary>
    ''' 集成框架的并行调度工具：把「是否并行 + 串行回退」的成对循环收敛到一处。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 基模型训练与批量预测在两个维度上都可以并行（模型维 / 样本维），
    ''' 但并行与否必须由调用方显式控制：多数算法库内部已有批量并行，
    ''' 默认串行可以避免嵌套并行导致的线程饥饿。
    ''' </para>
    ''' <para>
    ''' 这里的并行只在元素数量大于 1 时启用，且每个回调只写入自己的下标槽位，
    ''' 因此不需要加锁。
    ''' </para>
    ''' </remarks>
    Public Module EnsembleParallel

        ''' <summary>
        ''' 并行/串行地训练全部基模型（每个回调只写自己的槽位，无共享可变状态）。
        ''' </summary>
        ''' <param name="count">基模型数量</param>
        ''' <param name="parallel">是否启用并行</param>
        ''' <param name="body">下标回调</param>
        Public Sub FitAll(count As Integer, parallel As Boolean, body As Action(Of Integer))
            Call RunAll(count, parallel, body)
        End Sub

        ''' <summary>并行/串行地对全部样本执行预测</summary>
        ''' <param name="count">样本数量</param>
        ''' <param name="parallel">是否启用并行</param>
        ''' <param name="body">下标回调</param>
        Public Sub PredictAll(count As Integer, parallel As Boolean, body As Action(Of Integer))
            Call RunAll(count, parallel, body)
        End Sub

        Private Sub RunAll(count As Integer, parallel As Boolean, body As Action(Of Integer))
            If body Is Nothing Then
                Throw New ArgumentNullException(NameOf(body))
            End If
            If count <= 0 Then
                Return
            End If

            If parallel AndAlso count > 1 Then
                Call System.Threading.Tasks.Parallel.For(0, count, body)
            Else
                For i As Integer = 0 To count - 1
                    Call body(i)
                Next
            End If
        End Sub

    End Module

End Namespace

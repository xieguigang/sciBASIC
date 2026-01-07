Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' 笛卡尔积是集合论中的一个基本概念，它描述了如何从两个或多个集合中生成所有可能的有序组合。
    ''' </summary>
    Public Module CartesianProduct

        ''' <summary>
        ''' 生成两个序列的两两组合 ``{<paramref name="seq_1"/> -> <paramref name="seq_2"/>}[]``
        ''' </summary>
        ''' <typeparam name="TA"></typeparam>
        ''' <typeparam name="TB"></typeparam>
        ''' <param name="seq_1"></param>
        ''' <param name="seq_2"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function CreateCombos(Of TA, TB)(seq_1 As IEnumerable(Of TA), seq_2 As IEnumerable(Of TB)) As IEnumerable(Of (a As TA, b As TB))
            Dim b As TB() = seq_2.ToArray

            For Each i As TA In seq_1
                For Each j As TB In b
                    Yield (i, j)
                Next
            Next
        End Function

        ''' <summary>
        ''' 生成三个序列的笛卡尔积（所有可能的三元组组合）
        ''' </summary>
        ''' <typeparam name="TA">第一个序列的元素类型</typeparam>
        ''' <typeparam name="TB">第二个序列的元素类型</typeparam>
        ''' <typeparam name="TC">第三个序列的元素类型</typeparam>
        ''' <param name="seq1">第一个序列</param>
        ''' <param name="seq2">第二个序列</param>
        ''' <param name="seq3">第三个序列</param>
        ''' <returns>包含所有可能三元组的序列</returns>
        <Extension>
        Public Iterator Function CreateTripleCartesianProduct(Of TA, TB, TC)(
            seq1 As IEnumerable(Of TA),
            seq2 As IEnumerable(Of TB),
            seq3 As IEnumerable(Of TC)) As IEnumerable(Of (First As TA, Second As TB, Third As TC))

            ' 将第二和第三个序列转换为数组，避免多次枚举
            Dim array2 As TB() = seq2.ToArray()
            Dim array3 As TC() = seq3.ToArray()

            ' 三重嵌套循环生成所有组合
            For Each item1 In seq1
                For Each item2 In array2
                    For Each item3 In array3
                        Yield (item1, item2, item3)
                    Next
                Next
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FullCombination(Of T)(seq As IEnumerable(Of T)) As IEnumerable(Of (a As T, b As T))
            Dim all = seq.ToArray
            Return all.CreateCombos(all)
        End Function
    End Module
End Namespace
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Mathematical.BasicR

Public Module Bootstraping

    ''' <summary>
    ''' bootstrap是一种非参数估计方法，它用到蒙特卡洛方法。bootstrap算法如下：
    ''' 假设样本容量为N
    '''
    ''' + 有放回的从样本中随机抽取N次(所以可能x1..xn中有的值会被抽取多次)，每次抽取一个元素。并将抽到的元素放到集合S中；
    ''' + 重复**步骤1** B次（例如``B = 100``）， 得到B个集合， 记作S1, S2,…, SB;
    ''' + 对每个Si （i=1,2,…,B），用蒙特卡洛方法估计随机变量的数字特征d，分别记作d1,d2,…,dB;
    ''' + 用d1,d2,…dB来近似d的分布；
    ''' 
    ''' 本质上，bootstrap算法是最大似然估计的一种实现，它和最大似然估计相比的优点在于，它不需要用参数来刻画总体分布。
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="N"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function Samples(Of T)(source As IEnumerable(Of T), N As Integer, Optional B As Integer = 100) As IEnumerable(Of IntegerTagged(Of T()))
        Dim array As T() = source.ToArray
        Dim rnd As New Random(Now.Millisecond)

        For i As Integer = 0 To B
            Dim ls As New List(Of T)

            For k As Integer = 0 To N
                ls += array(rnd.Next(array.Length))
            Next

            Yield New IntegerTagged(Of T()) With {
                .Tag = i,
                .value = ls.ToArray
            }
        Next
    End Function

    <Extension>
    Public Iterator Function Sampling(source As IEnumerable(Of Double), N As Integer, Optional B As Integer = 100) As IEnumerable(Of IntegerTagged(Of Vector))
        For Each x In Samples(source, N, B)
            Yield New IntegerTagged(Of Vector) With {
                .Tag = x.Tag,
                .value = New Vector(x.value)
            }
        Next
    End Function

    <Extension>
    Public Function Samples(Of T)(source As IEnumerable(Of T), getValue As Func(Of T, Double), N As Integer, Optional B As Integer = 100) As IEnumerable(Of IntegerTagged(Of Vector))
        Return source.Select(getValue).Sampling(N, B)
    End Function
End Module

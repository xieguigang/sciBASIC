
Imports System.Runtime.CompilerServices
''' <summary>
''' ### Fisher's exact test
''' 
''' > https://en.wikipedia.org/wiki/Fisher's_exact_test
''' </summary>
Public Module FisherTest

    Public Function FisherPvalue(a#, b#, c#, d#) As Double
        Dim sX = FactorialSequence(a + b).AsList +
                 FactorialSequence(c + d) +
                 FactorialSequence(a + c) +
                 FactorialSequence(b + d)
        Dim N = a + b + c + d
        Dim sY = FactorialSequence(a).AsList +
                 FactorialSequence(b) +
                 FactorialSequence(c) +
                 FactorialSequence(d) +
                 FactorialSequence(N)
        Dim p = FisherTest.FactorialDivide(sX, sY)
        Return p
    End Function

    ''' <summary>
    ''' 在做基因富集的时候，背景基因的数量会达到上万个，直接计算阶乘的积是无法被计算出来的
    ''' 在这里使用约分来降低数量级
    ''' </summary>
    ''' <param name="X"></param>
    ''' <param name="Y"></param>
    ''' <returns></returns>
    Private Function FactorialDivide(X As List(Of Integer), Y As List(Of Integer)) As Double
        Dim gx = X.GroupBy(Function(n) n).ToDictionary(Function(n) n.Key, Function(n) n.Count)
        Dim gy = Y.GroupBy(Function(n) n).ToDictionary(Function(n) n.Key, Function(n) n.Count)

        ' 将相同的因子在分子和分母之间约掉
        Dim dx As New List(Of KeyValuePair(Of Integer, Integer))

        For Each factor In gx
            If gy.ContainsKey(factor.Key) Then
                ' 取最少的
                Dim min = VBMath.Min(factor.Value, gy(factor.Key))
                dx.Add(New KeyValuePair(Of Integer, Integer)(factor.Key, factor.Value - min))
                gy(factor.Key) -= min
            End If
        Next

        Dim px = dx.ToDictionary.product
        Dim py = gy.product

        Return px / py
    End Function

    <Extension>
    Private Function product(x As Dictionary(Of Integer, Integer)) As Double
        Return x.Select(Function(n) n.Key ^ n.Value).ProductALL
    End Function
End Module

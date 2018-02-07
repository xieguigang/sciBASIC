Imports System.Runtime.CompilerServices

Namespace Math.Information

    ''' <summary>
    ''' 信息熵越大表示所含信息量越多
    ''' </summary>
    Public Module Entropy

        ''' <summary>
        ''' 计算出目标序列的香农信息熵
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="collection"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 计算公式
        ''' 
        ''' ```
        ''' H(x) = E[ I(xi) ] 
        '''      = E[ log(2, 1/p(xi)) ] 
        '''      = -∑ p(xi)log(2, p(xi))  (i=1, 2, ..., n)
        ''' ```
        ''' 
        ''' 其中，``x``表示随机变量，与之相对应的是所有可能输出的集合，定义为符号集，随机变量的输出用``x``表示。
        ''' ``P(x)``表示输出概率函数。变量的不确定性越大，熵也就越大，把它搞清楚所需要的信息量也就越大.
        ''' </remarks>
        <Extension>
        Public Function ShannonEnt(Of T)(collection As IEnumerable(Of T)) As Double
            Dim distincts = (From x As T In collection Group x By x Into Count).ToArray
            Dim entropy#
            Dim prob#
            Dim numEntries% = Aggregate g In distincts Into Sum(g.Count)

            For Each item In distincts
                prob = item.Count / numEntries
                entropy -= prob * Math.Log(prob, newBase:=2)
            Next

            Return entropy
        End Function
    End Module
End Namespace
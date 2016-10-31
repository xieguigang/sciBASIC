Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' ##### Random generator based on the random table.(请注意，这个模块之中的所有函数都是线程不安全的)
''' 
''' ###### A Million Random Digits with 100,000 Normal Deviates
''' 
''' Not long after research began at RAND in 1946, the need arose for random numbers that 
''' could be used to solve problems of various kinds of experimental probability procedures. 
''' These applications, called Monte Carlo methods, required a large supply of random 
''' digits and normal deviates of high quality, and the tables presented here were produced 
''' to meet those requirements. This book was a product of RAND's pioneering work in computing, 
''' as well a testament to the patience and persistence of researchers in the early days of 
''' RAND. The tables of random numbers in this book have become a standard reference in 
''' engineering and econometrics textbooks and have been widely used in gaming and simulations 
''' that employ Monte Carlo trials. Still the largest published source of random digits and 
''' normal deviates, the work is routinely used by statisticians, physicists, polltakers, 
''' market analysts, lottery administrators, and quality control engineers. A 2001 article 
''' in the New York Times on the value of randomness featured the original edition of the book, 
''' published in 1955 by the Free Press. The rights have since reverted to RAND, and in this 
''' digital age, we thought it appropriate to reissue a new edition of the book in its original 
''' format, with a new foreword by Michael D. Rich, RAND's Executive Vice President.
''' 
''' > http://www.rand.org/pubs/monograph_reports/MR1418.html
''' </summary>
Public Class Randomizer

    Shared ReadOnly deviates#()()
    Shared ReadOnly digits%()()

    Shared Sub New()
        Dim lines$() = My.Resources.deviates.lTokens

        deviates# = LinqAPI.Exec(Of Double()) <=
            From line As String
            In lines.AsParallel  ' 并行化可能会进一步加深随机
            Let tokens As String() = Regex.Replace(Mid(line, 5).Trim, "\s{2,}", " ").Split
            Let n As Double() =
                tokens _
                .Select(Function(s) If(s.Last = "-"c, -Val(s), Val(s))) _
                .ToArray
            Select n

        lines$ = My.Resources.digits.lTokens

        digits% = LinqAPI.Exec(Of Integer()) <=
            From line As String
            In lines.AsParallel  ' 并行化可能会进一步加深随机
            Let tokens As String() = Regex.Replace(Mid(line, 6).Trim, "\s{2,}", " ").Split
            Let n As Integer() =
                tokens _
                .Select(Function(s) CInt(Val(s))) _
                .ToArray
            Select n

        max = digits.IteratesALL.Max
        min = digits.IteratesALL.Min
        len = max - min
    End Sub

    ReadOnly _deviates As LoopArray(Of Double())
    ReadOnly _digits As LoopArray(Of Integer())
    ''' <summary>
    ''' <see cref="_digits"/> max integer
    ''' </summary>
    Shared ReadOnly max%, min%, len%

    Sub New()
        Dim rand As New Random

        SyncLock deviates
            Dim list As New List(Of Double())(deviates)

            rand.Shuffle(list)
            _deviates = New LoopArray(Of Double())(list)
        End SyncLock

        SyncLock digits
            Dim list As New List(Of Integer())(digits)

            rand.Shuffle(list)
            _digits = New LoopArray(Of Integer())(list)
        End SyncLock
    End Sub

    ''' <summary>
    ''' 每一行有10个随机数
    ''' </summary>
    Const DigitsRowLength% = 10

    Public Function GetRandomInts(n As Integer) As Integer()
        Return __getRandoms(n, _digits)
    End Function

    Public Function GetRandomInt() As Integer
        Return __getRandom(_digits)
    End Function

    Dim rand As New Random(Now.Millisecond)

    Private Function __getRandom(Of T)(array As LoopArray(Of T())) As T
        Dim d As Integer = rand.NextBoolean
        Dim out As New List(Of T)
        Dim maxRange As Integer = (rand.NextDouble * 100) + 1

        Call array.Set(rand.Next(array.Length))

        Dim delta% = rand.Next(maxRange)
        Dim c% = rand.Next(DigitsRowLength)

        Return array.GET(delta)(c)
    End Function

    Private Function __getRandoms(Of T)(n%, array As LoopArray(Of T())) As T()
        Dim rand As New Random(n * Now.Millisecond)
        Dim d As Integer = rand.NextBoolean
        Dim out As New List(Of T)
        Dim maxRange As Integer = n * (rand.NextDouble * 100) + n

        Call array.Set(rand.Next(array.Length))

        For i As Integer = 0 To n - 1
            Dim delta% = rand.Next(maxRange)
            Dim c% = rand.Next(DigitsRowLength)

            Call out.Add(array.GET(delta)(c))
        Next

        Return out.ToArray
    End Function

    ''' <summary>
    ''' 返回随机的0-1之间的百分比数值
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Public Function GetRandomPercentages(n As Integer) As Double()
        Dim ints%() = GetRandomInts(n)
        Dim ps#() = ints _
            .Select(Function(x) (x - min) / len) _
            .ToArray
        Return ps
    End Function

    Public Function NextDouble() As Double
        Return (__getRandom(_digits) - min) / len
    End Function

    ''' <summary>
    ''' 返回一组符合标准正态分布的实数
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Public Function GetRandomNormalDeviates(n As Integer) As Double()
        Return __getRandoms(n, _deviates)
    End Function

    Public Function GetRandomNormalDeviate() As Double
        Return __getRandom(_deviates)
    End Function
End Class

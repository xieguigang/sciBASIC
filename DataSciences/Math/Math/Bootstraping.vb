Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension
Imports Microsoft.VisualBasic.Linq

Public Module Bootstraping

    Public Function Sample(x As Integer) As Vector
        Dim xvec As Integer() = New Random(Now.Millisecond).Permutation(x, x)
        Return New Vector(xvec.Select(Function(n) CDbl(n)))
    End Function

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

            For k As Integer = 0 To N - 1
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

    '' rcpp_trunc_ndist
    ''
    '' Truncated normal distribution (mean 1, respective upper and lower limits of
    '' 0 and 2).
    ''
    '' @param len Number of elements to be simulated
    '' @param sd Standard deviation
    ''
    '' @return A vector of truncated normally distributed values
    ''
    ' [[Rcpp::export]]
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="len"></param>
    ''' <param name="sd"></param>
    ''' <returns></returns>
    ''' <remarks>https://github.com/mpadge/tnorm</remarks>
    Public Function TruncNDist(len As Integer, sd As Double) As Vector
        Dim eps As Vector ' Set up truncated normal distribution
        Dim z As New List(Of Double)()

        While z.Count < len
            eps = Normal.rnorm(len, 1.0, sd)
            For Each it As Double In eps
                If it >= 0.0 AndAlso it <= 2.0 Then
                    z.Add(it)
                End If
                it += 1
            Next
        End While

        Return New Vector(z)
    End Function

    ''' <summary>
    ''' 标准正态分布, delta = 1, u = 0
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Function StandardDistribution(x As Double) As Double
        Dim answer As Double = 1 / ((Math.Sqrt(2 * Math.PI)))
        Dim exp1 As Double = Math.Pow(x, 2) / 2
        Dim exp As Double = Math.Pow(Math.E, -(exp1))
        answer = answer * exp
        Return answer
    End Function

    ''' <summary>
    ''' Normal Distribution.(正态分布)
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="m">Mean</param>
    ''' <param name="sd"></param>
    ''' <returns></returns>
    Public Function ProbabilityDensity(x As Double, m As Double, sd As Double) As Double
        Dim answer As Double = 1 / (sd * (Math.Sqrt(2 * Math.PI)))
        Dim exp As Double = Math.Pow((x - m), 2.0)
        Dim expP2 As Double = 2 * Math.Pow(sd, 2.0)
        Dim expP3 As Double = Math.Pow(Math.E, (-(exp / expP2)))
        answer = answer * expP3
        Return answer
    End Function

    Public Function AboveStandardDistribution(upperX As Double, n As Double, m As Double, sd As Double) As Double
        Dim lowerX As Double = m - 4.1 * sd
        Dim answer As Double = TrapezodialRule(lowerX, upperX, n, m, sd)
        Return 1 - answer
    End Function

    Public Function BelowStandardDistribution(upperX As Double, n As Double, m As Double, sd As Double) As Double
        Dim lowerX As Double = m + 4.1 * sd
        Dim answer As Double = TrapezodialRule(lowerX, upperX, n, m, sd)
        Return 1 + answer 'lol
    End Function

    Public Function BetweenStandardDistribution(lowerX As Double, upperX As Double, n As Double, m As Double, sd As Double) As Double
        Dim answer As Double = TrapezodialRule(lowerX, upperX, n, m, sd)
        Return answer
    End Function

    Public Function OutsideStandardDistribution(lowerX As Double, upperX As Double, n As Double, m As Double, sd As Double) As Double
        Dim answer As Double = 1 - TrapezodialRule(lowerX, upperX, n, m, sd)
        Return answer
    End Function

    Public Function TrapezodialRule(a As Double, b As Double, n As Double, m As Double, sd As Double) As Double
        Dim changeX As Double = (b - a) / n
        Dim a1 As Double = ProbabilityDensity(a, m, sd)
        Dim b1 As Double = ProbabilityDensity(b, m, sd)
        Dim c As Double = 0.5 * (a1 + b1)

        For i As Double = 1 To n - 1
            c = c + ProbabilityDensity((a + (i * changeX)), m, sd)
        Next i
        c = changeX * c

        Return c
    End Function

    Public Function Z(x As Double, m As Double, sd As Double) As Double
        Dim answer As Double = (x - m) / sd
        Return answer
    End Function
End Module

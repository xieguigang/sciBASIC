Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical

Public Module BootstrapEstimate

    ''' <summary>
    ''' Bootstrapping 参数估计分析，这个函数用于生成基本的采样数据
    ''' </summary>
    ''' <param name="vars">各个参数的变化范围</param>
    ''' <typeparam name="T">具体的求解方程组</typeparam>>
    ''' <param name="k">重复的次数</param>
    ''' <param name="yinit">``Y0``初值</param>
    ''' <returns></returns>
    Public Iterator Function Bootstrapping(Of T As ODEs)(
                                           vars As IEnumerable(Of NamedValue(Of DoubleRange)),
                                          yinit As IEnumerable(Of NamedValue(Of DoubleRange)),
                                              k As Integer,
                                              n As Integer,
                                              a As Integer,
                                              b As Integer) As IEnumerable(Of out)

        Dim params As NamedValue(Of DoubleRange)() = vars.ToArray
        Dim y0 As NamedValue(Of DoubleRange)() = yinit.ToArray
        Dim ps As Dictionary(Of String, Action(Of Object, Double)) =
            params _
            .Select(Function(x) x.Name) _
            .SetParameters(Of T)

        For Each x As out In From it As Integer ' 进行n次并行的采样计算
                             In k.Sequence.AsParallel
                             Select params.iterate(Of T)(y0, ps, n, a, b)
            Yield x
        Next
    End Function

    <Extension>
    Private Function iterate(Of TODEs As ODEs)(vars As NamedValue(Of DoubleRange)(),
                                               yinis As NamedValue(Of DoubleRange)(),
                                               ps As Dictionary(Of String, Action(Of Object, Double)),
                                               n As Integer,
                                               a As Integer,
                                               b As Integer) As out

        Dim odes As TODEs = Activator.CreateInstance(Of TODEs)
        Dim rnd As New Random(Now.Millisecond)

        For Each x In vars
            Dim value As Double = rnd.NextDouble(range:=x.x)
            Call ps(x.Name)(odes, value)  ' 设置方程的参数的值
        Next

        For Each y In yinis
            Dim value As Double = rnd.NextDouble(range:=y.x)
            odes(y.Name).value = value
        Next

        Return odes.Solve(n, a, b, incept:=True)
    End Function

    <Extension>
    Public Function SetParameters(Of T As ODEs)(vars As IEnumerable(Of String)) As Dictionary(Of String, Action(Of Object, Double))
        Dim type As Type = GetType(T)
        Dim ps As New Dictionary(Of String, Action(Of Object, Double))

        For Each var As String In vars
            ps(var) = type.FieldSet(Of Double)(var)
        Next

        Return ps
    End Function
End Module

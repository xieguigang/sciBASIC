Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Text.Xml.Models

''' <summary>
''' A numeric sequence model with exactly ``n + 1`` sampling points, the sampling 
''' grid includes both boundaries of the target range.
''' (带有精确 ``n + 1`` 个采样点的数值序列模型，采样网格包含区间的两个端点)
''' </summary>
''' <remarks>
''' 基类 <see cref="Sequence"/> 的 <see cref="Sequence.ToArray"/> 采用
''' ``range.Enumerate(n).Take(n)`` 的语义，只会返回 ``[min, max)`` 区间上的 n 个点，
''' 末点永远达不到 ``max``；而常微分方程积分需要「n 步 + 1 个端点」共 n + 1 个采样点，
''' 故这里用精确构造的等距点数组覆盖之，保证 ``ToArray().Last = max``。
''' </remarks>
Public Class ODESequence : Inherits Sequence

    ''' <summary>
    ''' The exactly sampling points of current sequence, includes both boundaries.
    ''' (精确的采样点数组，包含区间两端)
    ''' </summary>
    ''' <returns></returns>
    <XmlIgnore>
    Public Property Points As Double()

    Sub New()
    End Sub

    ''' <summary>
    ''' Create a sequence that contains ``n + 1`` sampling points over the range ``[a, b]``.
    ''' </summary>
    ''' <param name="a">The lower bound of the integration range.(积分下界)</param>
    ''' <param name="b">The upper bound of the integration range.(积分上界)</param>
    ''' <param name="n">The number of integration steps.(积分步数)</param>
    Sub New(a As Double, b As Double, n As Integer)
        Call MyBase.New(a, b, n)

        Me.Points = CreatePoints(a, b, n)
    End Sub

    ''' <summary>
    ''' Create the sampling points of the ``[a, b]`` range by ``n`` equal steps.
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Public Shared Function CreatePoints(a As Double, b As Double, n As Integer) As Double()
        If n < 1 Then
            Throw New ArgumentOutOfRangeException(NameOf(n), "The number of integration steps must be positive!")
        End If

        Dim points As Double() = New Double(n) {}
        Dim h As Double = (b - a) / n

        For i As Integer = 0 To n
            points(i) = a + h * i
        Next

        ' 消除浮点累加误差，保证末点精确等于积分上界
        points(n) = b

        Return points
    End Function

    ''' <summary>
    ''' Get the sampling points of current sequence, the last point is exactly the
    ''' upper bound of the range.
    ''' </summary>
    ''' <returns></returns>
    Public Shadows Function ToArray() As Double()
        If Points Is Nothing Then
            Points = CreatePoints(range.Min, range.Max, n)
        End If

        Return Points
    End Function

    Public Overrides Function ToString() As String
        Return range.ToString
    End Function
End Class

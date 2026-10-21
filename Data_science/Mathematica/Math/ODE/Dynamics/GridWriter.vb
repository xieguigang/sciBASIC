Imports System.Runtime.CompilerServices

Namespace Dynamics

    ''' <summary>
    ''' The uniform output sampling grid of the ODEs integration: ``n + 1`` time points over
    ''' ``[a, b]`` (both boundaries are included) and the result columns of each state variable.
    ''' (等间距的输出采样网格与各个状态变量的结果列)
    ''' </summary>
    ''' <remarks>
    ''' 所有求解器都通过这个类型写出结果，从而保证：
    ''' 
    ''' 1. 结果序列与时间序列严格对齐，``Points(0) = a``、``Points(n) = b``；
    ''' 2. 自适应/变步长/隐式细分的求解器也可以通过 <see cref="WriteSpan"/> 把内部步长上
    '''    的计算结果用三次 Hermite 插值重采样到等距网格之上。
    ''' </remarks>
    Friend NotInheritable Class GridWriter

        ''' <summary>
        ''' The ``n + 1`` uniform sampling time points.(n + 1 个等间距采样点)
        ''' </summary>
        Friend ReadOnly Points As Double()
        ''' <summary>
        ''' The result column of each state variable, the length of each column is ``n + 1``.
        ''' (各个状态变量的结果列)
        ''' </summary>
        Friend ReadOnly Columns As Double()()
        ''' <summary>
        ''' The number of the state variables.(状态变量的个数)
        ''' </summary>
        Friend ReadOnly Size As Integer

        Private writePosition As Integer

        ''' <summary>
        ''' Create the output grid of the integration range ``[a, b]`` which contains ``n + 1``
        ''' sampling points.
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="n"></param>
        ''' <param name="size">The number of the state variables.</param>
        Friend Sub New(a As Double, b As Double, n As Integer, size As Integer)
            Me.Points = ODESequence.CreatePoints(a, b, n)
            Me.Size = size
            Me.Columns = New Double(size - 1)() {}

            For i As Integer = 0 To Columns.Length - 1
                Columns(i) = New Double(n) {}
            Next
        End Sub

        ''' <summary>
        ''' The total number of the sampling points, it equals to ``n + 1``.
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property Count As Integer
            Get
                Return Points.Length
            End Get
        End Property

        ''' <summary>
        ''' The index of the next sampling point which is not filled yet.
        ''' (下一个尚未填充的采样点位置)
        ''' </summary>
        ''' <returns></returns>
        Friend Property Cursor As Integer
            Get
                Return writePosition
            End Get
            Set(value As Integer)
                writePosition = value
            End Set
        End Property

        ''' <summary>
        ''' Write the given initial state into the first sampling point and move the cursor to
        ''' the second sampling point.
        ''' </summary>
        ''' <param name="y"></param>
        Friend Sub WriteInitial(y As Double())
            Call Write(0, y)

            writePosition = 1
        End Sub

        ''' <summary>
        ''' Write the given state into the sampling point at the given position.
        ''' </summary>
        ''' <param name="position"></param>
        ''' <param name="y"></param>
        Friend Sub Write(position As Integer, y As Double())
            For i As Integer = 0 To Size - 1
                Columns(i)(position) = y(i)
            Next
        End Sub

        ''' <summary>
        ''' Write the given state into the next unfilled sampling point and advance the cursor.
        ''' </summary>
        ''' <param name="y"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub WriteNext(y As Double())
            Call Write(writePosition, y)

            writePosition += 1
        End Sub

        ''' <summary>
        ''' Fill all of the sampling points which are located inside the step ``(t0, t1]`` by
        ''' using the cubic Hermite interpolation of the step, the sampling points which are
        ''' out of the step will not be touched.
        ''' (用三次 Hermite 插值填充落在该步区间内部的所有采样点)
        ''' </summary>
        ''' <param name="t0">The begin time of the step.</param>
        ''' <param name="y0">The state at the begin of the step.</param>
        ''' <param name="k0">The derivative at the begin of the step.</param>
        ''' <param name="t1">The end time of the step.</param>
        ''' <param name="y1">The state at the end of the step.</param>
        ''' <param name="k1">The derivative at the end of the step.</param>
        Friend Sub WriteSpan(t0 As Double, y0 As Double(), k0 As Double(),
                             t1 As Double, y1 As Double(), k1 As Double())

            Dim h As Double = t1 - t0

            If h <= 0.0 Then
                Return
            End If

            While writePosition < Points.Length AndAlso Points(writePosition) <= t1
                Dim theta As Double = (Points(writePosition) - t0) / h

                For i As Integer = 0 To Size - 1
                    Columns(i)(writePosition) = SolverKernels.Hermite(y0(i), k0(i), y1(i), k1(i), h, theta)
                Next

                writePosition += 1
            End While
        End Sub

        ''' <summary>
        ''' Fill the remaining sampling points by the given state, it is a fallback of the
        ''' floating point error of the last step.
        ''' </summary>
        ''' <param name="y"></param>
        Friend Sub Flush(y As Double())
            While writePosition < Points.Length
                Call Write(writePosition, y)

                writePosition += 1
            End While
        End Sub
    End Class
End Namespace

#Region "Microsoft.VisualBasic::7a0a75c381881c0025c597f28a53d652, Data_science\Mathematica\Math\ODE\Dynamics\RungeKutta4.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Class RungeKutta4
    '
    '     Constructor: (+1 Overloads) Sub New
    '
    '     Function: Solve, solverIteration, ToString
    '
    '     Sub: GetResult, rungeKutta
    '
    '
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Dynamics

    ''' <summary>
    ''' The classical 4th order Runge-Kutta solver for the <see cref="ODEs"/> dynamics system.
    ''' (用于求解 ODE 方程组的经典四阶龙格库塔法求解器)
    ''' </summary>
    ''' <remarks>
    ''' 求解过程使用预分配的工作缓冲，每一个积分步都不会产生任何托管堆分配；结果序列
    ''' 的时间采样点与计算结果严格一一对应，并且同时包含积分区间的两个端点。
    ''' </remarks>
    Public Class RungeKutta4

        Dim ODEs As ODEs

        ReadOnly buffers As SolverBuffers

        Dim xArray As Double()
        Dim yColumns As Double()()

        Sub New(system As ODEs)
            ODEs = system
            buffers = New SolverBuffers(system.Size)
        End Sub

        ''' <summary>
        ''' The sampling time points of the last integration result, it contains ``n + 1``
        ''' points which includes both boundaries of the integration range.
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property XValues As Double()
            Get
                Return xArray
            End Get
        End Property

        ''' <summary>
        ''' The integration result columns of each state variable of the last integration result.
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property YValues As Double()()
            Get
                Return yColumns
            End Get
        End Property

        ''' <summary>
        ''' Runge-Kutta 4th order integration step, the ``y(n+1)`` value will be
        ''' written into the internal working buffer directly.
        ''' </summary>
        ''' <param name="dxn">The x initial value.(x初值)</param>
        ''' <param name="dh">Steps delta.(步长)</param>
        ''' <param name="index">The position of the current step in the output columns.</param>
        Private Sub rungeKutta(dxn As Double, dh As Double, index As Integer)
            Dim k As Double()() = buffers.k
            Dim hh As Double = 0.5 * dh

            ' 供 RefODEs 等需要按步索引定位参考数据的模型使用
            ODEs.CurrentStep = index

            ' 求解K1
            Call ODEs.ODEs(dxn, buffers.vy, buffers.vk(0))
            ' 求解K2
            Call SolverKernels.Combine(buffers.tmp, buffers.y, hh, k(0))
            Call ODEs.ODEs(dxn + hh, buffers.vtmp, buffers.vk(1))
            ' 求解K3
            Call SolverKernels.Combine(buffers.tmp, buffers.y, hh, k(1))
            Call ODEs.ODEs(dxn + hh, buffers.vtmp, buffers.vk(2))
            ' 求解K4
            Call SolverKernels.Combine(buffers.tmp, buffers.y, dh, k(2))
            Call ODEs.ODEs(dxn + dh, buffers.vtmp, buffers.vk(3))

            ' 求解下一步的值y(n+1)，并直接写入输出列
            Call SolverKernels.Rk4Update(buffers.y, k(0), k(1), k(2), k(3), dh / 6.0, yColumns, index)
        End Sub

        ''' <summary>
        ''' Get the integration result in the <see cref="List(Of Double)"/> format.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        Public Sub GetResult(ByRef x As Double(), ByRef y As List(Of Double)())
            x = xArray

            If yColumns Is Nothing Then
                y = New List(Of Double)() {}
            Else
                Dim lists As List(Of Double)() = New List(Of Double)(yColumns.Length - 1) {}

                For i As Integer = 0 To yColumns.Length - 1
                    lists(i) = New List(Of Double)(yColumns(i))
                Next

                y = lists
            End If
        End Sub

        ''' <summary>
        ''' Solve the ODEs system.
        ''' </summary>
        ''' <param name="y0">The y initial value.(初值y(n))</param>
        ''' <param name="n">A larger value of this parameter, will makes your result more precise.</param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        Public Function Solve(y0 As Double(), n As Integer, a As Double, b As Double) As RungeKutta4
            Dim steps As IEnumerator(Of Integer) = solverIteration(y0, n, a, b).GetEnumerator()

            ' 强制把迭代器推进到结束，这样结果序列才会被完整地写入到工作缓冲之中
            Do While steps.MoveNext()
            Loop

            Return Me
        End Function

        ''' <summary>
        ''' The step by step integration iterator, the first yielded value is the index of the
        ''' initial state ``i = 0``, then the step index ``1`` to ``n`` will be yielded.
        ''' </summary>
        ''' <param name="y0">The y initial value.(初值y(n))</param>
        ''' <param name="n">The number of the integration steps.</param>
        ''' <param name="a">The lower bound of the integration range.</param>
        ''' <param name="b">The upper bound of the integration range.</param>
        Friend Iterator Function solverIteration(y0 As Double(), n As Integer, a As Double, b As Double) As IEnumerable(Of Integer)
            If n < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(n), "The number of the integration steps must be positive!")
            End If
            If y0 Is Nothing OrElse y0.Length <> buffers.Size Then
                Throw New ArgumentException(
                    $"The dimension of the given y0 initial value ({If(y0 Is Nothing, "null", y0.Length.ToString)}) is not matched to the ODEs system ({buffers.Size})!",
                    NameOf(y0))
            End If

            ' 步长
            Dim dh As Double = (b - a) / n
            Dim y As Double() = buffers.y
            Dim columns As Double()() = New Double(buffers.Size - 1)() {}

            For i As Integer = 0 To columns.Length - 1
                columns(i) = New Double(n) {}
            Next

            ' 写入初值，采样点 0 就是积分下界 a
            For i As Integer = 0 To y.Length - 1
                y(i) = y0(i)
                columns(i)(0) = y0(i)
            Next

            xArray = ODESequence.CreatePoints(a, b, n)
            yColumns = columns

            Yield 0

            For iStep As Integer = 1 To n
                ' 第 iStep 步是从时间 a + (iStep - 1) * dh 推进到 a + iStep * dh
                Call rungeKutta(a + dh * (iStep - 1), dh, iStep)

                Yield iStep
            Next

            ' 积分结束之后，把最终状态同步到各个状态变量对象之上
            Call ODEs.SyncState(y)
        End Function

        Public Overrides Function ToString() As String
            Return If(ODEs Is Nothing, NameOf(RungeKutta4), ODEs.ToString)
        End Function
    End Class
End Namespace

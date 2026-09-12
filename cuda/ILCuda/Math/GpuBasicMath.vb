#Region "Microsoft.VisualBasic::a88d2df4c523351ec03296d60f0cf04c, cuda\ILCuda\Math\GpuBasicMath.vb"

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
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 95
    '    Code Lines: 60 (63.16%)
    ' Comment Lines: 14 (14.74%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 21 (22.11%)
    '     File Size: 3.99 KB


    '     Module GpuBasicMath
    ' 
    '         Function: IsAcceptable, RunBasicDemo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' 框架自带基础内核（vecAdd / saxpy）的冒烟示例
'
' 它演示了一条最简链路：主机数据 -> 上传 -> 启动内核 -> 回读 -> 与 CPU 比对，
' 同时被 test 工程当作"GPU 是否真的能算对"的快速自检。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

Namespace Math

    ''' <summary>基础数学加速示例</summary>
    Public Module GpuBasicMath

        ''' <summary>saxpy 示例使用的缩放系数</summary>
        Public Const SaxpyAlpha As Single = 2.5F

        ''' <summary>
        ''' 在 GPU 上执行 vecAdd(c = a + b) 与 saxpy(out = alpha * x + y)，
        ''' 并与 CPU 结果比对。
        ''' </summary>
        ''' <param name="stream">可选的执行流；省略时使用 NULL 流</param>
        Public Function RunBasicDemo(engine As CudaEngine, n As Integer,
                                     ByRef elapsedMs As Double,
                                     ByRef maxError As Double,
                                     Optional stream As CudaStream = Nothing,
                                     Optional seed As Integer = 20240906) As Boolean
            If n <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(n))

            Const blockSize As Integer = 256

            Dim a(n - 1) As Single
            Dim b(n - 1) As Single
            Dim x(n - 1) As Single
            Dim y(n - 1) As Single
            Dim expectedC(n - 1) As Single
            Dim expectedOut(n - 1) As Single

            Dim rnd As New System.Random(seed)

            For i As Integer = 0 To n - 1
                a(i) = CSng(rnd.NextDouble() * 2.0 - 1.0)
                b(i) = CSng(rnd.NextDouble() * 2.0 - 1.0)
                x(i) = CSng(rnd.NextDouble() * 2.0 - 1.0)
                y(i) = CSng(rnd.NextDouble() * 2.0 - 1.0)
                expectedC(i) = a(i) + b(i)
                expectedOut(i) = SaxpyAlpha * x(i) + y(i)
            Next

            Using deviceA As New DeviceBuffer(Of Single)(n),
                  deviceB As New DeviceBuffer(Of Single)(n),
                  deviceC As New DeviceBuffer(Of Single)(n),
                  deviceX As New DeviceBuffer(Of Single)(n),
                  deviceY As New DeviceBuffer(Of Single)(n),
                  deviceOut As New DeviceBuffer(Of Single)(n)

                deviceA.Write(a)
                deviceB.Write(b)
                deviceX.Write(x)
                deviceY.Write(y)

                Dim config = LaunchPlanner.For1D(n, blockSize)

                Using timer As New CudaTimer()
                    timer.Start()

                    engine.GetKernel(KernelNames.VecAdd).Launch(
                        stream, config, deviceA, deviceB, deviceC, n)

                    engine.GetKernel(KernelNames.Saxpy).Launch(
                        stream, config, SaxpyAlpha, deviceX, deviceY, deviceOut, n)

                    elapsedMs = timer.Finish()
                End Using

                Dim c = deviceC.Read()
                Dim out = deviceOut.Read()

                maxError = 0

                For i As Integer = 0 To n - 1
                    maxError = System.Math.Max(maxError, System.Math.Abs(CDbl(c(i)) - CDbl(expectedC(i))))
                    maxError = System.Math.Max(maxError, System.Math.Abs(CDbl(out(i)) - CDbl(expectedOut(i))))
                Next
            End Using

            Return True
        End Function

        ''' <summary>把 GPU 结果与 CPU 结果的最大绝对误差作为通过阈值来判断对错</summary>
        Public Function IsAcceptable(maxError As Double, Optional tolerance As Double = 0.00001) As Boolean
            Return Not Double.IsNaN(maxError) AndAlso maxError <= tolerance
        End Function
    End Module
End Namespace

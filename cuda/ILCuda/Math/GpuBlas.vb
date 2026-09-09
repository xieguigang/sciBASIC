#Region "Microsoft.VisualBasic::05eede7264f3e11692045eb7dd8655b5, cuda\ILCuda\Math\GpuBlas.vb"

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

    '   Total Lines: 121
    '    Code Lines: 75 (61.98%)
    ' Comment Lines: 21 (17.36%)
    '    - Xml Docs: 33.33%
    ' 
    '   Blank Lines: 25 (20.66%)
    '     File Size: 6.09 KB


    '     Module GpuBlas
    ' 
    '         Function: Gemm, Gemv
    ' 
    '         Sub: Gemm, Gemv, RequireLength
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' 基础线性代数（行主序 row-major）
'
'   Gemm: C(m x n) = A(m x k) * B(k x n)
'   Gemv: y(m)     = A(m x k) * x(k)
'
' 内核用 16x16 分块 + 共享内存，越界位置补 0，因此任意尺寸都能直接算。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

Namespace Math

    ''' <summary>单精度矩阵/向量的基础线性代数算子</summary>
    Public Module GpuBlas

        ''' <summary>gemmKernel 的分块大小（必须与 blas.cu 中的 GEMM_TILE 一致）</summary>
        Public Const GemmTile As Integer = 16
        ''' <summary>gemvKernel 每个 block 的线程数</summary>
        Public Const GemvBlockSize As Integer = 256

        ' ------------------------------------------------------------------
        ' GEMM
        ' ------------------------------------------------------------------

        ''' <summary>C(m x n) = A(m x k) * B(k x n)，全部数据已在显存中</summary>
        Public Sub Gemm(engine As CudaEngine,
                        a As DeviceBuffer(Of Single), b As DeviceBuffer(Of Single), c As DeviceBuffer(Of Single),
                        m As Integer, n As Integer, k As Integer,
                        Optional stream As CudaStream = Nothing)
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            If m <= 0 OrElse n <= 0 OrElse k <= 0 Then
                Throw New ArgumentOutOfRangeException("m / n / k 必须为正数")
            End If
            RequireLength(NameOf(a), a, m * k, $"A 应为 {m} x {k}")
            RequireLength(NameOf(b), b, k * n, $"B 应为 {k} x {n}")
            RequireLength(NameOf(c), c, m * n, $"C 应为 {m} x {n}")

            Dim config = LaunchPlanner.For2D(m, n, GemmTile, GemmTile, 0)

            engine.GetKernel(KernelNames.Gemm).Launch(stream, config, a, b, c, m, n, k)
        End Sub

        ''' <summary>主机便捷入口：C(m x n) = A(m x k) * B(k x n)</summary>
        Public Function Gemm(engine As CudaEngine, a As Single(), b As Single(),
                             m As Integer, n As Integer, k As Integer) As Single()
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))
            If a Is Nothing OrElse b Is Nothing Then Throw New ArgumentNullException("输入矩阵不能为空")
            If m <= 0 OrElse n <= 0 OrElse k <= 0 Then
                Throw New ArgumentOutOfRangeException("m / n / k 必须为正数")
            End If
            If a.Length <> m * k Then Throw New ArgumentException($"A 的长度 {a.Length} 与 {m} x {k} 不一致")
            If b.Length <> k * n Then Throw New ArgumentException($"B 的长度 {b.Length} 与 {k} x {n} 不一致")

            Using deviceA As New DeviceBuffer(Of Single)(a.Length),
                  deviceB As New DeviceBuffer(Of Single)(b.Length),
                  deviceC As New DeviceBuffer(Of Single)(m * n)

                deviceA.Write(a)
                deviceB.Write(b)

                Gemm(engine, deviceA, deviceB, deviceC, m, n, k)

                Return deviceC.Read()
            End Using
        End Function

        ' ------------------------------------------------------------------
        ' GEMV
        ' ------------------------------------------------------------------

        ''' <summary>y(m) = A(m x k) * x(k)，全部数据已在显存中</summary>
        Public Sub Gemv(engine As CudaEngine,
                        a As DeviceBuffer(Of Single), x As DeviceBuffer(Of Single), y As DeviceBuffer(Of Single),
                        m As Integer, k As Integer,
                        Optional stream As CudaStream = Nothing)
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            If m <= 0 OrElse k <= 0 Then Throw New ArgumentOutOfRangeException("m / k 必须为正数")
            RequireLength(NameOf(a), a, m * k, $"A 应为 {m} x {k}")
            RequireLength(NameOf(x), x, k, $"x 长度应为 {k}")
            RequireLength(NameOf(y), y, m, $"y 长度应为 {m}")

            engine.GetKernel(KernelNames.Gemv).Launch(
                stream, m, 1, GemvBlockSize, 1, GemvBlockSize * 4,
                a, x, y, m, k)
        End Sub

        ''' <summary>主机便捷入口：y(m) = A(m x k) * x(k)</summary>
        Public Function Gemv(engine As CudaEngine, a As Single(), x As Single(),
                             m As Integer, k As Integer) As Single()
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))
            If a Is Nothing OrElse x Is Nothing Then Throw New ArgumentNullException("输入不能为空")
            If m <= 0 OrElse k <= 0 Then Throw New ArgumentOutOfRangeException("m / k 必须为正数")
            If a.Length <> m * k Then Throw New ArgumentException($"A 的长度 {a.Length} 与 {m} x {k} 不一致")
            If x.Length <> k Then Throw New ArgumentException($"x 的长度 {x.Length} 与 {k} 不一致")

            Using deviceA As New DeviceBuffer(Of Single)(a.Length),
                  deviceX As New DeviceBuffer(Of Single)(k),
                  deviceY As New DeviceBuffer(Of Single)(m)

                deviceA.Write(a)
                deviceX.Write(x)

                Gemv(engine, deviceA, deviceX, deviceY, m, k)

                Return deviceY.Read()
            End Using
        End Function

        Private Sub RequireLength(name As String, buffer As DeviceBuffer(Of Single),
                                  expected As Integer, hint As String)
            If buffer Is Nothing Then Throw New ArgumentNullException(name)

            If buffer.Count <> expected Then
                Throw New ArgumentException($"{name} 长度 {buffer.Count} 与预期 {expected} 不一致（{hint}）")
            End If
        End Sub
    End Module
End Namespace


#Region "Microsoft.VisualBasic::393d463c9bf7a84012a02215e40ad49c, Data_science\MachineLearning\TensorFlow\Compute\SIMDTensorF.vb"

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

    '   Total Lines: 319
    '    Code Lines: 226 (70.85%)
    ' Comment Lines: 39 (12.23%)
    '    - Xml Docs: 74.36%
    ' 
    '   Blank Lines: 54 (16.93%)
    '     File Size: 12.34 KB


    '     Class SIMDTensorF
    ' 
    '         Properties: [Default], Name, VectorWidth
    ' 
    '         Function: Abs, Add, AddScalar, Clip, Divide
    '                   DivideScalar, MaxAll, Maximum, MinAll, Minimum
    '                   Multiply, MultiplyScalar, Negate, Sqrt, Square
    '                   Subtract, SumAll, VecBinary, VecScalar, VecUnary
    ' 
    '         Sub: Register
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 单精度张量的默认计算后端：基于 System.Numerics.Vector(Of Single) 的 SIMD CPU 实现
'
' 与双精度栈的 SIMDTensor 定位一致：只重写"确实有向量指令收益"的算子，
' 其余（含 CFD 三个原语）自动继承 TensorComputeFBase 的标量实现。
'
' 相对双精度的关键收益：
'   Vector(Of Single) 在 AVX2 下是 8 lane，是 Vector(Of Double) 的 2 倍；
'   配合减半的内存占用，逐元素算子的理论吞吐约为双精度版的 2 倍。
' ---------------------------------------------------------------------------

Imports nv = System.Numerics
Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' 单精度张量的默认 SIMD 加速 CPU 计算后端。
    ''' </summary>
    Public Class SIMDTensorF
        Inherits TensorComputeFBase

        ''' <summary>向量宽度（运行时决定，AVX2 下 Single 为 8）</summary>
        Public Shared ReadOnly Property VectorWidth As Integer
            Get
                Return nv.Vector(Of Single).Count
            End Get
        End Property

        ''' <summary>全局默认实例（<see cref="TensorF.computeKernelF"/> 的初始值）</summary>
        Public Shared ReadOnly Property [Default] As SIMDTensorF = New SIMDTensorF()

        ''' <summary>后端名称</summary>
        Public Overrides ReadOnly Property Name As String = "SIMD-F32"

        ''' <summary>
        ''' 把 <see cref="TensorF.computeKernelF"/> 切换为 SIMD 加速的 CPU 实现。
        ''' </summary>
        Public Shared Sub Register()
            SyncLock TensorF.SyncRoot
                TensorF.computeKernelF = [Default]
            End SyncLock
        End Sub

#Region "向量化辅助"

        ''' <summary>逐元素二元向量运算</summary>
        Private Shared Function VecBinary(x As Single(), y As Single(),
                                          op As Func(Of nv.Vector(Of Single), nv.Vector(Of Single), nv.Vector(Of Single)),
                                          tail As Func(Of Single, Single, Single)) As Single()
            Dim n = x.Length
            Dim r(n - 1) As Single
            Dim w = nv.Vector(Of Single).Count
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                op(New nv.Vector(Of Single)(x, i), New nv.Vector(Of Single)(y, i)).CopyTo(r, i)
                i += w
            End While
            While i < n
                r(i) = tail(x(i), y(i))
                i += 1
            End While

            Return r
        End Function

        ''' <summary>逐元素一元向量运算</summary>
        Private Shared Function VecUnary(x As Single(),
                                         op As Func(Of nv.Vector(Of Single), nv.Vector(Of Single)),
                                         tail As Func(Of Single, Single)) As Single()
            Dim n = x.Length
            Dim r(n - 1) As Single
            Dim w = nv.Vector(Of Single).Count
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                op(New nv.Vector(Of Single)(x, i)).CopyTo(r, i)
                i += w
            End While
            While i < n
                r(i) = tail(x(i))
                i += 1
            End While

            Return r
        End Function

        ''' <summary>带标量的逐元素向量运算</summary>
        Private Shared Function VecScalar(x As Single(), s As Single,
                                          op As Func(Of nv.Vector(Of Single), nv.Vector(Of Single), nv.Vector(Of Single)),
                                          tail As Func(Of Single, Single, Single)) As Single()
            Dim n = x.Length
            Dim r(n - 1) As Single
            Dim w = nv.Vector(Of Single).Count
            Dim vs As New nv.Vector(Of Single)(s)
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                op(New nv.Vector(Of Single)(x, i), vs).CopyTo(r, i)
                i += w
            End While
            While i < n
                r(i) = tail(x(i), s)
                i += 1
            End While

            Return r
        End Function

#End Region

#Region "逐元素 - 二元"

        ''' <summary>逐元素相加</summary>
        Public Overrides Function Add(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "相加")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Add(x, y),
                                  Function(x, y) x + y), a.Shape)
        End Function

        ''' <summary>逐元素相减</summary>
        Public Overrides Function Subtract(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "相减")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Subtract(x, y),
                                  Function(x, y) x - y), a.Shape)
        End Function

        ''' <summary>逐元素相乘</summary>
        Public Overrides Function Multiply(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "相乘")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Multiply(x, y),
                                  Function(x, y) x * y), a.Shape)
        End Function

        ''' <summary>逐元素相除</summary>
        Public Overrides Function Divide(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "相除")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Divide(x, y),
                                  Function(x, y) x / y), a.Shape)
        End Function

        ''' <summary>逐元素取较大值</summary>
        Public Overrides Function Maximum(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "取最大值")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Max(x, y),
                                  Function(x, y) std.Max(x, y)), a.Shape)
        End Function

        ''' <summary>逐元素取较小值</summary>
        Public Overrides Function Minimum(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "取最小值")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Min(x, y),
                                  Function(x, y) std.Min(x, y)), a.Shape)
        End Function

#End Region

#Region "逐元素 - 一元"

        ''' <summary>逐元素平方根</summary>
        Public Overrides Function Sqrt(t As TensorF) As TensorF
            Return Wrap(VecUnary(t.Data,
                                 Function(x) nv.Vector.SquareRoot(x),
                                 Function(x) CSng(std.Sqrt(x))), t.Shape)
        End Function

        ''' <summary>逐元素平方</summary>
        Public Overrides Function Square(t As TensorF) As TensorF
            Return Wrap(VecUnary(t.Data,
                                 Function(x) nv.Vector.Multiply(x, x),
                                 Function(x) x * x), t.Shape)
        End Function

        ''' <summary>逐元素绝对值</summary>
        Public Overrides Function Abs(t As TensorF) As TensorF
            Return Wrap(VecUnary(t.Data,
                                 Function(x) nv.Vector.Abs(x),
                                 Function(x) std.Abs(x)), t.Shape)
        End Function

        ''' <summary>逐元素取负</summary>
        Public Overrides Function Negate(t As TensorF) As TensorF
            Return Wrap(VecUnary(t.Data,
                                 Function(x) nv.Vector.Negate(x),
                                 Function(x) -x), t.Shape)
        End Function

#End Region

#Region "标量运算"

        ''' <summary>逐元素加标量</summary>
        Public Overrides Function AddScalar(t As TensorF, scalar As Single) As TensorF
            Return Wrap(VecScalar(t.Data, scalar,
                                  Function(x, s) nv.Vector.Add(x, s),
                                  Function(x, s) x + s), t.Shape)
        End Function

        ''' <summary>逐元素乘标量</summary>
        Public Overrides Function MultiplyScalar(t As TensorF, scalar As Single) As TensorF
            Return Wrap(VecScalar(t.Data, scalar,
                                  Function(x, s) nv.Vector.Multiply(x, s),
                                  Function(x, s) x * s), t.Shape)
        End Function

        ''' <summary>逐元素除标量</summary>
        Public Overrides Function DivideScalar(t As TensorF, scalar As Single) As TensorF
            Return Wrap(VecScalar(t.Data, scalar,
                                  Function(x, s) nv.Vector.Divide(x, s),
                                  Function(x, s) x / s), t.Shape)
        End Function

        ''' <summary>把元素钳制到 [low, high] 区间</summary>
        Public Overrides Function Clip(t As TensorF, low As Single, high As Single) As TensorF
            Return Wrap(VecUnary(t.Data,
                                 Function(x) nv.Vector.Min(nv.Vector.Max(x, New nv.Vector(Of Single)(low)),
                                                           New nv.Vector(Of Single)(high)),
                                 Function(x) std.Min(std.Max(x, low), high)), t.Shape)
        End Function

#End Region

#Region "归约运算"

        ''' <summary>整体求和（向量累加 + 双精度收尾，避免单精度累加误差）</summary>
        Public Overrides Function SumAll(t As TensorF) As Double
            Dim x = t.Data
            Dim n = x.Length
            Dim w = nv.Vector(Of Single).Count
            Dim acc As New nv.Vector(Of Single)(0.0F)
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                acc = nv.Vector.Add(acc, New nv.Vector(Of Single)(x, i))
                i += w
            End While

            Dim s As Double = 0
            For lane = 0 To w - 1
                s += acc(lane)
            Next
            While i < n
                s += x(i)
                i += 1
            End While

            Return s
        End Function

        ''' <summary>最小值</summary>
        Public Overrides Function MinAll(t As TensorF) As Single
            Dim x = t.Data
            If x.Length = 0 Then Return 0.0F
            Dim w = nv.Vector(Of Single).Count
            Dim n = x.Length
            Dim acc As New nv.Vector(Of Single)(Single.MaxValue)
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                acc = nv.Vector.Min(acc, New nv.Vector(Of Single)(x, i))
                i += w
            End While

            Dim m As Single = Single.MaxValue
            For lane = 0 To w - 1
                If acc(lane) < m Then m = acc(lane)
            Next
            While i < n
                If x(i) < m Then m = x(i)
                i += 1
            End While

            Return m
        End Function

        ''' <summary>最大值</summary>
        Public Overrides Function MaxAll(t As TensorF) As Single
            Dim x = t.Data
            If x.Length = 0 Then Return 0.0F
            Dim w = nv.Vector(Of Single).Count
            Dim n = x.Length
            Dim acc As New nv.Vector(Of Single)(Single.MinValue)
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                acc = nv.Vector.Max(acc, New nv.Vector(Of Single)(x, i))
                i += w
            End While

            Dim m As Single = Single.MinValue
            For lane = 0 To w - 1
                If acc(lane) > m Then m = acc(lane)
            Next
            While i < n
                If x(i) > m Then m = x(i)
                i += 1
            End While

            Return m
        End Function

#End Region

    End Class

End Namespace

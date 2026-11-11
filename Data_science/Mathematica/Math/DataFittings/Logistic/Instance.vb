#Region "Microsoft.VisualBasic::a7e1e778fac94762f42be3ec3ea7a616, Data_science\Mathematica\Math\DataFittings\Logistic\Instance.vb"

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

#End Region

Imports System
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.SIMD
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

Namespace Logistic

    ''' <summary>
    ''' 按列标准化参数。训练与推理必须共用同一份参数，否则模型系数与输入
    ''' 不在同一个空间，预测结果会静默失真。
    ''' </summary>
    Public Structure Standardization

        ''' <summary>每一列的均值</summary>
        Public Property Means As Double()
        ''' <summary>每一列的总体标准差；常量列为 0</summary>
        Public Property Sds As Double()

        ''' <summary>是否未启用标准化</summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Means Is Nothing OrElse Means.Length = 0
            End Get
        End Property

        ''' <summary>
        ''' 把一个原始特征向量变换到标准化空间。未启用时原样返回输入（不拷贝）。
        ''' </summary>
        Public Function Apply(x As Double()) As Double()
            If IsEmpty Then
                Return x
            End If
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If x.Length <> Means.Length Then
                Throw New ArgumentException($"feature size not agree: {x.Length} vs {Means.Length}!", NameOf(x))
            End If

            Dim z As Double() = New Double(x.Length - 1) {}

            For j As Integer = 0 To x.Length - 1
                Dim sd As Double = Sds(j)
                z(j) = If(sd = 0.0R, 0.0R, (x(j) - Means(j)) / sd)
            Next

            Return z
        End Function
    End Structure

    ''' <summary>
    ''' classify training model
    ''' </summary>
    Public Class Instance

        ''' <summary>
        ''' the real label data, must be 0 or 1
        ''' </summary>
        ''' <returns></returns>
        Public Property label As Double
        ''' <summary>
        ''' the object properties vector
        ''' </summary>
        ''' <returns></returns>
        Public Property x As Double()

        Public ReadOnly Property featureSize As Integer
            Get
                Return x.Length
            End Get
        End Property

        Public Sub New(label As Integer, x As Integer())
            Me.label = label
            Me.x = x.Select(Function(d) CDbl(d)).ToArray
        End Sub

        Public Sub New(label As Integer, x As Double())
            Me.label = label
            Me.x = x
        End Sub

        Sub New(label As Double, x As IEnumerable(Of Double))
            Me.label = label
            Me.x = x.ToArray
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{label}] {x.GetJson}"
        End Function

        ''' <summary>
        ''' 按列标准化，返回**新的**样本数组并带出 μ/σ；不修改输入。
        ''' </summary>
        ''' <param name="data">原始样本</param>
        ''' <param name="norm">输出的标准化参数，供推理阶段复用</param>
        ''' <param name="size">
        ''' 参与标准化的列数，缺省（-1）时自动取特征数。
        ''' </param>
        ''' <remarks>
        ''' 旧实现原地写回 <c>data(j).x(i)</c> 并丢弃 μ/σ，导致重复调用会二次标准化、
        ''' 且无法对新样本做同样的变换；常量列还会因 σ=0 得到 NaN。
        ''' </remarks>
        Public Shared Function ZScore(data As Instance(),
                                      ByRef norm As Standardization,
                                      Optional size As Integer = -1) As Instance()
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If
            If data.Length = 0 Then
                Throw New ArgumentException("ZScore requires at least one instance!", NameOf(data))
            End If
            If data(0) Is Nothing OrElse data(0).x Is Nothing Then
                Throw New ArgumentException("the first instance has no feature values!", NameOf(data))
            End If

            Dim dims As Integer = data(0).x.Length

            If size < 0 Then
                size = dims
            End If
            If size < 1 OrElse size > dims Then
                Throw New ArgumentException($"ZScore size must be within [1, {dims}], but got {size}!", NameOf(size))
            End If

            Dim n As Integer = data.Length
            Dim result As Instance() = New Instance(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim item As Instance = data(i)

                If item Is Nothing OrElse item.x Is Nothing OrElse item.x.Length <> dims Then
                    Throw New ArgumentException($"inconsistent feature size at index {i}!", NameOf(data))
                End If

                For j As Integer = 0 To dims - 1
                    If Double.IsNaN(item.x(j)) OrElse Double.IsInfinity(item.x(j)) Then
                        Throw New ArgumentException($"feature value at [{i},{j}] is not finite!", NameOf(data))
                    End If
                Next

                ' 用 (Integer, Double()) 这个明确的重载，再单独赋 Double 标签，
                ' 避免 (Double, IEnumerable) 重载的解析歧义；特征数组做深拷贝。
                result(i) = New Instance(0, CType(item.x.Clone(), Double()))
                result(i).label = item.label
            Next

            Dim means As Double() = New Double(size - 1) {}
            Dim sds As Double() = New Double(size - 1) {}
            Dim column As Double() = New Double(n - 1) {}

            For j As Integer = 0 To size - 1
                For i As Integer = 0 To n - 1
                    column(i) = data(i).x(j)
                Next

                Dim mean As Double = SimdReduce.Mean(column)
                Dim variance As Double = 0.0R

                For i As Integer = 0 To n - 1
                    Dim d As Double = column(i) - mean
                    variance += d * d
                Next

                ' 总体标准差（与 pnorm.Z 的 x.SD 语义一致）
                Dim sd As Double = std.Sqrt(variance / n)

                means(j) = mean
                sds(j) = sd

                For i As Integer = 0 To n - 1
                    result(i).x(j) = If(sd = 0.0R, 0.0R, (column(i) - mean) / sd)
                Next
            Next

            norm = New Standardization With {.Means = means, .Sds = sds}

            Return result
        End Function

        ''' <summary>
        ''' 兼容旧签名：返回标准化后的新样本数组（不再原地修改输入）。
        ''' </summary>
        ''' <param name="size">参与标准化的列数，必须不大于特征数</param>
        Public Shared Function ZScore(data As Instance(), size As Integer) As Instance()
            Dim ignored As Standardization = Nothing
            Return ZScore(data, ignored, size)
        End Function

        ''' <summary>
        ''' load raw dataset helper function
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <param name="featureSet"></param>
        ''' <param name="label"></param>
        ''' <returns></returns>
        Public Shared Iterator Function Load(Of T As {DynamicPropertyBase(Of Double)})(data As IEnumerable(Of T), featureSet As String(), label As String) As IEnumerable(Of Instance)
            For Each row As T In data
                Yield New Instance(row(label), row(featureSet))
            Next
        End Function
    End Class
End Namespace

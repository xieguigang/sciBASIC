#Region "Microsoft.VisualBasic::CMeansEngine, Data_science\DataMining\DataMining\Clustering\FuzzyCMeans\CMeansEngine.vb"

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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace FuzzyCMeans

    ''' <summary>
    ''' 模糊 C 均值聚类的数值计算结果
    ''' </summary>
    Public Class CMeansResult

        ''' <summary>
        ''' 每一个簇的中心点
        ''' </summary>
        ''' <returns></returns>
        Public Property centers As Double()()

        ''' <summary>
        ''' 隶属度矩阵，``membership(sample)(cluster)``
        ''' </summary>
        ''' <returns></returns>
        Public Property membership As Double()()

        Private _cluster As Integer()

        ''' <summary>
        ''' 硬划分的簇编号结果（取每一行隶属度最大的那个簇）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property cluster As Integer()
            Get
                If _cluster Is Nothing Then
                    _cluster = New Integer(membership.Length - 1) {}

                    For i As Integer = 0 To membership.Length - 1
                        _cluster(i) = Array.IndexOf(membership(i), membership(i).Max)
                    Next
                End If

                Return _cluster
            End Get
        End Property
    End Class

    ''' <summary>
    ''' 基于数值行（``Double()``）的模糊 C 均值聚类引擎。
    ''' 
    ''' 该引擎直接消费<see cref="NumericTable"/>的特征矩阵，不再依赖
    ''' <see cref="ComponentModel.EntityModels.ClusterEntity"/> 实体对象，
    ''' 运算结果（隶属度矩阵与硬划分结果）可以写回标签矩阵。
    ''' </summary>
    Public Class CMeansEngine

        ''' <summary>
        ''' 执行模糊 C 均值聚类
        ''' </summary>
        ''' <param name="rows">行主序的数值特征矩阵</param>
        ''' <param name="classCount">簇的数量</param>
        ''' <param name="m">模糊因子 fuzzification</param>
        ''' <param name="threshold">迭代收敛的阈值</param>
        ''' <param name="parallel">是否使用并行计算更新隶属度矩阵</param>
        ''' <param name="maxLoop">最大迭代次数</param>
        ''' <returns></returns>
        Public Function Run(rows As Double()(),
                            classCount As Integer,
                            Optional m As Double = 2,
                            Optional threshold As Double = 0.001,
                            Optional parallel As Boolean = True,
                            Optional maxLoop As Integer = 10000) As CMeansResult

            Dim nsamples As Integer = rows.Length
            Dim width As Integer = If(nsamples = 0, 0, rows(Scan0).Length)

            Call sanitizeNaN(rows)

            Dim u As Double()() = GetRandomMatrix(classCount, nsamples).ToArray
            Dim j_old As Double = -1
            Dim centers As Double()() = Nothing
            Dim j_new As Double
            Dim membership_diff As Double
            Dim [loop] As Integer = 0

            While True
                centers = GetCenters(classCount, m, u, rows, width).ToArray
                j_new = J(m, u, centers, rows)
                membership_diff = std.Abs(j_new - j_old)

                If j_old <> -1 AndAlso membership_diff < threshold Then
                    Exit While
                End If

                j_old = j_new

                If parallel Then
                    u = updateMembershipParallel(u, rows, centers, classCount, m)
                Else
                    Call updateMembership(u, rows, centers, classCount, m)
                End If

                [loop] += 1

                If [loop] > maxLoop Then
                    Exit While
                End If
            End While

            Return New CMeansResult With {
                .centers = centers,
                .membership = u
            }
        End Function

        Private Shared Sub sanitizeNaN(rows As Double()())
            For Each v As Double() In rows
                If v Is Nothing Then
                    Continue For
                End If

                For i As Integer = 0 To v.Length - 1
                    If v(i).IsNaNImaginary Then
                        v(i) = 0
                    End If
                Next
            Next
        End Sub

        Private Iterator Function GetRandomMatrix(classCount As Integer, nsamples As Integer) As IEnumerable(Of Double())
            For i As Integer = 0 To nsamples - 1
                Dim c As Double() = New Double(classCount - 1) {}

                For j As Integer = 0 To c.Length - 1
                    c(j) = randf.randf(0, 1 - c.Sum())

                    If j = c.Length - 1 Then
                        c(j) = 1 - c.Sum()
                    End If
                Next

                Yield c
            Next
        End Function

        Public Iterator Function GetCenters(classCount As Integer,
                                            m As Double,
                                            u As Double()(),
                                            rows As Double()(),
                                            width As Integer) As IEnumerable(Of Double())

            Dim nsamples As Integer = rows.Length

            For i As Integer = 0 To classCount - 1
                Dim center As Double() = New Double(width - 1) {}

                For x As Integer = 0 To width - 1
                    Dim numerator As Double = 0
                    Dim denominator As Double = 0

                    For j As Integer = 0 To nsamples - 1
                        Dim w As Double = u(j)(i) ^ m

                        If w.IsNaNImaginary Then
                            w = 0
                        End If

                        Dim value As Double = If(rows(j) Is Nothing, 0, rows(j)(x))

                        If value.IsNaNImaginary Then
                            value = 0
                        End If

                        numerator += w * value
                        denominator += w
                    Next

                    If denominator = 0.0 Then
                        center(x) = 10000
                    Else
                        center(x) = numerator / denominator
                    End If
                Next

                Yield center
            Next
        End Function

        ''' <summary>
        ''' 目标函数 J
        ''' </summary>
        Public Function J(m As Double, u As Double()(), centers As Double()(), rows As Double()()) As Double
            Dim jsum As Double = 0

            For i As Integer = 0 To centers.Length - 1
                Dim sum As Double = 0

                For k As Integer = 0 To rows.Length - 1
                    sum += (u(k)(i) ^ m) * rows(k).SquareDistance(centers(i))
                Next

                jsum += sum
            Next

            Return jsum
        End Function

        Private Shared Function updateMembershipParallel(u As Double()(), rows As Double()(), centers As Double()(), classCount As Integer, m As Double) As Double()()
            Return Enumerable.Range(0, u.Length) _
                .AsParallel() _
                .AsOrdered() _
                .Select(Function(j) scanRow(centers, rows(j), classCount, m)) _
                .ToArray()
        End Function

        Private Shared Sub updateMembership(u As Double()(), rows As Double()(), centers As Double()(), classCount As Integer, m As Double)
            For j As Integer = 0 To rows.Length - 1
                Dim ui As Double() = scanRow(centers, rows(j), classCount, m)

                For i As Integer = 0 To classCount - 1
                    u(j)(i) = ui(i)
                Next
            Next
        End Sub

        Private Shared Function scanRow(centers As Double()(), entity As Double(), classCount As Integer, m As Double) As Double()
            Dim ui As Double() = New Double(classCount - 1) {}

            For i As Integer = 0 To classCount - 1
                Dim sumAll As Double = 0

                For k As Integer = 0 To classCount - 1
                    Dim a As Double = std.Sqrt(entity.SquareDistance(centers(i))) / std.Sqrt(entity.SquareDistance(centers(k)))
                    sumAll += a ^ (2 / (m - 1))
                Next

                ui(i) = 1 / sumAll

                If Double.IsNaN(ui(i)) Then
                    ui(i) = 1
                End If
            Next

            Return ui
        End Function
    End Class
End Namespace

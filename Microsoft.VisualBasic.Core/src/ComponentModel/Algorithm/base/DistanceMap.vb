#Region "Microsoft.VisualBasic::33c49b9db4261394205b041d38184368, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\base\DistanceMap.vb"

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
    '    Code Lines: 58 (61.05%)
    ' Comment Lines: 20 (21.05%)
    '    - Xml Docs: 65.00%
    ' 
    '   Blank Lines: 17 (17.89%)
    '     File Size: 3.72 KB


    '     Class DistanceMap
    ' 
    '         Properties: GetMap
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: MakeMxN, MakeNxN
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm.base

    Public Class DistanceMap(Of T)

        ReadOnly distanceMap As Double()()

        Default Public ReadOnly Property Distance(i As Integer, j As Integer) As Double
            Get
                Return distanceMap(i)(j)
            End Get
        End Property

        Default Public ReadOnly Property Distance(i As IIndexOf(Of Integer), j As IIndexOf(Of Integer)) As Double
            Get
                Return distanceMap(i.Address)(j.Address)
            End Get
        End Property

        Public ReadOnly Property GetMap As Double()()
            Get
                Return distanceMap.ToArray
            End Get
        End Property

        ''' <summary>
        ''' NxN distance matrix
        ''' </summary>
        ''' <param name="points"></param>
        ''' <param name="metric">
        ''' 具有对称性的距离计算公式，通常为欧几里得距离
        ''' </param>
        Sub New(points As IEnumerable(Of T), metric As Func(Of T, T, Double))
            distanceMap = MakeNxN(points.SafeQuery.ToArray, metric)
        End Sub

        ''' <summary>
        ''' MxN distance matrix
        ''' </summary>
        ''' <param name="pts1"></param>
        ''' <param name="pts2"></param>
        ''' <param name="metric">具有对称性的距离计算公式，通常为欧几里得距离</param>
        Sub New(pts1 As IEnumerable(Of T), pts2 As IEnumerable(Of T), metric As Func(Of T, T, Double))
            distanceMap = MakeMxN(pts1.SafeQuery.ToArray, pts2.SafeQuery.ToArray, metric)
        End Sub

        Private Function MakeNxN(pool As T(), metric As Func(Of T, T, Double)) As Double()()
            Dim n As Integer = pool.Length
            ' create NxN matrix
            Dim matrix As Double()() = RectangularArray.Matrix(Of Double)(n, n)

            ' 使用 Parallel.For 并行化外层循环（遍历行索引 i）
            System.Threading.Tasks.Parallel.For(0, n,
                Sub(i)
                    Dim dist As Double = 0
                    Dim vec As Double() = matrix(i)

                    ' 对角线元素：点到自身的距离（每个线程独立计算自己的 i）
                    vec(i) = metric(pool(i), pool(i))

                    ' 上三角部分（j > i）：仅计算当前行 i 的对应列
                    For j As Integer = i + 1 To n - 1
                        dist = metric(pool(i), pool(j))
                        vec(j) = dist
                        matrix(j)(i) = dist ' 利用对称性赋值下三角
                    Next
                End Sub)

            Return matrix
        End Function

        Private Function MakeMxN(pool1 As T(), pool2 As T(), metric As Func(Of T, T, Double)) As Double()()
            Dim M As Integer = pool1.Length
            Dim N As Integer = pool2.Length
            ' 创建 MxN 矩阵
            Dim matrix As Double()() = RectangularArray.Matrix(Of Double)(M, N)

            ' 使用 Parallel.For 并行化外层循环（遍历 pool1 的索引 i）
            System.Threading.Tasks.Parallel.For(0, M,
                Sub(i)
                    Dim vec As Double() = matrix(i)
                    Dim point1 As T = pool1(i)

                    ' 计算 pool1 中第 i 个点与 pool2 中所有点的距离
                    For j As Integer = 0 To N - 1
                        vec(j) = metric(point1, pool2(j))
                    Next
                End Sub)

            Return matrix
        End Function
    End Class
End Namespace

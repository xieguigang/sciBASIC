#Region "Microsoft.VisualBasic::5f4fb5d76a4f4edca3d938f781746071, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Repository\BloomFilter.vb"

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

'   Total Lines: 157
'    Code Lines: 71 (45.22%)
' Comment Lines: 65 (41.40%)
'    - Xml Docs: 76.92%
' 
'   Blank Lines: 21 (13.38%)
'     File Size: 7.18 KB


'     Class BloomFilter
' 
'         Properties: FalsePositiveRate
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: Contains, Create, GetHashPositions, OptimalK, OptimalM
' 
'         Sub: Add
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.Repository
Imports std = System.Math

Namespace ComponentModel.DataSourceModel.Repository

    ''' <summary>
    ''' 提供布隆过滤器的实现。
    ''' 布隆过滤器是一种空间效率很高的概率型数据结构，用于判断一个元素是否在集合中。
    ''' 它允许一定的假阳性，但绝不允许假阴性。
    ''' </summary>
    Public Class BloomFilter

        ''' <summary>
        ''' 使用 BitArray 来高效地存储位数组
        ''' </summary>
        ReadOnly _bitArray As BitArray

        ''' <summary>
        ''' length of the <see cref="BitArray"/> 
        ''' </summary>
        ''' <returns>位数组的长度</returns>
        Public ReadOnly Property m As Integer
        ''' <summary>
        ''' 哈希函数的数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property k As Integer

        ''' <summary>
        ''' 获取当前布隆过滤器的理论假阳性率。
        ''' </summary>
        ''' <param name="currentItemCount">当前已插入的元素数量。</param>
        ''' <returns>理论假阳性率。</returns>
        Public ReadOnly Property FalsePositiveRate(currentItemCount As Integer) As Double
            Get
                If currentItemCount < 0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(currentItemCount))
                Else
                    ' 公式: (1 - e^(-k*n/m))^k
                    Return std.Pow((1.0 - std.Exp(-_k * currentItemCount / _m)), _k)
                End If
            End Get
        End Property

        ''' <summary>
        ''' 使用指定的位数组大小和哈希函数数量初始化 BloomFilter 类的新实例。
        ''' </summary>
        ''' <param name="m">位数组的大小。必须为正数。</param>
        ''' <param name="k">哈希函数的数量。必须为正数。</param>
        Public Sub New(m As Integer, k As Integer)
            If m <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(m), "位数组大小必须为正数。")
            If k <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(k), "哈希函数数量必须为正数。")

            _m = m
            _k = k
            _bitArray = New BitArray(m)
        End Sub

        ''' <summary>
        ''' 根据期望的元素数量和目标假阳性率，自动计算最优参数并初始化 BloomFilter 类的新实例。
        ''' </summary>
        ''' <param name="expectedItems">预计将要插入的元素数量。必须为正数。</param>
        ''' <param name="desiredFalsePositiveRate">期望的假阳性率，范围在 (0, 1) 之间。</param>
        Public Shared Function Create(expectedItems As Integer, desiredFalsePositiveRate As Double) As BloomFilter
            If expectedItems <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(expectedItems), "期望元素数量必须为正数。")
            End If
            If desiredFalsePositiveRate <= 0.0 OrElse desiredFalsePositiveRate >= 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(desiredFalsePositiveRate), "假阳性率必须在 0 和 1 之间。")
            End If

            ' 根据公式计算最优的 m 和 k
            Dim optimalM = BloomFilter.OptimalM(expectedItems, desiredFalsePositiveRate)
            Dim optimalK = BloomFilter.OptimalK(optimalM, expectedItems)

            ' 调用主构造函数
            Return New BloomFilter(optimalM, optimalK)
        End Function

        ''' <summary>
        ''' 将指定的元素添加到布隆过滤器中。
        ''' </summary>
        ''' <param name="item">要添加的元素。不能为 Nothing。</param>
        Public Sub Add(item As String)
            Dim positions = GetHashPositions(item)

            For Each pos As Integer In positions
                _bitArray(pos) = True
            Next
        End Sub

        ''' <summary>
        ''' 检查布隆过滤器中是否可能包含指定的元素。
        ''' </summary>
        ''' <param name="item">要检查的元素。不能为 Nothing。</param>
        ''' <returns>如果元素可能存在，则为 true；如果元素绝对不存在，则为 false。</returns>
        Public Function Contains(item As String) As Boolean
            Dim positions = GetHashPositions(item)

            For Each pos As Integer In positions
                ' 只要有一个哈希位置不为1，就说明元素绝对不存在
                If Not _bitArray(pos) Then
                    Return False
                End If
            Next

            ' 如果所有位置都为1，则元素可能存在（存在假阳性可能）
            Return True
        End Function

        ''' <summary>
        ''' 根据给定的元素数量和假阳性率，计算最优的位数组大小。
        ''' </summary>
        ''' <param name="n">预计插入的元素数量。</param>
        ''' <param name="p">期望的假阳性率。</param>
        ''' <returns>最优的位数组大小。</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function OptimalM(n As Integer, p As Double) As Integer
            ' 公式: m = - (n * ln(p)) / (ln(2))^2
            Return CInt(-n * std.Log(p) / (std.Log(2) ^ 2))
        End Function

        ''' <summary>
        ''' 根据给定的位数组大小和元素数量，计算最优的哈希函数数量。
        ''' </summary>
        ''' <param name="m">位数组的大小。</param>
        ''' <param name="n">预计插入的元素数量。</param>
        ''' <returns>最优的哈希函数数量。</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function OptimalK(m As Integer, n As Integer) As Integer
            ' 公式: k = (m/n) * ln(2)
            Return CInt((m / n) * std.Log(2))
        End Function

        ''' <summary>
        ''' Knuth's multiplicative constant
        ''' </summary>
        Public Const prime As Integer = 2654435761

        ''' <summary>
        ''' 获取元素在位数组中映射的 k 个位置。
        ''' 这里使用双哈希技术来生成 k 个独立的哈希值。
        ''' </summary>
        ''' <param name="item">要哈希的元素。</param>
        ''' <returns>包含 k 个位置的整数数组。</returns>
        Private Function GetHashPositions(item As String) As Integer()
            ' 1. 将字符串转换为字节数组
            Dim data As Byte() = Encoding.UTF8.GetBytes(item)

            ' 2. 使用两个不同的种子进行双哈希
            Dim h1 As UInteger = MurmurHash.MurmurHashCode3_x86_32(data, 0) ' 种子 0
            Dim h2 As UInteger = MurmurHash.MurmurHashCode3_x86_32(data, &HFFFFFFFFUI) ' 种子 FFFFFFFF
            Dim positions(_k - 1) As Integer

            ' 3. 组合哈希以生成 k 个位置
            For i As Integer = 0 To _k - 1
                ' 组合哈希：Hash_i = (h1 + i * h2) mod m
                ' 确保 h2 不为 0，以避免所有位置都相同
                If h2 = 0 Then
                    h2 = 1
                End If

                positions(i) = (h1 + i * h2) Mod _m
            Next

            Return positions
        End Function
    End Class
End Namespace

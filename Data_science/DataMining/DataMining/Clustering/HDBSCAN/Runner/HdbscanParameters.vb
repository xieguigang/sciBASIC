#Region "Microsoft.VisualBasic::9205eea14b0b6c1c04737152af3b6093, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Runner\HdbscanParameters.vb"

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

    '   Total Lines: 17
    '    Code Lines: 14 (82.35%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (17.65%)
    '     File Size: 667 B


    '     Class HdbscanParameters
    ' 
    '         Properties: CacheDistance, Constraints, DataSet, DistanceFunction, Distances
    '                     MaxDegreeOfParallelism, MinClusterSize, MinPoints
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Distance
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Hdbscanstar
Imports Microsoft.VisualBasic.Math.SIMD

Namespace HDBSCAN.Runner

    ''' <summary>
    ''' 稠密距离矩阵的缓存策略。
    ''' </summary>
    ''' <remarks>
    ''' HDBSCAN 的核心距离与 MST 只需要"按需"读取距离，
    ''' 而完整缓存需要 <c>8·n²</c> 字节（n = 20000 时约 3.2 GB）。
    ''' 因此默认 <see cref="None"/>：完全不物化矩阵，峰值内存退回 O(n)。
    ''' </remarks>
    Public Enum DistanceCacheMode
        ''' <summary>
        ''' 不缓存，每次按需计算（默认，峰值内存 O(n)）。
        ''' </summary>
        None
        ''' <summary>
        ''' 缓存完整 n×n 矩阵（仅适合小数据集；超过 <see cref="HdbscanParameters(Of T).MaxCachedPoints"/> 会自动降级）。
        ''' </summary>
        Full
    End Enum

    Public Class HdbscanParameters(Of T)
        ''' <summary>
        ''' 距离缓存策略，默认不缓存。
        ''' </summary>
        Public Property DistanceCacheMode As DistanceCacheMode = DistanceCacheMode.None

        ''' <summary>
        ''' 允许物化距离矩阵的最大样本数（超过则自动降级为 <see cref="DistanceCacheMode.None"/>）。
        ''' </summary>
        ''' <remarks>
        ''' 默认 8000 对应约 <c>8000² × 8 B ≈ 512 MB</c>；
        ''' 显式设置 <see cref="DistanceCacheMode"/> 时仍受此上限保护，避免 OOM。
        ''' </remarks>
        Public Property MaxCachedPoints As Integer = 8000

        Public Property MaxDegreeOfParallelism As Integer = 1

        ''' <summary>
        ''' 距离矩阵缓存（仅在 <see cref="DistanceCacheMode"/> 不为 <see cref="DistanceCacheMode.None"/> 时填充）。
        ''' </summary>
        Public Property Distances As Double()()

        ''' <summary>
        ''' 【兼容保留】是否缓存距离矩阵。
        ''' </summary>
        ''' <remarks>
        ''' 读写这个属性等价于读写 <see cref="DistanceCacheMode"/>（<c>True</c> → <see cref="DistanceCacheMode.Full"/>）。
        ''' 默认值已由 <c>True</c> 改为 <c>False</c>：n×n 物化在大数据集上会直接 OOM。
        ''' </remarks>
        Public Property CacheDistance As Boolean
            Get
                Return DistanceCacheMode <> DistanceCacheMode.None
            End Get
            Set(value As Boolean)
                DistanceCacheMode = If(value, DistanceCacheMode.Full, DistanceCacheMode.None)
            End Set
        End Property

        Public Property DataSet As T()
        Public Property DistanceFunction As IDistanceCalculator(Of T)

        Public Property MinPoints As Integer
        Public Property MinClusterSize As Integer
        Public Property Constraints As List(Of HdbscanConstraint)
    End Class
End Namespace

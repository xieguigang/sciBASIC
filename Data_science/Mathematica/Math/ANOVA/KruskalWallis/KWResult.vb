#Region "Microsoft.VisualBasic::3789c41824db0cf08ca6720efdf2917e, Data_science\Mathematica\Math\ANOVA\KruskalWallis\KWResult.vb"

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

    '   Total Lines: 43
    '    Code Lines: 15 (34.88%)
    ' Comment Lines: 16 (37.21%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (27.91%)
    '     File Size: 1.49 KB


    ' Structure KWResult
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Kruskal-Wallis 检验结果结构体，存储单个分类单元（物种/OTU）的检验统计量
''' </summary>
Public Structure KWResult
    ''' <summary>分类单元名称（物种名或OTU编号）</summary>
    Public TaxonName As String

    ''' <summary>Kruskal-Wallis H 统计量（已做结值校正）</summary>
    Public HStatistic As Double

    ''' <summary>未校正的 H 统计量（仅用于对比参考）</summary>
    Public HUncorrected As Double

    ''' <summary>结值校正因子 C，C = 1 - Σ(ti³ - ti) / (N³ - N)</summary>
    Public TieCorrectionFactor As Double

    ''' <summary>自由度 df = k - 1（k 为组数）</summary>
    Public DegreesOfFreedom As Integer

    ''' <summary>卡方分布 p 值</summary>
    Public PValue As Double

    ''' <summary>总样本数 N</summary>
    Public TotalN As Integer

    ''' <summary>组数 k</summary>
    Public GroupCount As Integer

    ''' <summary>各组样本量数组</summary>
    Public GroupSizes As Integer()

    ''' <summary>各组平均秩数组</summary>
    Public GroupMeanRanks As Double()

    ''' <summary>各组秩和数组</summary>
    Public GroupRankSums As Double()

    ''' <summary>检验是否有效（样本量不足等原因可能导致无效）</summary>
    Public IsValid As Boolean

    ''' <summary>无效原因说明（当 IsValid=False 时给出原因）</summary>
    Public InvalidReason As String
End Structure

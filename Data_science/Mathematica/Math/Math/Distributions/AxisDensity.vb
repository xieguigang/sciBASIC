#Region "Microsoft.VisualBasic::99d7c36e9081a8fee529da57c2e54ccd, Data_science\Mathematica\Math\Math\Distributions\AxisDensity.vb"

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

    '   Total Lines: 45
    '    Code Lines: 25 (55.56%)
    ' Comment Lines: 14 (31.11%)
    '    - Xml Docs: 78.57%
    ' 
    '   Blank Lines: 6 (13.33%)
    '     File Size: 2.28 KB


    '     Module AxisDensity
    ' 
    '         Function: GetClusters, MAD
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Math.Statistics.Linq

Namespace Distributions

    Public Module AxisDensity

        <Extension>
        Public Iterator Function GetClusters(axis As IEnumerable(Of Double)) As IEnumerable(Of DoubleTagged(Of Double()))
            Dim sortted = axis.OrderBy(Function(xi) xi).ToArray
            Dim diff As Double() = NumberGroups.diff(sortted)
            Dim threshold As Double = diff.Quartile.Q3

            For Each group As NamedCollection(Of Double) In sortted.GroupBy(offset:=threshold)
                Yield New DoubleTagged(Of Double())(Val(group.name), group.value)
            Next
        End Function

        ''' <summary>
        ''' MAD（Median Absolute Deviation，中位数绝对偏差）是一种鲁棒的统计量，用于衡量单变量数据的离散程度。其核心思想是通过中位数计算偏差，避免异常值对结果的影响。数学定义为：
        ''' 
        ''' ```
        ''' MAD=median(∣Xi−median(X)∣)
        ''' ```
        ''' 
        ''' 其中 X表示基因在所有样本中的表达值向量，Xi为单个样本的表达值，median(X)是基因表达值的中位数。
        ''' 与标准差不同，MAD使用中位数而非均值，因此不受极端值干扰。例如，若某基因在多数样本中表达稳定，但个别样本异常高/低，标准差会显著增大，而MAD几乎不变。
        ''' 
        ''' WGCNA等共表达网络分析中，需筛选高变异基因（如取MAD值最高的前5000个基因）。高MAD值表明基因表达在样本间波动大，可能具有生物学意义（如调控关键通路）。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MAD(x As Vector) As Double
            Dim delta_median As Vector = (x - x.Median).Abs
            Dim result As Double = delta_median.Median

            Return result
        End Function
    End Module
End Namespace

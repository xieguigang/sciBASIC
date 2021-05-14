#Region "Microsoft.VisualBasic::149889335b38136e0d3eccf502b78876, Microsoft.VisualBasic.Core\src\Extensions\Math\Information\Entropy.vb"

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

    '     Module Entropy
    ' 
    '         Function: ShannonEnt, ShannonEntropy
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Namespace Math.Information

    ''' <summary>
    ''' 信息熵越大表示所含信息量越多
    ''' </summary>
    Public Module Entropy

        ''' <summary>
        ''' 计算出目标序列的香农信息熵
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="collection"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 计算公式
        ''' 
        ''' ```
        ''' H(x) = E[ I(xi) ] 
        '''      = E[ log(2, 1/p(xi)) ] 
        '''      = -∑ p(xi)log(2, p(xi))  (i=1, 2, ..., n)
        ''' ```
        ''' 
        ''' 其中，``x``表示随机变量，与之相对应的是所有可能输出的集合，定义为符号集，随机变量的输出用``x``表示。
        ''' ``P(x)``表示输出概率函数。变量的不确定性越大，熵也就越大，把它搞清楚所需要的信息量也就越大.
        ''' </remarks>
        <Extension>
        Public Function ShannonEnt(Of T)(collection As IEnumerable(Of T)) As Double
            Dim distincts = (From x As T In collection Group x By x Into Count).ToArray
            Dim numEntries% = Aggregate g In distincts Into Sum(g.Count)
            Dim probs = From item In distincts Select item.Count / numEntries
            Dim entropy# = ShannonEntropy(probs)

            Return entropy
        End Function

        ''' <summary>
        ''' 直接从一个概率向量之中计算出香农信息熵
        ''' </summary>
        ''' <param name="probs">Sum of this probability vector must equals to 1</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function ShannonEntropy(probs As IEnumerable(Of Double)) As Double
            Dim entropy# = Aggregate prob As Double
                           In probs
                           Where prob > 0  ' 因为是求和，所以prob等于零的时候，乘上ln应该也是零的，因为零对求和无影响，所以在这里直接使用where跳过零了
                           Let ln = stdNum.Log(prob, newBase:=2)
                           Into Sum(prob * ln)
            ' 和的负数，注意在这里最后的结果还需要乘以-1
            ' 有一个负号
            Return -entropy
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::c928f81255f4f5e4d7061e1ecf88de0c, Microsoft.VisualBasic.Core\src\Extensions\Math\Information\Entropy.vb"

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

    '   Total Lines: 105
    '    Code Lines: 28 (26.67%)
    ' Comment Lines: 71 (67.62%)
    '    - Xml Docs: 85.92%
    ' 
    '   Blank Lines: 6 (5.71%)
    '     File Size: 4.75 KB


    '     Module Entropy
    ' 
    '         Function: Gini, ShannonEnt, ShannonEntropy
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
        ''' Calculate entropy value.
        ''' 
        ''' (直接从一个概率向量之中计算出香农信息熵)
        ''' </summary>
        ''' <param name="probs">Sum of this probability vector must equals to 1, Histogram array.</param>
        ''' <returns></returns>
        ''' <remarks><para>
        ''' The input array is treated as histogram, i.e. its
        ''' indexes are treated as values of stochastic function, but
        ''' array values are treated as "probabilities" (total amount of
        ''' hits).</para>
        ''' 
        ''' <para>Sample usage:</para>
        ''' <code>
        ''' // create histogram array with 2 values of equal probabilities
        ''' int[] histogram1 = new int[2] { 3, 3 };
        ''' // calculate entropy
        ''' double entropy1 = Statistics.Entropy( histogram1 );
        ''' // output it (1.000)
        ''' Console.WriteLine( "entropy1 = " + entropy1.ToString( "F3" ) );
        ''' 
        ''' // create histogram array with 4 values of equal probabilities
        ''' int[] histogram2 = new int[4] { 1, 1, 1, 1 };
        ''' // calculate entropy
        ''' double entropy2 = Statistics.Entropy( histogram2 );
        ''' // output it (2.000)
        ''' Console.WriteLine( "entropy2 = " + entropy2.ToString( "F3" ) );
        ''' 
        ''' // create histogram array with 4 values of different probabilities
        ''' int[] histogram3 = new int[4] { 1, 2, 3, 4 };
        ''' // calculate entropy
        ''' double entropy3 = Statistics.Entropy( histogram3 );
        ''' // output it (1.846)
        ''' Console.WriteLine( "entropy3 = " + entropy3.ToString( "F3" ) );
        ''' </code>
        ''' </remarks>
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

        ''' <summary>
        ''' 基尼系数的选择的标准就是每个子节点达到最高的纯度，即落在子节点中的所有观察都属于同一个分类，
        ''' 此时基尼系数最小，纯度最高，不确定度最小。
        ''' 
        ''' 基尼指数越大，说明不确定性就越大；基尼系数越小，不确定性越小，数据分割越彻底，越干净。
        ''' </summary>
        ''' <param name="p">
        ''' the data probability
        ''' </param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Gini(p As IEnumerable(Of Double)) As Double
            Return 1 - (Aggregate pk As Double In p Into Sum(pk ^ 2))
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::65cd86fde2a509436126c3fdf80bfe45, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Importance.vb"

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

    '   Total Lines: 70
    '    Code Lines: 40
    ' Comment Lines: 20
    '   Blank Lines: 10
    '     File Size: 2.94 KB


    '     Module Helpers
    ' 
    '         Function: Importance, SumWeight
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile

Namespace NeuralNetwork

    Partial Module Helpers

        ''' <summary>
        ''' 计算输入节点的重要性
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 1. 首先将突触链接的权重的绝对值进行quantile计算
        ''' 2. 然后删除20% quantile以下的所有节点连接
        ''' 3. 从input开始添加权重直到某一个输出节点
        ''' 4. 某一个input的权重除以最大的权重,得到归一化的相对值
        ''' </remarks>
        <Extension>
        Public Iterator Function Importance(model As StoreProcedure.NeuralNetwork, Optional q# = 0.2) As IEnumerable(Of DataSet)
            Dim edges = model.connections.Shadows
            Dim allWeights As Vector = edges!w.Abs
            Dim quantile As QuantileEstimationGK = allWeights.GKQuantile
            Dim threshold# = quantile.Query(q)

            ' edge cutoff
            model.connections = edges(allWeights >= threshold)

            ' 枚举每一个output
            For i As Integer = 0 To model.outputlayer.size - 1

            Next
        End Function

        ''' <summary>
        ''' 只计算输入节点和第一层隐藏层的节点之间的权重的和
        ''' 
        ''' > https://stats.stackexchange.com/questions/261008/deep-learning-how-do-i-know-which-variables-are-important
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns>得到的是最原始的权重的和,可以除上最大的权重和来得到相对权重</returns>
        <Extension>
        Public Iterator Function SumWeight(model As StoreProcedure.NeuralNetwork, Optional factorNames$() = Nothing) As IEnumerable(Of NamedValue(Of Double))
            Dim inputs$() = model.inputlayer.AsEnumerable.ToArray
            Dim firstHidden = model.hiddenlayers _
                .AsEnumerable _
                .OrderBy(Function(l) Val(l.id)) _
                .First _
                .AsEnumerable _
                .Indexing

            factorNames = factorNames Or inputs.AsDefault

            For Each input As SeqValue(Of String) In inputs.SeqIterator
                Dim inputId As String = input.value
                Dim allw = model.connections _
                    .Where(Function(n) n.in = inputId AndAlso n.out Like firstHidden) _
                    .Shadows!w _
                    .Abs

                Yield New NamedValue(Of Double)(factorNames(input), allw.Sum)
            Next
        End Function
    End Module
End Namespace

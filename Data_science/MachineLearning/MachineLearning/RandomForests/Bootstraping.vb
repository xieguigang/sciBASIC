#Region "Microsoft.VisualBasic::7d1578a6d1a8a4e72f65503f0e5eff63, sciBASIC#\Data_science\MachineLearning\MachineLearning\RandomForests\Bootstraping.vb"

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

    '   Total Lines: 51
    '    Code Lines: 32
    ' Comment Lines: 15
    '   Blank Lines: 4
    '     File Size: 2.03 KB


    '     Module RandomForestsBootstraping
    ' 
    '         Function: Bagging, Sampling
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.DecisionTree.Data
Imports Microsoft.VisualBasic.Math.Distributions

Namespace RandomForests

    <HideModuleName>
    Public Module RandomForestsBootstraping

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="trainingSet"></param>
        ''' <param name="n">随机森林之中的决策树的数量</param>
        ''' <param name="size">随机采样得到的子数据集内的样本数量</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Bagging(trainingSet As Entity(), n%, size%) As IEnumerable(Of Entity())
            Return Bootstraping _
                .Samples(Of Entity)(trainingSet, size, n) _
                .Select(Function(subSample)
                            Return subSample.value _
                                .Select(Function(entity)
                                            Return DirectCast(entity.Clone, Entity)
                                        End Function) _
                                .ToArray
                        End Function)
        End Function

        ''' <summary>
        ''' 每一个<see cref="DataTable"/>结果输出都是一颗新的决策树
        ''' </summary>
        ''' <param name="trainingSet"></param>
        ''' <param name="n%"></param>
        ''' <param name="size%"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function Sampling(trainingSet As DataTable, n%, size%) As IEnumerable(Of DataTable)
            For Each bag As Entity() In trainingSet.rows.Bagging(n, size)
                Yield New DataTable With {
                    .rows = bag,
                    .headers = trainingSet _
                        .headers _
                        .ToArray
                }
            Next
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::dac588f9d8f7978405698987159e547f, sciBASIC#\CLI_tools\MLkit\ANN\Config.vb"

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
    '    Code Lines: 27
    ' Comment Lines: 33
    '   Blank Lines: 10
    '     File Size: 2.83 KB


    ' Class Config
    ' 
    '     Properties: [Default], default_active, dropoutRate, hidden_size, hiddens_active
    '                 initializer, input_active, iterations, layerNormalize, learnRate
    '                 learnRateDecay, minErr, momentum, normalize, output_active
    '                 scattered, selective, truncate
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.Math.Scripting

Public Class Config

    <DataFrameColumn> Public Property learnRate As Double = 0.1
    <DataFrameColumn> Public Property momentum As Double = 0.9

    ''' <summary>
    ''' <see cref="Methods"/>
    ''' </summary>
    ''' <returns></returns>
    <DataFrameColumn> Public Property normalize As String = Methods.NormalScaler.ToString

    ''' <summary>
    ''' random表示随机初始化
    ''' 其他的任意数值则会将所有的数据都统一初始化为同一个数值
    ''' </summary>
    ''' <returns></returns>
    <DataFrameColumn> Public Property initializer As String = "random"
    <DataFrameColumn> Public Property learnRateDecay As Double = 0.0000000001
    <DataFrameColumn> Public Property iterations As Integer = 10000
    <DataFrameColumn> Public Property minErr As Double = 0.01
    <DataFrameColumn> Public Property truncate As Double = -1

#Region "所使用的激活函数的配置"
    ''' <summary>
    ''' ``func(args)``, using parser <see cref="FuncParser.TryParse(String)"/>
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' <see cref="input_active"/>, <see cref="hiddens_active"/>, <see cref="output_active"/>为空值属性的话,
    ''' 会使用这个属性值为默认值
    ''' </remarks>
    <DataFrameColumn> Public Property default_active As String = "Sigmoid()"

    <DataFrameColumn> Public Property input_active As String
    ''' <summary>
    ''' 推荐<see cref="ReLU"/>
    ''' </summary>
    ''' <returns></returns>
    <DataFrameColumn> Public Property hiddens_active As String
    <DataFrameColumn> Public Property output_active As String
#End Region

    ''' <summary>
    ''' ``a,b,c``使用逗号分隔的隐藏层每一层网络的节点数量的列表
    ''' </summary>
    ''' <returns></returns>
    <DataFrameColumn> Public Property hidden_size As String = "100,100,100"

    <DataFrameColumn> Public Property selective As String = "no"
    <DataFrameColumn> Public Property scattered As String = "no"
    ''' <summary>
    ''' 只针对隐藏层有意义
    ''' </summary>
    ''' <returns></returns>
    <DataFrameColumn> Public Property layerNormalize As String = "no"
    ''' <summary>
    ''' [0, 1]之间, 默认值为0表示不运行于dropout模式之下
    ''' </summary>
    ''' <returns></returns>
    <DataFrameColumn> Public Property dropoutRate As Double = 0

    Public Shared ReadOnly Property [Default] As [Default](Of Config) = New Config

End Class

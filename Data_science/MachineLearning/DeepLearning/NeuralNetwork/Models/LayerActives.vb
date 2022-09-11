#Region "Microsoft.VisualBasic::3399628be965507adc35af828e7ea82d, sciBASIC#\Data_science\MachineLearning\DeepLearning\NeuralNetwork\Models\LayerActives.vb"

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

    '   Total Lines: 63
    '    Code Lines: 37
    ' Comment Lines: 19
    '   Blank Lines: 7
    '     File Size: 2.33 KB


    '     Class LayerActives
    ' 
    '         Properties: hiddens, input, output
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: FromXmlModel, GetDefaultConfig, GetXmlModels
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork.Activations

    ''' <summary>
    ''' 每一层神经元都设立单独的激活函数
    ''' </summary>
    ''' <remarks>
    ''' 推荐的激活函数配置为:
    ''' 
    ''' + input: ``<see cref="Sigmoid"/>``
    ''' + hidden: ``<see cref="SigmoidFunction"/>``
    ''' + output: ``<see cref="Sigmoid"/>``
    ''' </remarks>
    Public Class LayerActives

        Public Property input As IActivationFunction
        ''' <summary>
        ''' 隐藏层都公用同一种激活函数?
        ''' </summary>
        ''' <returns></returns>
        Public Property hiddens As IActivationFunction
        Public Property output As IActivationFunction

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetXmlModels() As Dictionary(Of String, ActiveFunction)
            Return New Dictionary(Of String, ActiveFunction) From {
                {NameOf(input), input.Store},
                {NameOf(hiddens), hiddens.Store},
                {NameOf(output), output.Store}
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromXmlModel(functions As Dictionary(Of String, ActiveFunction)) As LayerActives
            Return New LayerActives With {
                .hiddens = functions!hiddens.Function,
                .input = functions!input.Function,
                .output = functions!output.Function
            }
        End Function

        ''' <summary>
        ''' 默认是都使用<see cref="Sigmoid"/>激活函数
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetDefaultConfig() As [Default](Of LayerActives)
            Return New LayerActives With {
                .input = New Sigmoid,
                .hiddens = New SigmoidFunction,
                .output = New Sigmoid
            }
        End Function
    End Class
End Namespace

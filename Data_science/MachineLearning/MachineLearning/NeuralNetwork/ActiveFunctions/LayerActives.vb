#Region "Microsoft.VisualBasic::1e531d33158faef3700ec91413c73bcb, Data_science\MachineLearning\MachineLearning\NeuralNetwork\ActiveFunctions\LayerActives.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
        Public Shared Function GetDefaultConfig() As [Default](Of  LayerActives)
            Return New LayerActives With {
                .input = New Sigmoid,
                .hiddens = New SigmoidFunction,
                .output = New Sigmoid
            }
        End Function
    End Class
End Namespace

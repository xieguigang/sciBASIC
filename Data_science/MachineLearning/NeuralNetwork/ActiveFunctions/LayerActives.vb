Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork.Activations

    ''' <summary>
    ''' 每一层神经元都设立单独的激活函数
    ''' </summary>
    Public Class LayerActives

        Public Property input As IActivationFunction
        ''' <summary>
        ''' 隐藏层都公用同一种激活函数?
        ''' </summary>
        ''' <returns></returns>
        Public Property hiddens As IActivationFunction
        Public Property output As IActivationFunction

        Public Function GetXmlModels() As Dictionary(Of String, ActiveFunction)
            Return New Dictionary(Of String, ActiveFunction) From {
                {NameOf(input), input.Store},
                {NameOf(hiddens), hiddens.Store},
                {NameOf(output), output.Store}
            }
        End Function

        ''' <summary>
        ''' 默认是都使用<see cref="Sigmoid"/>激活函数
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetDefaultConfig() As DefaultValue(Of LayerActives)
            Return New LayerActives With {
                .input = New Sigmoid,
                .hiddens = New Sigmoid,
                .output = New Sigmoid
            }
        End Function
    End Class
End Namespace
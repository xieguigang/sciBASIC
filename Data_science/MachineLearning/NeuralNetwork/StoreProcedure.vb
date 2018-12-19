Imports System.Runtime.CompilerServices

Namespace NeuralNetwork

    Public Module StoreProcedureExtensions

        ''' <summary>
        ''' 将保存在Xml文件之中的已经训练好的模型加载为人工神经网络对象用来进行后续的分析操作
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadModel(model As StoreProcedure.NeuralNetwork) As Network

        End Function
    End Module
End Namespace
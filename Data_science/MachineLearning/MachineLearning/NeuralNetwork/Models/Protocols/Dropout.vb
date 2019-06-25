Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace NeuralNetwork.Protocols

    Module Dropout

        ''' <summary>
        ''' 只针对隐藏层进行随机删除操作
        ''' </summary>
        ''' <param name="model"></param>
        ''' <param name="percentage">
        ''' [0,1]之间,建议设置一个[0.3,0.6]之间的值, 这个参数表示被随机删除的节点的数量百分比,值越高,则剩下的神经元节点越少
        ''' </param>
        <Extension>
        Public Sub DoDropOut(model As Network, Optional percentage As Double = 0.5)
            For Each layer As Layer In model.HiddenLayer
                ' reset the status of all nodes
                For Each node As Neuron In layer
                    node.isDroppedOut = False
                Next
                ' drop out parts of the neuron nodes 
                ' in current network layer
                For Each node As Neuron In layer.TakeRandomly(layer.Neurons.Length * percentage)
                    node.isDroppedOut = True
                Next
            Next
        End Sub
    End Module
End Namespace
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace NeuralNetwork

    ''' <summary>
    ''' softmax output layer of the classify result.
    ''' </summary>
    Public NotInheritable Class SoftmaxLayer

        Private Sub New()
        End Sub

        Public Shared Sub CalculateValue(neurons As Neuron(),
                                         Optional parallel As Boolean = False,
                                         Optional truncate As Double = -1,
                                         Optional doDropOutMode As Boolean = False)
            Dim V As Double()

            If Not parallel Then
                Dim i As Integer = Scan0

                V = New Double(neurons.Length - 1) {}

                For Each neuron As Neuron In neurons
                    V(i) = neuron.InputSynapsesValueSum(doDropOutMode, truncate)
                    i += 1
                Next
            Else
                ' 在这里将结果值赋值到一个临时的匿名变量中
                ' 来触发这个并行调用表达式
                '
                ' 2019-1-14 因为在计算的时候，取的neuron.value是上一层网络的值
                ' 只是修改当前网络的节点值
                ' 并没有修改上一层网络的任何参数
                ' 所以在这里的并行是没有问题的
                V = (From neuron As SeqValue(Of Neuron)
                     In neurons.SeqIterator.AsParallel
                     Let run = neuron.value.InputSynapsesValueSum(doDropOutMode, truncate)
                     Order By neuron.i
                     Select run).ToArray
            End If

            V = Softmax(V)

            For i As Integer = 0 To V.Length - 1
                neurons(i).Value = neurons(i).activation.Function(V(i) + neurons(i).Bias)
            Next
        End Sub

        Public Shared Function Softmax(V As Double()) As Double()
            Dim EVj As Double = Aggregate Vj As Double In V Into Sum(stdNum.Exp(Vj))
            Dim smax As Double() = V _
                .Select(Function(Vi) stdNum.Exp(Vi) / EVj) _
                .ToArray

            Return smax
        End Function
    End Class
End Namespace
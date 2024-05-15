#Region "Microsoft.VisualBasic::ae438b94b78c20f584e509a4248d3898, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Models\SoftmaxLayer.vb"

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

    '   Total Lines: 68
    '    Code Lines: 44
    ' Comment Lines: 11
    '   Blank Lines: 13
    '     File Size: 2.52 KB


    '     Class SoftmaxLayer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Softmax
    ' 
    '         Sub: CalculateValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

            If EVj.IsNaNImaginary Then
                ' x / inf = 0
                Return New Double(V.Length - 1) {}
            End If

            Dim smax As Double() = V _
                .Select(Function(Vi)
                            Return If(Vi.IsNaNImaginary, 0, stdNum.Exp(Vi) / EVj)
                        End Function) _
                .ToArray

            Return smax
        End Function
    End Class
End Namespace

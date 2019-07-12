Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' A linear correlation system
''' </summary>
Public Class Correlation : Implements ICloneable(Of Correlation)

    Public Property B As Vector

    Dim sigmoid As New BipolarSigmoid

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Evaluate(X As Vector) As Double
        ' 通过sigmoid函数强制约束输出到[-1, 1]这个区间内
        ' 因为实际的X值可能会比较大，这样子累加之后的相关系数会非常高
        ' 故而会导致相应的系统变量的结果值很容易的就出现无穷大的结果值
        ' 添加了BipolarSigmoid函数的约束之后可以避免出现这种情况
        Return sigmoid.Function((B * X).Sum)
    End Function

    Public Overrides Function ToString() As String
        Return B.ToString
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Clone() As Correlation Implements ICloneable(Of Correlation).Clone
        Return New Correlation With {
            .B = New Vector(B.AsEnumerable)
        }
    End Function
End Class

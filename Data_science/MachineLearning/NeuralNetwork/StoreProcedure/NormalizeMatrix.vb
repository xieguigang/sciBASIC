Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Distributions

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' 进行所输入的样本数据的归一化的矩阵
    ''' </summary>
    Public Class NormalizeMatrix : Inherits XmlDataModel

        ''' <summary>
        ''' 每一个属性都具有一个归一化区间
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("matrix")>
        Public Property matrix As SampleDistribution()
        ''' <summary>
        ''' 属性名称列表,这个序列的长度是和<see cref="matrix"/>的长度一致的,并且元素的顺序一一对应的
        ''' </summary>
        ''' <returns></returns>
        Public Property names As String()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function NormalizeInput(sample As Sample) As Double()
            Static normalRange As DoubleRange = {0, 1}

            Return sample _
                .status _
                .Select(Function(x, i)
                            x = matrix(i).GetRange.ScaleMapping(x, normalRange)

                            If x.IsNaNImaginary Then
                                Return matrix(i).average
                            Else
                                Return x
                            End If
                        End Function) _
                .ToArray
        End Function

        Public Shared Function CreateFromSamples(samples As IEnumerable(Of Sample), names As IEnumerable(Of String)) As NormalizeMatrix
            With samples.ToArray
                Dim len% = .First.status.Length
                Dim index%
                Dim matrix As New List(Of SampleDistribution)
                Dim averages As New List(Of Double)
                Dim [property] As Double()

                For i As Integer = 0 To len - 1
                    index = i
                    [property] = .Select(Function(sample) sample.status(index)).ToArray
                    matrix += New SampleDistribution([property])
                    averages.Add([property].Average)
                Next

                Return New NormalizeMatrix With {
                    .matrix = matrix,
                    .names = names.ToArray
                }
            End With
        End Function
    End Class
End Namespace
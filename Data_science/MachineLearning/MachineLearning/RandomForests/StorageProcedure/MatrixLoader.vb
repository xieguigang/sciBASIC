Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
Imports Microsoft.VisualBasic.DataMining.DecisionTree.Data
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace RandomForests

    ''' <summary>
    ''' 随机森林有一种数据来源是与神经网络的数据集一致的
    ''' 只不过在这里解析样本数据使用的归一化方法始终是
    ''' 区间离散化方法
    ''' </summary>
    Public Module MatrixLoader

        <Extension>
        Public Function CreateTable(matrix As DataSet, outputName$) As DataTable
            Dim outIndex As Integer = matrix.output.IndexOf(outputName)

            If outIndex < 0 Then
                Throw New InvalidExpressionException($"Result output name '{outputName}' is not exists in trainingSet!")
            End If

            Dim headers As String() = matrix.NormalizeMatrix.names.AsList + outputName
            Dim trainingSet As Entity() = matrix _
                .PopulateNormalizedSamples(Methods.RangeDiscretizer) _
                .Select(Function(sample)
                            Return New Entity With {
                                .entityVector = sample.status _
                                    .vector _
                                    .AsCharacter(format:="G2") _
                                    .AsList + sample.target(outIndex).ToString
                            }
                        End Function) _
                .ToArray

            Dim table As New DataTable With {
                .headers = headers,
                .rows = trainingSet
            }

            Return table
        End Function
    End Module
End Namespace
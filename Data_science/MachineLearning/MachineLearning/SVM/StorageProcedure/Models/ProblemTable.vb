Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Linq

Namespace SVM.StorageProcedure

    Public Class SupportVector : Inherits DynamicPropertyBase(Of Double)
        Implements INamedValue

        Public Property id As String Implements INamedValue.Key
        Public Property labels As Dictionary(Of String, String)

    End Class

    Public Class ProblemTable

        Public Property vectors As SupportVector()

        ''' <summary>
        ''' the key collection of the support vector: <see cref="SupportVector.Properties"/> inputs.
        ''' </summary>
        ''' <returns></returns>
        Public Property DimensionNames As String()

        Public Function GetTopics() As String()
            ' 20200828
            ' 使用readonly属性会导致json反序列化出错
            ' 在这里修改为函数
            If vectors.IsNullOrEmpty Then
                Return {}
            End If

            Return vectors _
                .Select(Function(a) a.labels.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray
        End Function

        Public Function Clone() As ProblemTable
            Return New ProblemTable With {
                .DimensionNames = DimensionNames.ToArray,
                .vectors = vectors _
                    .Select(Function(vec)
                                Return New SupportVector With {
                                    .id = vec.id,
                                    .labels = New Dictionary(Of String, String)(vec.labels),
                                    .Properties = New Dictionary(Of String, Double)(vec.Properties)
                                }
                            End Function) _
                    .ToArray
            }
        End Function

        ''' <summary>
        ''' 获取所指定的<paramref name="topic"/>下的所有标签数据，不去重
        ''' </summary>
        ''' <param name="topic"></param>
        ''' <returns></returns>
        Public Function GetTopicLabels(topic As String) As String()
            Return vectors.Select(Function(a) a.labels(topic)).ToArray
        End Function

        Public Function GetProblem(topic As String) As Problem
            Dim inputs As New List(Of Node())
            Dim labels As New List(Of String)

            For Each vec As SupportVector In vectors
                Call DimensionNames _
                    .Select(Function(x, i) New Node(i + 1, vec(x))) _
                    .ToArray _
                    .DoCall(AddressOf inputs.Add)

                Call labels.Add(vec.labels(topic))
            Next

            Return New Problem With {
                .DimensionNames = DimensionNames,
                .MaxIndex = .DimensionNames.Length,
                .X = inputs _
                    .Select(Function(i) Node.Copy(i).ToArray) _
                    .ToArray,
                .Y = labels.ClassEncoder.ToArray
            }
        End Function

        ''' <summary>
        ''' row append
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Shared Function Append(a As ProblemTable, b As ProblemTable) As ProblemTable
            Dim union As SupportVector() = a.vectors _
                .JoinIterates(b.vectors) _
                .ToArray
            Dim names As String() = union _
                .Select(Function(vi) vi.Properties.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray

            Return New ProblemTable With {
                .DimensionNames = names,
                .vectors = union
            }
        End Function
    End Class
End Namespace
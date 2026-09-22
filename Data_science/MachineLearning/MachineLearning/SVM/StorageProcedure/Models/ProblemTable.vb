#Region "Microsoft.VisualBasic::dca0d8eb053aea50944ddbb57bb9b978, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\Models\ProblemTable.vb"

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

    '   Total Lines: 126
    '    Code Lines: 83 (65.87%)
    ' Comment Lines: 26 (20.63%)
    '    - Xml Docs: 88.46%
    ' 
    '   Blank Lines: 17 (13.49%)
    '     File Size: 4.67 KB


    '     Class SupportVector
    ' 
    '         Properties: id, labels
    ' 
    '     Class ProblemTable
    ' 
    '         Properties: dimensionNames, vectors
    ' 
    '         Function: Append, Clone, GetProblem, GetTopicLabels, GetTopics
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Linq

Namespace SVM.StorageProcedure

    ''' <summary>
    ''' data -> labels
    ''' </summary>
    Public Class SupportVector : Inherits DynamicPropertyBase(Of Double)
        Implements INamedValue

        ''' <summary>
        ''' The unique reference id of this support vector row.
        ''' </summary>
        ''' <returns>A string value.</returns>
        Public Property id As String Implements INamedValue.Key

        ''' <summary>
        ''' The expected class label of this sample row, the dictionary key is 
        ''' the topic name and the value is the class label of that topic.
        ''' </summary>
        ''' <returns>A dictionary which maps the topic name to its class label.</returns>
        Public Property labels As Dictionary(Of String, String)

    End Class

    ''' <summary>
    ''' A tabular training data model: each <see cref="SupportVector"/> row 
    ''' contains the feature values and the class labels of multiple topics.
    ''' </summary>
    Public Class ProblemTable

        ''' <summary>
        ''' The sample rows of this problem table.
        ''' </summary>
        ''' <returns>An array of the <see cref="SupportVector"/> objects.</returns>
        Public Property vectors As SupportVector()

        ''' <summary>
        ''' the key collection of the support vector: <see cref="SupportVector.Properties"/> inputs.
        ''' </summary>
        ''' <returns>An array of the feature dimension names.</returns>
        Public Property dimensionNames As String()

        ''' <summary>
        ''' Get all of the topic names which are defined by the 
        ''' <see cref="SupportVector.labels"/> of the sample rows.
        ''' </summary>
        ''' <returns>An array of the distinct topic names.</returns>
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

        ''' <summary>
        ''' Create a deep copy of this problem table.
        ''' </summary>
        ''' <returns>A new <see cref="ProblemTable"/> object with the same data.</returns>
        Public Function Clone() As ProblemTable
            Return New ProblemTable With {
                .dimensionNames = dimensionNames.ToArray,
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
        ''' <param name="topic">The name of the target topic.</param>
        ''' <returns>
        ''' The class label of each sample row under the <paramref name="topic"/>; 
        ''' the duplicated label values are kept.
        ''' </returns>
        ''' <exception cref="KeyNotFoundException">
        ''' Thrown when a sample row does not define the class label of the 
        ''' <paramref name="topic"/>.
        ''' </exception>
        Public Function GetTopicLabels(topic As String) As String()
            Return vectors _
                .Select(Function(a)
                            If a.labels.ContainsKey(topic) Then
                                Return a.labels(topic)
                            Else
                                Throw New KeyNotFoundException($"missing topic key '{topic}' for vector [{a.id}] for get svm classify result data!")
                            End If
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' create a problem model under the given <paramref name="topic"/>
        ''' </summary>
        ''' <param name="topic">The name of the target topic, which provides the class label values.</param>
        ''' <returns>A <see cref="Problem"/> object which is ready for the SVM training.</returns>
        Public Function GetProblem(topic As String) As Problem
            Dim inputs As New List(Of Node())
            Dim labels As New List(Of String)

            For Each vec As SupportVector In vectors
                Call dimensionNames _
                    .Select(Function(x, i) New Node(i + 1, vec(x))) _
                    .ToArray _
                    .DoCall(AddressOf inputs.Add)

                Call labels.Add(vec.labels(topic))
            Next

            Return New Problem With {
                .dimensionNames = dimensionNames,
                .maxIndex = .dimensionNames.Length,
                .X = inputs _
                    .Select(Function(i) Node.Copy(i).ToArray) _
                    .ToArray,
                .Y = labels.ClassEncoder.ToArray
            }
        End Function

        ''' <summary>
        ''' row append
        ''' </summary>
        ''' <param name="a">The first problem table.</param>
        ''' <param name="b">The second problem table which will be appended to the <paramref name="a"/>.</param>
        ''' <returns>
        ''' A new <see cref="ProblemTable"/> object which contains all of the 
        ''' sample rows of both <paramref name="a"/> and <paramref name="b"/>, the 
        ''' dimension names are the union of the columns of the two tables.
        ''' </returns>
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
                .dimensionNames = names,
                .vectors = union
            }
        End Function
    End Class
End Namespace

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
    '    Code Lines: 83
    ' Comment Lines: 26
    '   Blank Lines: 17
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

        Public Property id As String Implements INamedValue.Key
        Public Property labels As Dictionary(Of String, String)

    End Class

    Public Class ProblemTable

        Public Property vectors As SupportVector()

        ''' <summary>
        ''' the key collection of the support vector: <see cref="SupportVector.Properties"/> inputs.
        ''' </summary>
        ''' <returns></returns>
        Public Property dimensionNames As String()

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
        ''' <param name="topic"></param>
        ''' <returns></returns>
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
        ''' <param name="topic"></param>
        ''' <returns></returns>
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
                .dimensionNames = names,
                .vectors = union
            }
        End Function
    End Class
End Namespace

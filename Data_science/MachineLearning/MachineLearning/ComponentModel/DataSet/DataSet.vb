#Region "Microsoft.VisualBasic::73b3d6e670ff635929112a2133a7e93d, Data_science\MachineLearning\MachineLearning\ComponentModel\DataSet\DataSet.vb"

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

    '   Total Lines: 151
    '    Code Lines: 99
    ' Comment Lines: 33
    '   Blank Lines: 19
    '     File Size: 5.80 KB


    '     Class DataSet
    ' 
    '         Properties: DataSamples, NormalizeMatrix, output, OutputSize, Size
    '                     width
    ' 
    '         Function: createExtends, JoinSamples, PopulateNormalizedSamples, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.StoreProcedure

    ''' <summary>
    ''' A training dataset that stored in XML file.
    ''' </summary>
    ''' <remarks>
    ''' 一般只需要生成这个数据对象, 之后就可以直接使用这个对象来进行统一的训练代码调用即可
    ''' </remarks>
    Public Class DataSet : Inherits XmlDataModel

        ''' <summary>
        ''' the training data samples
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("sample")>
        Public Property DataSamples As SampleList

        ''' <summary>
        ''' 主要是对<see cref="Sample.label"/>输入向量进行``[0, 1]``区间内的归一化操作
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("normalization")>
        Public Property NormalizeMatrix As NormalizeMatrix

        ''' <summary>
        ''' The element names of output vector
        ''' </summary>
        ''' <returns></returns>
        Public Property output As String()

        ''' <summary>
        ''' 样本的矩阵大小：``[属性长度, 样本数量]``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Size
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Size With {
                    .Width = width,
                    .Height = DataSamples.size
                }
            End Get
        End Property

        Public ReadOnly Property width As Integer
            Get
                If NormalizeMatrix Is Nothing Then
                    Return DataSamples(Scan0).vector.Length
                Else
                    Return NormalizeMatrix.matrix.size
                End If
            End Get
        End Property

        ''' <summary>
        ''' 神经网络的输出节点的数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property OutputSize As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If output.IsNullOrEmpty Then
                    Return DataSamples(Scan0).target.Length
                Else
                    Return output.Length
                End If
            End Get
        End Property

        ''' <summary>
        ''' Populates all of the normalized training dataset from current matrix data object.
        ''' </summary>
        ''' <param name="dummyExtends">
        ''' This function will extends <see cref="Sample.target"/> when this parameter is greater than ZERO.
        ''' </param>
        ''' <returns></returns>
        Public Iterator Function PopulateNormalizedSamples(Optional method As Normalizer.Methods = Normalizer.Methods.NormalScaler,
                                                           Optional dummyExtends% = 0) As IEnumerable(Of Sample)
            Dim input#()
            Dim normSample As Sample

            For Each sample As Sample In DataSamples.items
                input = NormalizeMatrix.NormalizeInput(sample, method)
                normSample = New Sample(input, sample.target + createExtends(input, dummyExtends), sample.ID)

                If sample.vector.Any(AddressOf IsNaNImaginary) Then
                    Throw New InvalidProgramException("NaN value exists in your dataset: " & normSample.GetJson)
                End If

                Yield normSample
            Next
        End Function

        Private Shared Function createExtends(input As Double(), n%) As List(Of Double)
            Dim extends As New List(Of Double)

            For i As Integer = 0 To input.Length - 1
                For j As Integer = i + 1 To input.Length - 1
                    If extends > n - 1 Then
                        Exit For
                    Else
                        extends += input(i) * input(j)
                    End If
                Next
            Next

            Return extends
        End Function

        Public Shared Function JoinSamples(dataset As DataSet, samples As IEnumerable(Of Sample), Optional estimateQuantile As Boolean = True) As DataSet
            Dim union As Sample() = dataset.DataSamples _
                .AsEnumerable _
                .JoinIterates(samples) _
                .ToArray
            Dim outputNames As String() = dataset.output
            Dim inputNames As String() = dataset.NormalizeMatrix.names

            If outputNames.IsNullOrEmpty Then
                outputNames = union(Scan0).target _
                    .Select(Function(x, i) $"Y_{i + 1}") _
                    .ToArray
            End If
            If inputNames.IsNullOrEmpty Then
                inputNames = union(Scan0).vector _
                    .Select(Function(x, i) $"X_{i + 1}") _
                    .ToArray
            End If

            Return New DataSet With {
                .DataSamples = New SampleList With {
                    .items = union
                },
                .NormalizeMatrix = NormalizeMatrix.CreateFromSamples(samples:=union, inputNames, estimateQuantile),
                .output = outputNames
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"DataSet with {Size.Height} samples and {Size.Width} properties in each sample."
        End Function
    End Class
End Namespace

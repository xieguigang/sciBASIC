#Region "Microsoft.VisualBasic::0d0bbee649a0179f277a14d18229540f, Data_science\MachineLearning\MachineLearning\NeuralNetwork\StoreProcedure\DataSet.vb"

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

    '     Class Sample
    ' 
    '         Properties: ID, status, target
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class DataSet
    ' 
    '         Properties: DataSamples, NormalizeMatrix, OutputSize, Size
    ' 
    '         Function: createExtends, PopulateNormalizedSamples, ToString
    '         Class SampleList
    ' 
    '             Properties: items
    ' 
    '             Function: [Select], getCollection, getSize
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' A training dataset that stored in XML file.
    ''' </summary>
    ''' <remarks>
    ''' 一般只需要生成这个数据对象, 之后就可以直接使用这个对象来进行统一的训练代码调用即可
    ''' </remarks>
    Public Class DataSet : Inherits XmlDataModel

        <XmlElement("sample")>
        Public Property DataSamples As SampleList

        ''' <summary>
        ''' 主要是对<see cref="Sample.status"/>输入向量进行``[0, 1]``区间内的归一化操作
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("normalization")>
        Public Property NormalizeMatrix As NormalizeMatrix

        Public Class SampleList : Inherits ListOf(Of Sample)

            ''' <summary>
            ''' 样本列表
            ''' </summary>
            ''' <returns></returns>
            <XmlElement("sample")> Public Property items As Sample()

            Default Public ReadOnly Property Item(index As Integer) As Sample
                <MethodImpl(MethodImplOptions.AggressiveInlining)>
                Get
                    Return items(index)
                End Get
            End Property

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Protected Overrides Function getSize() As Integer
                Return items?.Length
            End Function

            Public Iterator Function [Select](Of T)(project As Func(Of Sample, Integer, T)) As IEnumerable(Of T)
                Dim i As VBInteger = Scan0

                For Each item As Sample In items
                    Yield project(item, ++i)
                Next
            End Function

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Shared Widening Operator CType(samples As Sample()) As SampleList
                Return New SampleList With {
                    .items = samples
                }
            End Operator

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Shared Widening Operator CType(samples As List(Of Sample)) As SampleList
                Return New SampleList With {
                    .items = samples.ToArray
                }
            End Operator

            Protected Overrides Function getCollection() As IEnumerable(Of Sample)
                Return items
            End Function
        End Class

        ''' <summary>
        ''' 样本的矩阵大小：``[属性长度, 样本数量]``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Size
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Size With {
                    .Width = DataSamples(Scan0).status.Length,
                    .Height = DataSamples.size
                }
            End Get
        End Property

        ''' <summary>
        ''' 神经网络的输出节点的数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property OutputSize As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return DataSamples(Scan0).target.Length
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dummyExtends%">
        ''' This function will extends <see cref="Sample.target"/> when this parameter is greater than ZERO.
        ''' </param>
        ''' <returns></returns>
        Public Iterator Function PopulateNormalizedSamples(Optional dummyExtends% = 0) As IEnumerable(Of Sample)
            Dim input#()

            For Each sample As Sample In DataSamples.items
                input = NormalizeMatrix.NormalizeInput(sample)
                sample = New Sample With {
                    .ID = sample.ID,
                    .status = input,
                    .target = sample.target + createExtends(input, dummyExtends)
                }

                Yield sample
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

        Public Overrides Function ToString() As String
            Return $"DataSet with {Size.Height} samples and {Size.Width} properties in each sample."
        End Function
    End Class
End Namespace

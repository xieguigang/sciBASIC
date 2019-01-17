#Region "Microsoft.VisualBasic::50ce1810adea8accac4d1c56a0dd119b, Data_science\MachineLearning\NeuralNetwork\StoreProcedure\Dataset.vb"

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
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports row = Microsoft.VisualBasic.Data.csv.IO.DataSet

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' The training dataset
    ''' </summary>
    Public Class Sample : Implements INamedValue

        ''' <summary>
        ''' 可选的数据集唯一标记信息
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("id")>
        Public Property ID As String Implements IKeyedEntity(Of String).Key

        ''' <summary>
        ''' Neuron network input parameters
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 属性值可能会很长,为了XML文件的美观,在这里使用element
        ''' </remarks>
        <XmlElement>
        Public Property status As Double()

        ''' <summary>
        ''' The network expected output values
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property target As Double()

        ''' <summary>
        ''' Create a new training dataset
        ''' </summary>
        ''' <param name="values">Neuron network input parameters</param>
        ''' <param name="targets">The network expected output values</param>
        Public Sub New(values#(), targets#())
            Me.status = values
            Me.target = targets
        End Sub

        ''' <summary>
        ''' Create a new empty training dataset
        ''' </summary>
        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{status.AsVector.ToString} => {target.AsVector.ToString}"
        End Function
    End Class

    ''' <summary>
    ''' A training dataset that stored in XML file.
    ''' </summary>
    ''' <remarks>
    ''' 一般只需要生成这个数据对象, 之后就可以直接使用这个对象来进行统一的训练代码调用即可
    ''' </remarks>
    Public Class DataSet : Inherits XmlDataModel

        <XmlElement("sample")>
        Public Property DataSamples As SampleList
        <XmlElement("normalization")>
        Public Property NormalizeMatrix As NormalizeMatrix

        Public Class SampleList : Inherits ListOf

            <XmlElement>
            Public Property items As Sample()

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
        End Class

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
        ''' 从csv文件数据之中读取和当前的数据集一样的元素顺序的向量用于预测分析
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetInput(data As row) As Double()
            Return NormalizeMatrix _
                .names _
                .Select(Function(key) data(key)) _
                .ToArray
        End Function

        Public Iterator Function PopulateNormalizedSamples() As IEnumerable(Of Sample)
            Dim input#()

            For Each sample As Sample In DataSamples.items
                input = NormalizeMatrix.NormalizeInput(sample)
                sample = New Sample With {
                    .ID = sample.ID,
                    .status = input,
                    .target = sample.target
                }

                Yield sample
            Next
        End Function

        Public Overrides Function ToString() As String
            Return $"DataSet with {Size.Height} samples and {Size.Width} properties in each sample."
        End Function
    End Class
End Namespace

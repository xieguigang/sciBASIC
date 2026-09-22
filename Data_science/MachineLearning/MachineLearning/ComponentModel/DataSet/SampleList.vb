#Region "Microsoft.VisualBasic::f67caf927f05b8304e84da41750f4450, Data_science\MachineLearning\MachineLearning\ComponentModel\DataSet\SampleList.vb"

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

    '   Total Lines: 90
    '    Code Lines: 67 (74.44%)
    ' Comment Lines: 7 (7.78%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (17.78%)
    '     File Size: 3.15 KB


    '     Class SampleList
    ' 
    '         Properties: items
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: [Select], CreateDataSet, getCollection, getSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace ComponentModel.StoreProcedure

    ''' <summary>
    ''' the <see cref="Sample"/> collection
    ''' </summary>
    Public Class SampleList : Inherits ListOf(Of Sample)

        ''' <summary>
        ''' 样本列表
        ''' </summary>
        ''' <returns>An array of the <see cref="Sample"/> objects.</returns>
        <XmlElement("sample")> Public Property items As Sample()

        ''' <summary>
        ''' Gets the <see cref="Sample"/> data at a specific index of this sample list.
        ''' </summary>
        ''' <param name="index">The zero based index of the target sample.</param>
        ''' <returns>A <see cref="Sample"/> object.</returns>
        Default Public ReadOnly Property Item(index As Integer) As Sample
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return items(index)
            End Get
        End Property

        ''' <summary>
        ''' Create an empty sample list.
        ''' </summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Create a sample list from a given sample collection.
        ''' </summary>
        ''' <param name="samples">A collection of the <see cref="Sample"/> data.</param>
        Sub New(samples As IEnumerable(Of Sample))
            items = samples.SafeQuery.ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function getSize() As Integer
            Return items?.Length
        End Function

        ''' <summary>
        ''' Project each sample of this sample list into a new form.
        ''' </summary>
        ''' <typeparam name="T">The type of the projection result.</typeparam>
        ''' <param name="project">
        ''' The projection function, its first parameter is the <see cref="Sample"/> 
        ''' data and its second parameter is the zero based index of that sample.
        ''' </param>
        ''' <returns>A sequence of the projection results.</returns>
        Public Iterator Function [Select](Of T)(project As Func(Of Sample, Integer, T)) As IEnumerable(Of T)
            Dim i As i32 = Scan0

            For Each item As Sample In items
                Yield project(item, ++i)
            Next
        End Function

        ''' <summary>
        ''' Convert a sample array as a <see cref="SampleList"/> object.
        ''' </summary>
        ''' <param name="samples">An array of the <see cref="Sample"/> data.</param>
        ''' <returns>A <see cref="SampleList"/> object which wraps the given sample array.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(samples As Sample()) As SampleList
            Return New SampleList With {
                .items = samples
            }
        End Operator

        ''' <summary>
        ''' Convert a sample <see cref="List(Of T)"/> as a <see cref="SampleList"/> object.
        ''' </summary>
        ''' <param name="samples">A list of the <see cref="Sample"/> data.</param>
        ''' <returns>A <see cref="SampleList"/> object which wraps the given sample list.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(samples As List(Of Sample)) As SampleList
            Return New SampleList With {
                .items = samples.ToArray
            }
        End Operator

        Protected Overrides Function getCollection() As IEnumerable(Of Sample)
            Return items
        End Function

        ''' <summary>
        ''' Create a training <see cref="DataSet"/> object from this sample list, 
        ''' the normalization matrix will be evaluated at the same time.
        ''' </summary>
        ''' <param name="inputNames">
        ''' The property names of the input vector; the names will be generated 
        ''' automatically in format ``input_%d`` when this parameter is ``Nothing``.
        ''' </param>
        ''' <param name="outputNames">
        ''' The property names of the output vector; the names will be generated 
        ''' automatically in format ``output_%d`` when this parameter is ``Nothing``.
        ''' </param>
        ''' <returns>A <see cref="DataSet"/> object which is ready for model training.</returns>
        Public Function CreateDataSet(Optional inputNames As IEnumerable(Of String) = Nothing, Optional outputNames As IEnumerable(Of String) = Nothing) As DataSet
            Dim namesOfOutput = outputNames.SafeQuery.ToArray
            Dim namesOfInput = inputNames.SafeQuery.ToArray

            If namesOfOutput.IsNullOrEmpty Then
                namesOfOutput = items(Scan0) _
                    .target _
                    .Select(Function(x, i) $"output_{i}") _
                    .ToArray
            End If
            If namesOfInput.IsNullOrEmpty Then
                namesOfInput = items(Scan0).vector _
                    .Select(Function(x, i) $"input_{i}") _
                    .ToArray
            End If

            Dim normalize As NormalizeMatrix = NormalizeMatrix.CreateFromSamples(samples:=items, namesOfInput)

            Return New DataSet With {
                .DataSamples = Me,
                .NormalizeMatrix = normalize,
                .output = namesOfOutput
            }
        End Function
    End Class
End Namespace

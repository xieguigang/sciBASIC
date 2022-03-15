#Region "Microsoft.VisualBasic::1094148389a9fd58a672b7acee33f857, sciBASIC#\Data_science\MachineLearning\MachineLearning\ComponentModel\DataSet\SampleList.vb"

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

    '   Total Lines: 80
    '    Code Lines: 62
    ' Comment Lines: 4
    '   Blank Lines: 14
    '     File Size: 2.90 KB


    '     Class SampleList
    ' 
    '         Properties: items
    ' 
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
            Dim i As i32 = Scan0

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

            Dim normalize As NormalizeMatrix = NormalizeMatrix.CreateFromSamples(items, namesOfInput)

            Return New DataSet With {
                .DataSamples = Me,
                .NormalizeMatrix = normalize,
                .output = namesOfOutput
            }
        End Function
    End Class
End Namespace

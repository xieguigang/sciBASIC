#Region "Microsoft.VisualBasic::3e869b448637bbcd87b6089df7240f9a, Data_science\MachineLearning\MachineLearning\DataSet\SampleList.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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

Namespace StoreProcedure

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
                namesOfInput = items(Scan0) _
                    .status _
                    .vector _
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

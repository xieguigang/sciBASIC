Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Table = Microsoft.VisualBasic.Data.csv.IO.DataSet

<HideModuleName>
Public Module DataSetExtensions

    ''' <summary>
    ''' We usually use this extension method for generates the demo test dataset.
    ''' </summary>
    ''' <param name="samples"></param>
    ''' <param name="inputNames"></param>
    ''' <param name="outputNames"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SampleSetCreator(samples As IEnumerable(Of Sample),
                                     Optional inputNames As IEnumerable(Of String) = Nothing,
                                     Optional outputNames As IEnumerable(Of String) = Nothing) As DataSet

        Return New SampleList With {
            .items = samples _
                .SafeQuery _
                .ToArray
        }.CreateDataSet(inputNames, outputNames)
    End Function

    ''' <summary>
    ''' Convert XML dataset to csv table
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="markOutput">All of the column name of the data output will be marked in format like ``[name]``.</param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ToTable(raw As DataSet, Optional markOutput As Boolean = False) As IEnumerable(Of Table)
        Dim inputNames As String() = raw.NormalizeMatrix.names
        Dim outputs As String() = raw.output
        Dim data As Dictionary(Of String, Double)
        Dim size As Integer = raw.DataSamples.size

        For Each sample As Sample In raw.DataSamples.AsEnumerable
            data = inputNames _
                .SeqIterator _
                .ToDictionary(Function(name) name.value,
                              Function(name)
                                  Return sample.status(name)
                              End Function)

            ' append output result to input data
            Call outputs _
                .SeqIterator _
                .DoEach(Sub(name)
                            ' output element name can not be duplicated with
                            ' the input name
                            If markOutput Then
                                Call data.Add($"[{name.value}]", sample.target(name))
                            Else
                                Call data.Add(name.value, sample.target(name))
                            End If
                        End Sub)

            size -= 1

            Yield New Table With {
                .ID = sample.ID,
                .Properties = data
            }
        Next

        If size > 0 Then
            Call $"Inconsistent sample size! {size} sample is missing!".Warning
        End If
    End Function
End Module

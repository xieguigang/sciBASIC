Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Table = Microsoft.VisualBasic.Data.csv.IO.DataSet

<HideModuleName>
Public Module DataSetExtensions

    <Extension>
    Public Iterator Function ToTable(raw As DataSet) As IEnumerable(Of Table)
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
                            Call data.Add(name.value, sample.target(name))
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

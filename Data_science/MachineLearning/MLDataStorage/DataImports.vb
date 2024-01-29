Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Module DataImports

    ''' <summary>
    ''' Convert dataframe object as the machine learning dataset
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="labels">The column field names for read vector as output labels</param>
    ''' <returns></returns>
    <Extension>
    Public Function [Imports](df As DataFrame, labels As String()) As DataSet
        Dim samples As New List(Of Sample)
        Dim features As New List(Of SampleDistribution)
        Dim featureNames As String() = df.featureNames _
            .Where(Function(lb) labels.IndexOf(lb) < 0) _
            .ToArray
        Dim feature As SampleDistribution
        Dim sample As Sample

        For Each field As String In featureNames
            feature = New SampleDistribution(
                data:=df.features(field).Numeric,
                estimateQuantile:=False
            )
            features.Add(feature)
        Next

        For i As Integer = 0 To df.nsamples - 1
            Dim idx As Integer = i
            Dim v As Double() = featureNames.Select(Function(name) CDbl(df.features(name).vector.GetValue(idx))).ToArray
            Dim label As Double() = labels.Select(Function(name) CDbl(df.features(name).vector.GetValue(idx))).ToArray

            sample = New Sample(v, label, df.rownames(i))
            samples.Add(sample)
        Next

        Return New DataSet With {
            .output = labels,
            .DataSamples = New SampleList With {
                .items = samples.ToArray
            },
            .NormalizeMatrix = New NormalizeMatrix With {
                .matrix = New XmlList(Of SampleDistribution) With {
                    .items = features.ToArray
                },
                .names = featureNames
            }
        }
    End Function

    <Extension>
    Private Iterator Function Numeric(v As FeatureVector) As IEnumerable(Of Double)
        For i As Integer = 0 To v.size - 1
            Yield CDbl(v.vector.GetValue(i))
        Next
    End Function

End Module

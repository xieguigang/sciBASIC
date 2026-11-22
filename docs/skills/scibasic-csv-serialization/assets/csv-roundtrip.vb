Imports System.Text
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Framework.DataFrame
Imports Microsoft.VisualBasic.Data.Framework.Extensions

''' <summary>
''' Demonstrates CSV/TSV object round-trip and NumericTable text I/O.
''' Requires the dataframework-netcore5.vbproj (Microsoft.VisualBasic.Data.Framework) reference.
''' </summary>
Public Module CsvDemo

    Public Sub Run()
        Dim a As New Person With {
            .name = "a",
            .age = 22,
            .data = New Dictionary(Of String, Double) From {{"bmi", 11}, {"weight", 33}}
        }

        ' CSV: name,age,bmi,weight / a,22,11,33
        Call {a}.SaveTo("data.csv")

        Dim people As List(Of Person) = "data.csv".LoadCsv(Of Person)()

        ' TSV variant
        Call people.SaveTo("data.tsv", tsv:=True)
        people = "data.tsv".LoadCsv(Of Person)(tsv:=True)

        ' Header renaming and deterministic metadata column order
        Call people.SaveTo("data2.csv",
                           layout:=New Dictionary(Of String, Integer) From {{"name", 0}, {"age", 1}},
                           reorderKeys:=1,
                           metaBlank:="NA")
        Dim people2 = "data2.csv".LoadCsv(Of Person)(metaBlank:="NA")

        ' NumericTable text I/O: labels round-trip via the label: prefix
        Dim table As New NumericTable(
            features:={ {1.0, 2.0}, {3.0, 4.0} },
            rowNames:={"s1", "s2"},
            featureNames:={"f1", "f2"})

        Call table.SetLabel("score", {0.5, 0.8})
        Call table.WriteCsv("numeric.csv")

        Dim restored As NumericTable = NumericTableIO.ReadCsv("numeric.csv")
        Dim df = restored.AsDataFrame()
        Dim back As NumericTable = df.AsNumericTable()
    End Sub

End Module

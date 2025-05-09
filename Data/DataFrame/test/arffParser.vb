Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.Framework
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Module arffParser

    Sub Main()
        Call writeTest()
    End Sub

    Sub readTest()
        Dim df As DataFrame = DataFrame.read_arff("G:\GCModeller\src\runtime\sciBASIC#\Data\Example\dataframe\Smile.arff")
        Dim df2 As DataFrame = DataFrame.read_arff("G:\GCModeller\src\runtime\sciBASIC#\Data\Example\dataframe\COP.arff")
        Dim df3 As DataFrame = DataFrame.read_arff("G:\GCModeller\src\runtime\sciBASIC#\Data\Example\dataframe\Aggregation.arff")

        Pause()
    End Sub

    Sub writeTest()
        Dim df As New DataFrame With {
            .name = "file-test",
            .description = "text data writer in different vectro data type",
            .features = New Dictionary(Of String, FeatureVector) From {
                {"numbers", New FeatureVector("numbers", randf.ExponentialRandomNumbers(2, 6))},
                {"flags", New FeatureVector("flags", {True, True, True, False, False, True})}
            },
            .rownames = {"# 1", "# 2", "# 3", "# 4", "# 5", "# 6"}
        }

        Dim text As New StringBuilder
        Dim writer As New StringWriter(text)

        Call DataFrame.write_arff(df, writer)
        Call writer.Flush()
        Call Console.WriteLine(text.ToString)

        Pause()
    End Sub
End Module

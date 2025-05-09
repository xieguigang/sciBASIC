Imports Microsoft.VisualBasic.Data.Framework

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
            .features = New Dictionary(Of String, FeatureVector)
        }
    End Sub
End Module

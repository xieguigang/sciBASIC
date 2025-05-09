Imports Microsoft.VisualBasic.Data.Framework

Module arffParser

    Sub Main()
        Dim df As DataFrame = DataFrame.read_arff("G:\GCModeller\src\runtime\sciBASIC#\Data\Example\dataframe\Smile.arff")

        Pause()
    End Sub
End Module

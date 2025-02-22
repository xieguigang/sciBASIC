Imports Microsoft.VisualBasic.MachineLearning.RandomForests
Imports Microsoft.VisualBasic.Scripting.Runtime

Module rf

    Sub Main()
        Dim y As New List(Of Double)
        Dim id As New List(Of String)
        Dim v As New List(Of Double())

        For Each line In "G:\GCModeller\src\runtime\sciBASIC#\Data_science\MachineLearning\MachineLearning\RandomForests\training_regression.txt".IterateAllLines

            Dim t = line.StringSplit("\s+")
            y.Add(Val(t(0)))
            id.Add(t(1))
            v.Add(t.Skip(2).AsDouble)
        Next

        Dim ref As New Data With {
            .attributeNames = v(0).Sequence(offSet:=1).Select(Function(i) $"#{i}").ToArray,
            .Genotype = v.ToArray,
            .ID = id.ToArray,
            .phenotype = y.ToArray
        }
        Dim tree As New RanFog
        Dim result = tree.Run(ref)

        Pause()
    End Sub
End Module

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports any = Microsoft.VisualBasic.Scripting

Public Module FileWriter

    <Extension>
    Public Sub WriteCsv(df As DataFrame, file As String)
        Using s As Stream = file.Open(FileMode.OpenOrCreate, doClear:=True)
            Call df.WriteCsv(s)
        End Using
    End Sub

    <Extension>
    Public Sub WriteCsv(df As DataFrame, file As Stream)
        Dim s As New StreamWriter(file, Encoding.UTF8)
        Dim names = df.featureNames
        Dim cols = names.Select(Function(c) df(c).Getter).ToArray
        Dim rownames = df.rownames
        Dim row As String() = New String(cols.Length) {}

        Call s.WriteLine("," & names.Select(Function(si) $"""{si}""").JoinBy(","))

        For i As Integer = 0 To rownames.Length - 1
            row(0) = rownames(i)

            For offset As Integer = 0 To cols.Length - 1
                row(offset + 1) = any.ToString(cols(offset)(i), "")
            Next

            Call s.WriteLine(row.Select(Function(si) $"""{si}""").JoinBy(","))
        Next

        Call s.Flush()
    End Sub
End Module

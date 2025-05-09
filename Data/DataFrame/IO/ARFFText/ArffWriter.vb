Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Linq

Namespace IO.ArffFile

    Public Module ArffWriter

        Public Sub WriteText(d As DataFrame, s As Stream)
            Using str As New StreamWriter(s)
                Call d.WriteText(str)
            End Using
        End Sub

        <Extension>
        Public Sub WriteText(d As DataFrame, s As TextWriter)
            Dim offsets As String() = d.featureNames

            Call s.WriteLine($"@relation {If(d.name, "no named").CLIToken}")
            Call s.WriteLine()

            If Not d.description.StringEmpty Then
                For Each line As String In d.description.LineTokens
                    Call s.WriteLine($"% {line}")
                Next

                Call s.WriteLine()
            End If

            For Each col As String In offsets
                Call s.WriteLine($"@attribute {col.CLIToken} {ArffWriter.desc(d(col))}")
            Next

            Call s.WriteLine()
            Call s.WriteLine("@data")

            Dim rows As Integer = d.nsamples

            For i As Integer = 0 To rows - 1
                Dim row As Object() = d.row(i)
                Dim line As String = New RowObject(row).AsLine

                Call s.WriteLine(line)
            Next

            Call s.Flush()
        End Sub

        Private Function desc(v As FeatureVector) As String
            Select Case v.type
                Case GetType(Integer) : Return "int"
                Case GetType(Long) : Return "long"
                Case GetType(Single) : Return "float"
                Case GetType(Double) : Return "numeric"
                Case GetType(Boolean) : Return "boolean"
                Case Else
                    Dim uniq = v.vector _
                        .AsObjectEnumerator _
                        .Distinct _
                        .ToArray

                    If v.size / uniq.Length > 10 Then
                        ' is enum
                        Return "{" & New RowObject(uniq).AsLine & "}"
                    Else
                        Return "chr"
                    End If
            End Select
        End Function
    End Module
End Namespace
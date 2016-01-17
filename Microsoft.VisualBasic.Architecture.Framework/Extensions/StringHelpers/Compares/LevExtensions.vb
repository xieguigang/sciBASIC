Imports System.Runtime.CompilerServices

Public Module LevExtensions

    <Extension> Public Sub GetMatches(Of T)(edits As DistResult, ref As T(), hyp As T(), ByRef refOUT As T(), ByRef hypOUT As T())
        Dim len As Integer = edits.DistEdits.Count("m"c)
        Dim idx As Integer = -1
        Dim iiiii As Integer = 0

        refOUT = New T(len - 1) {}
        hypOUT = New T(len - 1) {}

        For j As Integer = 0 To hyp.Length   ' 参照subject画列
            For i As Integer = 0 To ref.Length  ' 参照query画行
                If edits.IsPath(i, j) Then
                    Dim ch As String = edits.DistEdits.Get(idx.MoveNext)

                    If ch = "m"c Then
                        refOUT(iiiii) = ref(i)
                        hypOUT(iiiii) = hyp(j - 1)

                        iiiii += 1
                    End If
                End If
            Next
        Next
    End Sub
End Module

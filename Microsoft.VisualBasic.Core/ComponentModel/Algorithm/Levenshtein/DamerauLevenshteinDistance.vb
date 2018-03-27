Imports Microsoft.VisualBasic.Text.Levenshtein.LevenshteinDistance

Namespace ComponentModel.Algorithm.DynamicProgramming

    Public Module DamerauLevenshteinDistance

        Public Function DamerauLevenshtein(Of T)(down As T(), across As T(), equals As Equals(Of T), insert#, remove#, substitute As Func(Of T, T, Double), transpose As Func(Of T, T, Double)) As Double
            Dim matrix As New List(Of Double())

            For i As Integer = 0 To down.Length - 1
                Dim ds As Double() = New Double(across.Length - 1) {}
                Dim d = down(i)

                For j As Integer = 0 To across.Length - 1
                    Dim a = across(j)

                    If i = 0 AndAlso j = 0 Then
                        ds(j) = 0
                    ElseIf i = 0 Then
                        ds(j) = ds(j - 1) + insert
                    ElseIf j = 0 Then
                        ds(j) = matrix(i - 1)(j) + remove
                    Else
                        ds(j) = {
                            matrix(i - 1)(j) + remove,
                            ds(j - 1) + insert,
                            matrix(i - 1)(j - 1) + If(equals(d, a), 0, substitute(d, a))
                        }.Min

                        If i > 1 AndAlso j > 1 AndAlso equals(down(i - 1), a) AndAlso equals(d, across(j - 1)) Then
                            ds(j) = System.Math.Min(ds(j), matrix(i - 1)(j - 2) + If(equals(d, a), 0, transpose(d, down(i - 1))))
                        End If
                    End If
                Next
            Next

            Dim distance = matrix(down.Length - 1)(across.Length - 1)
            Return distance
        End Function
    End Module
End Namespace
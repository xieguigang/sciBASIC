Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module splitTest
    Sub Main()

        Dim bitmap As New List(Of Integer())
        Dim i As int = 0

        For y As Integer = 0 To 20 - 1
            Dim line As New List(Of Integer)

            For x As Integer = 0 To 20 - 1
                line.Add(++i)
            Next

            bitmap.Add(line)
        Next

        Dim vector = bitmap.IteratesALL.AsVector
        Dim target As Vector = {11, 12, 13, 14, 15}

        'Dim local As New GSW(Of Double)(target, vector, Function(a, b) If(a = b, 1, 0), AddressOf asChar)
        'Dim matches = local.GetMatches(local.MaxScore * 0.95).Select(Function(m) m - 1).ToArray



        Pause()
    End Sub

    Private Function asChar(d As Double) As Char
        If d = 0R OrElse d = 1.0R Then
            Return d.ToString.First
        ElseIf d = -1.0R Then
            Return "*"c
        Else
            Return "7"c
        End If
    End Function
End Module

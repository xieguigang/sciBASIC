Imports Microsoft.VisualBasic.Serialization

Public Structure PathMatch

    Dim Pair1 As String
    Dim Pair2 As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Shared Iterator Function Pairs(paths1 As IEnumerable(Of String), path2 As IEnumerable(Of String)) As IEnumerable(Of PathMatch)
        Dim pas1 As String() = paths1.ToArray
        Dim pas2 As String() = path2.ToArray

        If pas1.Length >= pas2.Length Then
            For Each x As PathMatch In __pairs(pas1, pas2)
                Yield x
            Next
        Else
            For Each x As PathMatch In __pairs(pas2, pas1)
                Yield x
            Next
        End If
    End Function

    ''' <summary>
    ''' <paramref name="paths1"/>的元素要比<paramref name="path2"/>多
    ''' </summary>
    ''' <param name="paths1"></param>
    ''' <param name="path2"></param>
    ''' <returns></returns>
    Private Shared Iterator Function __pairs(paths1 As String(), path2 As String()) As IEnumerable(Of PathMatch)
        Dim pls = (From p As String In path2 Select name = p.BaseName, p).ToArray

        For Each path As String In paths1
            Dim q As String = path.BaseName

            For Each s In pls
                If InStr(q, s.name, CompareMethod.Text) = 1 OrElse
                    InStr(s.name, q, CompareMethod.Text) = 1 Then
                    Yield New PathMatch With {
                        .Pair1 = path,
                        .Pair2 = s.p
                    }
                    Exit For
                End If
            Next
        Next
    End Function
End Structure

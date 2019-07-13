#Region "Microsoft.VisualBasic::61ebdc47db0f1d4716597be0889e90ec, Microsoft.VisualBasic.Core\Extensions\IO\Path\PathMatch.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Structure PathMatch
    ' 
    '         Function: __pairs, Pairs, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FileIO.Path

    Public Structure PathMatch

        Dim Pair1 As String
        Dim Pair2 As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Iterator Function Pairs(paths1 As IEnumerable(Of String), path2 As IEnumerable(Of String), Optional process As Func(Of String, String) = Nothing) As IEnumerable(Of PathMatch)
            Dim pas1 As String() = paths1.ToArray
            Dim pas2 As String() = path2.ToArray

            If process Is Nothing Then
                process = Function(s) s
            End If

            If pas1.Length >= pas2.Length Then
                For Each x As PathMatch In __pairs(pas1, pas2, process)
                    Yield x
                Next
            Else
                For Each x As PathMatch In __pairs(pas2, pas1, process)
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
        Private Shared Iterator Function __pairs(paths1 As String(), path2 As String(), process As Func(Of String, String)) As IEnumerable(Of PathMatch)
            Dim pls = (From p As String In path2 Select name = process(p.BaseName), p).ToArray

            For Each path As String In paths1
                Dim q As String = process(path.BaseName)

                For Each S In pls
                    If InStr(q, S.name, CompareMethod.Text) = 1 OrElse
                    InStr(S.name, q, CompareMethod.Text) = 1 Then
                        Yield New PathMatch With {
                        .Pair1 = path,
                        .Pair2 = S.p
                    }
                        Exit For
                    End If
                Next
            Next
        End Function
    End Structure
End Namespace

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports IOModule = System.IO.Path

Namespace FileIO.Path

    Public Module Extensions

        Public Iterator Function Pairs(ParamArray paths As NamedValue(Of String())()) As IEnumerable(Of Dictionary(Of String, String))
            Dim primary As NamedValue(Of String()) = paths(Scan0)
            Dim others = (From path As NamedValue(Of String())
                      In paths.Skip(1)
                          Select path.Name,
                          pls = (From p As String
                                 In path.Value
                                 Select pName = p.BaseName,
                                     p).ToArray).ToArray

            For Each path As String In primary.Value
                Dim q As String = path.BaseName
                Dim result As New Dictionary(Of String, String) From {{primary.Name, path}}

                For Each otherpath In others
                    For Each S In otherpath.pls
                        If InStr(q, S.pName, CompareMethod.Text) = 1 OrElse
                        InStr(S.pName, q, CompareMethod.Text) = 1 Then

                            result.Add(otherpath.Name, S.p)
                            Exit For
                        End If
                    Next
                Next

                If result.Count = paths.Length Then
                    Yield result
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFullPath(relativePath As String) As String
            Return IOModule.GetFullPath(relativePath)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Combine(destination$, name$) As String
            Return IOModule.Combine(destination, name)
        End Function
    End Module
End Namespace
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Text

Namespace CommandLine.Parsers

    Public NotInheritable Class DictionaryParser

        ''' <summary>
        ''' 键值对之间使用分号分隔
        ''' </summary>
        ''' <param name="str$"></param>
        ''' <returns></returns>
        Public Shared Function TryParse(str$) As Dictionary(Of String, String)
            Dim chars As New Pointer(Of Char)(str$)
            Dim tmp As New List(Of Char)
            Dim out As New Dictionary(Of String, String)
            Dim t As New List(Of String)
            Dim markOpen As Boolean = False
            Dim left As Char

            Do While Not chars.EndRead
                Dim c As Char = +chars

                tmp += c

                If c = ASCII.Mark Then
                    If Not markOpen Then
                        If left = "="c Then
                            markOpen = True
                        End If
                    Else
                        If chars.Current = ";"c Then
                            markOpen = False
                            t += New String(tmp)
                            tmp.Clear()
                            chars += 1
                        End If
                    End If
                ElseIf c = ";"c Then
                    If Not markOpen Then
                        tmp.RemoveLast
                        t += New String(tmp)
                        tmp.Clear()
                    End If
                End If

                left = c
            Loop

            If tmp.Count > 0 Then
                t += New String(tmp)
            End If

            For Each var$ In t
                Dim value = var.GetTagValue("="c)
                out(value.Name) =
                    value.x.GetString("'")
            Next

            Return out
        End Function
    End Class
End Namespace
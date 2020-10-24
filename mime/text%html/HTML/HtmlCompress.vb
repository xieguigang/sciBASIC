Namespace HTML

    ''' <summary>
    ''' html compress
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/lizzy0118/HtmlCompress
    ''' </remarks>
    <HideModuleName>
    Public Module HtmlCompress

        Private Function removes(html As String) As String
            Dim strs = html.Split(ChrW(10))

            If strs.Length = 0 Then
                Return html
            End If

            Dim i As Integer = 0
            Dim len = strs.Length

            While i < len
                If strs(CInt(i)).Contains(vbTab & "//") Then
                    Dim str1 = strs(CInt(i)).Replace(vbTab & "//", "§")
                    html = html.Replace(vbTab & "//" & str1.Substring(str1.IndexOf("§"c) + 1), "")
                    Continue While
                End If

                If strs(CInt(i)).Contains("//" & vbTab) Then
                    Dim str1 = strs(CInt(i)).Replace("//" & vbTab, "§")
                    html = html.Replace("//" & vbTab & str1.Substring(str1.IndexOf("§"c) + 1), "")
                    Continue While
                End If

                If strs(i).Contains("///") Then
                    Dim str1 = strs(i).Replace("///", "§")
                    html = html.Replace("///" & str1.Substring(str1.IndexOf("§"c) + 1), "")
                    Continue While
                End If

                If strs(i).Contains(" //") Then
                    Dim str1 = strs(i).Replace(" //", "§")
                    html = html.Replace("//" & str1.Substring(str1.IndexOf("§"c) + 1), "")
                    Continue While
                End If

                If strs(i).Contains("// ") Then
                    Dim str1 = strs(i).Replace("// ", "§ ")
                    html = html.Replace("//" & str1.Substring(str1.IndexOf("§"c) + 1), "")
                    Continue While
                End If

                If strs(i).Contains("{//") Then
                    Dim str1 = strs(i).Replace("{//", "{§")
                    html = html.Replace("//" & str1.Substring(str1.IndexOf("§"c) + 1), "")
                    Continue While
                End If

                If strs(i).Contains("}//") Then
                    Dim str1 = strs(i).Replace("}//", "}§")
                    html = html.Replace("//" & str1.Substring(str1.IndexOf("§"c) + 1), "")
                    Continue While
                End If

                i += 1
            End While

            Return html
        End Function

        Public Function Minify(html As String) As String
            ' removes html comments
            html = removes(html)

            If html.Contains("// ") Then
                html = html.Replace("//  ", "")
            End If

            html = html.Replace("/*", "ˇ")
            html = html.Replace("*/", "§")
            html = html.Replace("@*", "ˇ")
            html = html.Replace("*@", "§")

            Call update(html)

            If Not String.IsNullOrEmpty(html) Then
                html = html.Replace(vbCrLf, " ")
                html = html.Replace(vbLf, " ")
                html = html.Replace(vbTab, "")
                html = html.Replace("                               ", " ")
                html = html.Replace("                     ", " ")
                html = html.Replace("            ", " ")
                html = html.Replace("        ", " ")
                html = html.Replace("   ", "")
                html = html.Replace("  ", " ")
                html = html.Replace("　　　", "")
                html = html.Replace("　　", " ")

                If html.Contains("@Html") Then
                    html = html.Replace("@Html", " @Html")
                    html = html.Replace("' @Html", "'@Html")
                    html = html.Replace(""" @Html", """@Html")
                End If

                If html.Contains("@{") Then
                    html = html.Insert(html.IndexOf("@{"), vbCrLf)
                End If
            End If

            html = html.Replace("function ", ";function ")
            html = html.Replace("(;function ", "(function ")
            html = html.Replace(";;function ", ";function ")
            html = html.Replace("; ;function ", ";function ")
            html = html.Replace(": ;function ", ": function ")

            Return html
        End Function

        Private Sub update(ByRef str As String)
            If str.Contains("ˇ") AndAlso str.Contains("§") Then
                Dim a = str.IndexOf("ˇ"c)
                Dim b = str.IndexOf("§"c)

                If a <= b Then
                    str = str.Replace(str.Substring(a, b - a + 1), "")
                Else
                    str = str.Replace(str.Substring(b, a - b + 1), "")
                End If

                update(str)
            End If
        End Sub
    End Module
End Namespace

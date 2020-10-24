Imports System.Linq
Imports System.Text
Imports System.IO

Namespace HTML

    ''' <summary>
    ''' compress
    ''' </summary>
    Public Module HtmlCompress



        Public Function Minify(html As String) As String
            ' removes html comments
            Dim strs = html.Split(ChrW(10))

            If strs IsNot Nothing AndAlso strs.Length > 0 Then
                Dim i = 0, len = strs.Length

                While i < len

                    If strs(CInt(i)).Contains(Microsoft.VisualBasic.Constants.vbTab & "//") Then
                        Dim str1 = strs(CInt(i)).Replace(Microsoft.VisualBasic.Constants.vbTab & "//", "§")
                        html = html.Replace(Microsoft.VisualBasic.Constants.vbTab & "//" & str1.Substring(str1.IndexOf("§"c) + 1), "")
                        Continue While
                    End If

                    If strs(CInt(i)).Contains("//" & Microsoft.VisualBasic.Constants.vbTab) Then
                        Dim str1 = strs(CInt(i)).Replace("//" & Microsoft.VisualBasic.Constants.vbTab, "§")
                        html = html.Replace("//" & Microsoft.VisualBasic.Constants.vbTab & str1.Substring(str1.IndexOf("§"c) + 1), "")
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
            End If

            If html.Contains("// ") Then
                html = html.Replace("//  ", "")
            End If

            html = html.Replace("/*", "ˇ")
            html = html.Replace("*/", "§")
            html = html.Replace("@*", "ˇ")
            html = html.Replace("*@", "§")
            UpdateA(html)

            If Not String.IsNullOrEmpty(html) Then
                html = html.Replace(Microsoft.VisualBasic.Constants.vbCrLf, " ")
                html = html.Replace(Microsoft.VisualBasic.Constants.vbLf, " ")
                html = html.Replace(Microsoft.VisualBasic.Constants.vbTab, "")
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
                    html = html.Insert(html.IndexOf("@{"), Microsoft.VisualBasic.Constants.vbCrLf)
                End If
            End If

            html = html.Replace("function ", ";function ")
            html = html.Replace("(;function ", "(function ")
            html = html.Replace(";;function ", ";function ")
            html = html.Replace("; ;function ", ";function ")
            html = html.Replace(": ;function ", ": function ")

            Return html
        End Function

        Private Sub UpdateA(ByRef str As String)
            If str.Contains("ˇ") AndAlso str.Contains("§") Then
                Dim a = str.IndexOf("ˇ"c)
                Dim b = str.IndexOf("§"c)

                If a <= b Then
                    str = str.Replace(str.Substring(a, b - a + 1), "")
                Else
                    str = str.Replace(str.Substring(b, a - b + 1), "")
                End If

                UpdateA(str)
            End If
        End Sub
    End Module
End Namespace

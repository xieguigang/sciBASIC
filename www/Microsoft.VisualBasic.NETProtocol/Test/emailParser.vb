Imports System.IO
Imports Microsoft.VisualBasic.Net.Mailto
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Public Module emailParser

    Sub Main()
        Dim raw = "C:\Users\Administrator\Downloads\aaaa.eml"
        Dim email = EmlReader.ParseEMail(raw)
        Dim tables = email.BodyContent.GetTablesHTML
        Dim test = getInnerTable(tables(59)).GetColumnsHTML.Select(Function(c) c.StripHTMLTags).ToArray
        Dim data = tables.Select(Function(t)
                                     Return getInnerTable(t).GetColumnsHTML.Select(Function(r) r.StripHTMLTags()).ToArray
                                 End Function).Where(Function(r) r.Length > 3) _
            .ToArray

        Using book As New StreamWriter(raw.ChangeSuffix("txt").Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))
            For Each line In data.Where(Function(n) n.Length = 8)
                Call book.WriteLine(line.JoinBy(vbTab))
            Next
        End Using

        Pause()
    End Sub

    Function getInnerTable(text As String) As String
        Do While True
            Dim newText = text.GetTablesHTML

            If newText.IsNullOrEmpty Then
                Return text
            Else
                text = newText(0).Substring(1)
            End If
        Loop

        Return Nothing
    End Function
End Module

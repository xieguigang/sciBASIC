Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Net.Http

Namespace Text.Levenshtein

    Public Module ResultVisualize

        ''' <summary>
        ''' Output HTML result for visualization
        ''' </summary>
        ''' <returns></returns>
        <Extension>
        Public Function HTMLVisualize(result As DistResult) As String
            Try
                Return result.__visualizeHTML()
            Catch ex As Exception
                Call App.LogException(ex)
                Return <html>
                           <head>
                               <title><%= NameOf(HTTP_RFC.RFC_INTERNAL_SERVER_ERROR) %></title>
                           </head>
                           <body>
                               <pre><%= ex.ToString %></pre>
                           </body>
                       </html>
            End Try
        End Function

        <Extension>
        Private Function __visualizeHTML(dist As DistResult) As String
            Dim html As StringBuilder = New StringBuilder(1024)
            Dim distEdits$ = dist.DistEdits

            Call html.AppendLine("<!DOCTYPE HTML>
<html>
<head>
<title>{dist.ToString()}</title>
</head>
")
            Call html.AppendLine("<body style=""font-family:Ubuntu;"">")
            Call html.AppendLine("<h3>Summary</h3>")
            Call html.AppendLine($"<table>
<tr>
<td>Reference: </td><td> ({dist.__getReference.Length}) <strong> {dist.Reference}</strong></td>
</tr>
<tr>
<td>Hypotheses: </td><td> ({dist.__getSubject.Length}) <strong> {dist.Hypotheses}</strong></td>
</tr>
<tr>
<td>Levenshtein Edit: </td><td> ({Len(distEdits) - distEdits.Count("m"c)}/{Len(distEdits)}) <strong> {distEdits}</strong></td>
</tr>
<tr>
<td>Matches: </td><td> ({distEdits.Count("m"c)}/{Len(distEdits)}) <strong> {dist.Matches}</strong></td>
</tr>
{dist.__innerInsert()}
<tr>
<td>Distance: </td><td> <strong> {dist.Distance}</strong></td>
<tr>
<tr>
<td>Score: </td><td> <strong> {dist.Score}</strong></td>
</tr>
</table>")
            Call html.AppendLine("<p>
<table>
<tr><td>d</td><td> -> Delete</td></tr>
<tr><td>i</td><td> -> Insert</td></tr>
<tr><td>m</td><td> -> Match</td></tr>
<tr><td>s</td><td> -> Substitute</td></tr>
</table>
</p>")
            Call html.AppendLine("<h3>Levenshtein Edit Distance Table</h3>")
            Call html.AppendLine(dist.__innerMatrix)
            Call html.AppendLine("</body></html>")

            Return html.ToString
        End Function

        <Extension> Private Function __innerMatrix(matrix As DistResult) As String
            Dim MAT As StringBuilder = New StringBuilder("<table>")
            Dim dict As Dictionary(Of Integer, Integer())

            If matrix.DistTable Is Nothing Then
                Return ""
            End If

            If matrix.CSS.IsNullOrEmpty Then
                dict = New Dictionary(Of Integer, Integer())
            Else
                dict = (From cell As Point
                    In matrix.CSS
                        Select cell
                        Group cell By cell.X Into Group) _
                        .ToDictionary(Function(row) row.X,
                                      Function(row) row.Group.ToArray.ToArray(Function(cell) cell.Y))
            End If

            Dim Reference As String = matrix.__getReference()
            Dim Hypotheses As String = matrix.__getSubject()

            Call MAT.AppendLine($"<tr><td><tr><td></td>
{Hypotheses.ToArray(Function(ch) $"<td><strong>{ch.ToString}</strong></td>").JoinBy("")}</tr>")

            For i As Integer = 0 To Len(Reference) - 1
                Dim row As New StringBuilder()

                For j As Integer = 0 To Len(Hypotheses) - 1
                    If dict.ContainsKey(i) AndAlso Array.IndexOf(dict(i), j) > -1 Then
                        Call row.Append($"<td style=""background-color:green;color:white""><strong>{matrix.DistTable(i)(j)}</strong></td>")
                    Else
                        Call row.Append($"<td>{ matrix.DistTable(i)(j)}</td>")
                    End If
                Next
                Call MAT.AppendLine($"<tr><td><strong>{Reference(i).ToString}</strong></td>
{row.ToString}
<td style=""background-color:blue;color:white""><strong>{matrix.DistEdits(i).ToString}</strong></td></tr>")
            Next

            Call MAT.AppendLine("</table>")

            Return MAT.ToString
        End Function
    End Module
End Namespace
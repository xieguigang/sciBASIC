Public Class log

    Public Property commit As String
    Public Property author As String
    Public Property [date] As Date
    Public Property message As String

    ''' <summary>
    ''' parse git log text
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Shared Iterator Function ParseLogText(text As String) As IEnumerable(Of log)
        For Each block As String() In text.LineIterators.Split(Function(line) line.StartsWith("commit "), DelimiterLocation.NextFirst)
            Yield New log With {
                .commit = block(Scan0).Trim.Split.Last,
                .author = block(1).GetTagValue(":", trim:=True).Value,
                .[date] = Date.Parse(block(2).GetTagValue(":", trim:=True).Value),
                .message = block _
                    .Skip(3) _
                    .Select(AddressOf Strings.Trim) _
                    .Where(Function(s) Not s.StringEmpty) _
                    .JoinBy("; ")
            }
        Next
    End Function

End Class

Imports System.Text.RegularExpressions

Namespace Language.Python

    Public Module Regexp

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="pattern">这个会首先被分行然后去除掉python注释</param>
        ''' <param name="input"></param>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Public Function FindAll(pattern As String, input As String, Optional options As RegexOptions = RegexOptions.None) As String()
            Dim tokens As String() = pattern.Trim.lTokens _
                .Select(AddressOf __trimComment) _
                .Where(Function(s) Not String.IsNullOrEmpty(s)) _
                .ToArray
            pattern = String.Join("", tokens)
            Return Regex.Matches(input, pattern, options).ToArray
        End Function

        ''' <summary>
        ''' 假设所有的注释都是由#和一个空格开始起始的 ``# ``
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Private Function __trimComment(s As String) As String
            s = s.Trim

            If s.StartsWith("# ") Then Return "" ' 整行都是注释

            Dim i As Integer = s.IndexOf("# ")

            If i > -1 Then
                s = s.Substring(0, i).Trim
            End If

            Return s
        End Function
    End Module
End Namespace
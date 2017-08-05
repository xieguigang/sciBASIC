Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' Url处理辅助类
''' </summary>
Public Class UrlUtility

    Const Find$ = "(?is)(href|src)=(""|')([^(""|')]+)(""|')"

    ''' <summary>
    ''' 基于baseUrl，补全html代码中的链接
    ''' </summary>
    ''' <param name="baseUrl"></param>
    ''' <param name="html"></param>
    Public Shared Function FixUrl(baseUrl As String, html As String) As String
        html = Regex.Replace(
            html, Find,
            Function(match)
                Dim org As String = match.Value
                Dim link As String = match.Groups(3).Value
                If link.StartsWith("http") Then
                    Return org
                End If

                Try
                    Dim uri As New Uri(baseUrl)
                    Dim thisUri As New Uri(uri, link)
                    Dim fullUrl As String = String.Format("{0}=""{1}""", match.Groups(1).Value, thisUri.AbsoluteUri)
                    Return fullUrl
                Catch generatedExceptionName As Exception
                    Return org
                End Try

            End Function)
        Return html
    End Function
End Class

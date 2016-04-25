Imports System.Text.RegularExpressions

Public Class WebResult

    ''' <summary>
    ''' Specifies the Title element of the result string. 
    ''' </summary>
    ''' <returns></returns>
    Public Property Title As String
    ''' <summary>
    ''' In short description of the link produced. 
    ''' </summary>
    ''' <returns></returns>
    Public Property BriefText As String
    ''' <summary>
    ''' Url that points to the Current result.
    ''' </summary>
    ''' <returns></returns>
    Public Property URL As String

    Public ReadOnly Property Site As String
        Get
            Return Regex.Match(URL, "https?://(www.)?[^/]+").Value
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return String.Format("{0}  [{1}]", Title, URL)
    End Function
End Class

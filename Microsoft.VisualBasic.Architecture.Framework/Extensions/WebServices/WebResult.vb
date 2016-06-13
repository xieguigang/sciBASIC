Imports System.Text.RegularExpressions

Namespace Net.Http

    ''' <summary>
    ''' The data structure for represents the search result of the Web search egine.
    ''' </summary>
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
        ''' <summary>
        ''' Update time.
        ''' </summary>
        ''' <returns></returns>
        Public Property Update As String

        Public ReadOnly Property Site As String
            Get
                Return Regex.Match(URL, "https?://(www.)?[^/]+").Value
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("{0}  [{1}]", Title, URL)
        End Function
    End Class
End Namespace
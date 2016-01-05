
''' <summary>
''' Descripts the control appearances using html document and trigger event handler entry point.
''' </summary>
Public Class Designer : Implements System.Collections.Generic.IEnumerable(Of Func(Of String, String))

    ''' <summary>
    ''' 使用html文档为控件的外观来提供描述
    ''' </summary>
    ''' <returns></returns>
    Public Property HTML As String
    ''' <summary>
    ''' Get/Post请求所触发的事件，已经被转换为小写形式的了
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Events As SortedDictionary(Of String, Func(Of String, String)) =
        New SortedDictionary(Of String, Func(Of String, String))

    Protected Friend __refreshHandle As Action

    Public Sub [AddHandler](uri As String, [EventHandler] As Func(Of String, String))
        Call Events.Add(uri.ToLower, EventHandler)
    End Sub

    Public Event EventTrigger(uri As String, ByRef resultHtml As String)

    Protected Overridable Function __buildHTML() As String
        Return HTML
    End Function

    ''' <summary>
    ''' 返回执行之后的得到的html页面
    ''' </summary>
    ''' <param name="url"></param>
    ''' <returns></returns>
    Public Function HandleInvoke(url As String) As String
        Dim luri As String = url.ToLower
        Dim result As String = ""

        If Events.ContainsKey(luri) Then
            Dim handler = Events(luri)

            Try
                result = handler(luri)
            Catch ex As Exception
                Call App.LogException(New Exception(url, ex))
                Return HTML
            End Try
        End If

        Try
            RaiseEvent EventTrigger(url, result)
        Catch ex As Exception
            Call App.LogException(New Exception(url, ex))
            result = HTML
        End Try

        Return result
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator(Of Func(Of String, String)) Implements IEnumerable(Of Func(Of String, String)).GetEnumerator
        For Each [Handle] As KeyValuePair(Of String, Func(Of String, String)) In Events
            Yield Handle.Value
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class
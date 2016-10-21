Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.DocumentStream

Public Module DataEnumerator

    <Extension>
    Public Iterator Function [Where](df As DataFrame, query$) As IEnumerable(Of Dictionary(Of String, String))
        Dim exp As Expression = query.Build
        Dim def As New IObject(df.SchemaOridinal.Keys)

        For Each row In df.EnumerateData
            If exp.Evaluate(def, row) Then
                Yield row
            End If
        Next
    End Function

    ''' <summary>
    ''' 直接取出前n个
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="type"></param>
    ''' <param name="query$"></param>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function Limit(Of T)(source As IEnumerable(Of T), type As Type, query$, n%) As IEnumerable(Of T)
        Dim exp As Expression = query.Build
        Dim def As New IObject(type)

        For Each x As T In source
            If exp.Evaluate(def, x) Then
                Yield x

                If n > 0 Then
                    n -= 1
                Else
                    Exit For
                End If
            End If
        Next
    End Function

    ''' <summary>
    ''' 排序之后取得分最高的前n个
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="type"></param>
    ''' <param name="query$"></param>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Top(Of T)(source As IEnumerable(Of T), type As Type, query$, n%) As IEnumerable(Of T)

    End Function
End Module

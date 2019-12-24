Imports System.Runtime.CompilerServices

Namespace Linq

    <HideModuleName>
    Public Module JoinExtensions

        ''' <summary>
        ''' Iterates all of the elements in a two dimension collection as the data source 
        ''' for the linq expression or ForEach statement.
        ''' (适用于二维的集合做为linq的数据源，不像<see cref="Unlist"/>是进行转换，
        ''' 这个是返回迭代器的，推荐使用这个函数)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function IteratesALL(Of T)(source As IEnumerable(Of IEnumerable(Of T))) As IEnumerable(Of T)
            For Each line As IEnumerable(Of T) In source
                If Not line Is Nothing Then
                    Using iterator = line.GetEnumerator
                        Do While iterator.MoveNext
                            Yield iterator.Current
                        Loop
                    End Using
                End If
            Next
        End Function

        ''' <summary>
        ''' First, iterate populates the elements in collection <paramref name="a"/>, 
        ''' and then populate out all of the elements on collection <paramref name="b"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="a">Object collection</param>
        ''' <param name="b">Another object collection.</param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function JoinIterates(Of T)(a As IEnumerable(Of T), b As IEnumerable(Of T)) As IEnumerable(Of T)
            If Not a Is Nothing Then
                For Each x As T In a
                    Yield x
                Next
            End If

            If Not b Is Nothing Then
                For Each x As T In b
                    Yield x
                Next
            End If
        End Function

        <Extension>
        Public Iterator Function JoinIterates(Of T)(a As IEnumerable(Of T), b As T) As IEnumerable(Of T)
            If Not a Is Nothing Then
                For Each x As T In a
                    Yield x
                Next
            End If

            If Not b Is Nothing Then
                Yield b
            End If
        End Function
    End Module
End Namespace
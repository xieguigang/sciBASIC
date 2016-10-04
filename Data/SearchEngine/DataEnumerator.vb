Public Module DataEnumerator

    Public Iterator Function ForEach(Of T)(source As IEnumerable(Of T)) As IEnumerable(Of IObject)
        Dim o As New IObject(GetType(T))

        For Each x As T In source
            o.x = x
            Yield o
        Next
    End Function

    Public Iterator Function ForEach(source As IEnumerable(Of String)) As IEnumerable(Of IObject)
        Dim o As New IObject(GetType(Text))

        For Each s$ In source
            o.x = New Text With {
                .Text = s$
            }
            Yield o
        Next
    End Function
End Module

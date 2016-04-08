Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel


    Public Class DefaultHashHandle(Of T As sIdEnumerable) : Inherits HashHandle(Of IHashValue(Of T))

        Sub New(Optional capacity As Integer = 2048)
            Call MyBase.New(capacity)
        End Sub

        Sub New(source As IEnumerable(Of T), Optional capacity As Integer = 2048)
            Call Me.New(capacity)
            Call Me.Add(source)
        End Sub

        Public Overloads Sub Add(x As T)
            Call MyBase.Add(New IHashValue(Of T) With {.obj = x})
        End Sub

        Public Overloads Sub Add(source As IEnumerable(Of T))
            For Each x In source
                Call Add(x)
            Next
        End Sub

        Public Shared Operator +(list As DefaultHashHandle(Of T), x As T) As DefaultHashHandle(Of T)
            Call list.Add(x)
            Return list
        End Operator

        Public Shared Operator +(list As DefaultHashHandle(Of T), x As IEnumerable(Of T)) As DefaultHashHandle(Of T)
            Call list.Add(x)
            Return list
        End Operator
    End Class

    Public Class HashHandle(Of T As IHashHandle) : Implements IEnumerable(Of T)

        Protected __innerHash As New Dictionary(Of T)
        Protected __innerList As List(Of T)
        Dim __emptys As Queue(Of Integer)
        Dim delta As Integer

        Sub New(Optional capacity As Integer = 2048)
            __innerList = New List(Of T)(capacity)
            __emptys = New Queue(Of Integer)(capacity)
            delta = capacity

            Call Me.__allocate()
        End Sub

        Sub New(source As IEnumerable(Of T), Optional capacity As Integer = 2048)
            Call Me.New(capacity)
            Call Me.Add(source)
        End Sub

        Public Function HasElement(x As String) As Boolean
            Return __innerHash.ContainsKey(x)
        End Function

        Public Function IsNull(x As Integer) As Boolean
            Return __emptys.Contains(x)
        End Function

        Public Function [Next](x As T) As T
            Return [Next](x.Identifier)
        End Function

        Public Function [Next](x As String) As T
            Dim pos As Integer = __innerHash(x).AddrHwnd
            Dim n As T = __innerList(pos + 1)
            Return n
        End Function

        Public Function [Next](i As Integer) As T
            Return __innerList(i + 1)
        End Function

        Public Function Previous(x As T) As T
            Dim pos As Integer = __innerHash(x.Identifier).AddrHwnd
            Return __innerList(pos - 1)
        End Function

        Public Function Previous(x As String) As T
            Dim pos As Integer = __innerHash(x).AddrHwnd
            Return __innerList(pos - 1)
        End Function

        Public Function Previous(x As Integer) As T
            Return __innerList(x - 1)
        End Function

        Public Function Current(x As String) As T
            Return __innerHash(x)
        End Function

        Public Function Current(i As Integer) As T
            Return __innerList(i)
        End Function

        Public Sub Add(x As T)
            If __emptys.Count = 0 Then
                Call __allocate()
            End If

            Dim i As Integer = __emptys.Dequeue
            x.AddrHwnd = i
            __innerList(i) = x
            __innerHash(x.Identifier) = x
        End Sub

        Public Sub Add(source As IEnumerable(Of T))
            For Each x In source
                Call Add(x)
            Next
        End Sub

        Private Sub __allocate()
            Dim top As Integer = __innerList.Count

            For i As Integer = 0 To delta - 1
                Call __emptys.Enqueue(top + i)
            Next

            __innerList += New T(__emptys.Count - 1) {}
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x In __innerList
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
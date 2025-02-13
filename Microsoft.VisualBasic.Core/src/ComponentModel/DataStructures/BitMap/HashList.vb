Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel

    Public Class HashList(Of T As IAddressOf) : Implements Enumeration(Of T)

        Dim list As New Dictionary(Of Integer, T)

        Default Public Property Item(i As Integer) As T
            Get
                If list.ContainsKey(i) Then
                    Return list(key:=i)
                Else
                    Return Nothing
                End If
            End Get
            Set(value As T)
                If value Is Nothing Then
                    Call list.Remove(i)
                Else
                    value.Assign(i)
                    list(key:=i) = value
                End If
            End Set
        End Property

        Public ReadOnly Property Count As Integer
            Get
                Return list.Count
            End Get
        End Property

        Public ReadOnly Property MaxMap As Integer
            Get
                If list.Count = 0 Then
                    Return 0
                End If
                Return list.Keys.Max
            End Get
        End Property

        Public ReadOnly Property MinMap As Integer
            Get
                If list.Count = 0 Then
                    Return 0
                End If
                Return list.Keys.Min
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(capacity As Integer)
            list = New Dictionary(Of Integer, T)(capacity)
        End Sub

        Sub New(items As IEnumerable(Of T))
            Call ReplaceRange(items)
        End Sub

        Public Sub Clear()
            Call list.Clear()
        End Sub

        ''' <summary>
        ''' overrides the item value at specific address
        ''' </summary>
        ''' <param name="item"></param>
        Public Sub Replace(item As T)
            list(key:=item.Address) = item
        End Sub

        ''' <summary>
        ''' overrides the item value at specific index
        ''' </summary>
        ''' <param name="i"></param>
        ''' <param name="item"></param>
        Public Sub ReplaceAt(i As Integer, item As T)
            item.Assign(i)
            list(key:=i) = item
        End Sub

        Public Sub ReplaceRange(items As IEnumerable(Of T))
            If Not items Is Nothing Then
                For Each item As T In items
                    Call Replace(item)
                Next
            End If
        End Sub

        Public Sub Append(item As T)
            Dim i As Integer = MaxMap

            If i <> 0 Then
                i += 1
            End If

            item.Assign(i)
            list(key:=i) = item
        End Sub

        Public Function IndexOf(item As T) As Integer
            If Not list.ContainsKey(item.Address) Then
                Return -1
            End If
            Return item.Address
        End Function

        Public Function Contains(item As T) As Boolean
            Return list.ContainsKey(item.Address)
        End Function

        Public Sub Remove(item As T)
            Call list.Remove(item.Address)
        End Sub

        Public Sub RemoveAt(i As Integer)
            Call list.Remove(i)
        End Sub

        Public Sub Sort()
            Dim sorted = list.Values.OrderBy(Function(a) a).ToArray

            Call Clear()

            For i As Integer = 0 To sorted.Length - 1
                Dim item As T = sorted(i)

                Call item.Assign(i)
                Call list.Add(i, item)
            Next
        End Sub

        Public Iterator Function GenericEnumerator() As IEnumerator(Of T) Implements Enumeration(Of T).GenericEnumerator
            For Each item As T In list.Values
                Yield item
            Next
        End Function
    End Class
End Namespace
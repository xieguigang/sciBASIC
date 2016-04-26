Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

''' <summary>
''' Represents a collection of keys and values.To browse the .NET Framework source
''' code for this type, see the Reference Source.
''' </summary>
''' <typeparam name="V"></typeparam>
Public Class Dictionary(Of V As sIdEnumerable) : Inherits SortedDictionary(Of String, V)

    Sub New()
        Call MyBase.New
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the System.Collections.Generic.SortedDictionary`2
    ''' class that contains elements copied from the specified System.Collections.Generic.IDictionary`2
    ''' and uses the default System.Collections.Generic.IComparer`1 implementation for
    ''' the key type.
    ''' </summary>
    ''' <param name="source">
    ''' The System.Collections.Generic.IDictionary`2 whose elements are copied to the
    ''' new System.Collections.Generic.SortedDictionary`2.
    ''' </param>
    Sub New(source As Dictionary(Of String, V))
        Call MyBase.New(source)
    End Sub

    Sub New(source As IEnumerable(Of V))
        Call Me.New

        For Each x As V In source
            Call Add(x)
        Next
    End Sub

    Public Function GetValueList() As List(Of V)
        Return Values.ToList
    End Function

    ''' <summary>
    ''' Adds an element with the specified key and value into the System.Collections.Generic.SortedDictionary`2.
    ''' </summary>
    ''' <param name="item"></param>
    Public Overloads Sub Add(item As V)
        Call MyBase.Add(item.Identifier, item)
    End Sub

    Public Sub AddRange(source As IEnumerable(Of V))
        For Each x As V In source
            Call MyBase.Add(x.Identifier, x)
        Next
    End Sub

    Public Sub InsertOrUpdate(x As V)
        If Me.ContainsKey(x.Identifier) Then
            Me(x.Identifier) = x
        Else
            Call MyBase.Add(x.Identifier, x)
        End If
    End Sub

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="name">不区分大小写的</param>
    ''' <returns></returns>
    Public Function Find(name As String) As V
        If MyBase.ContainsKey(name) Then
            Return Me(name)
        Else
            If Me.ContainsKey(name.ToLower.ShadowCopy(name)) Then
                Return Me(name)
            ElseIf Me.ContainsKey(name.ToUpper.ShadowCopy(name)) Then
                Return Me(name)
            Else
                Return Nothing
            End If
        End If
    End Function

    Public Function SafeGetValue(name As String, Optional ByRef [default] As V = Nothing) As V
        Dim x As V = Nothing
        If TryGetValue(name, x) Then
            Return x
        Else
            Return [default]
        End If
    End Function

    ''' <summary>
    ''' 假若目标元素不存在于本字典之中，则会返回False
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Overloads Function Remove(x As V) As Boolean
        If Me.ContainsKey(x.Identifier) Then
            Return Me.Remove(x.Identifier)
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Adds an element with the specified key and value into the System.Collections.Generic.SortedDictionary`2.
    ''' </summary>
    ''' <param name="hash"></param>
    ''' <param name="item"></param>
    ''' <returns></returns>
    Public Shared Operator +(hash As Dictionary(Of V), item As V) As Dictionary(Of V)
        Call hash.Add(item)
        Return hash
    End Operator

    ''' <summary>
    ''' Find a variable in the hash table
    ''' </summary>
    ''' <param name="hash"></param>
    ''' <param name="uid"></param>
    ''' <returns></returns>
    Public Shared Operator ^(hash As Dictionary(Of V), uid As String) As V
        If hash.ContainsKey(uid) Then
            Return hash(uid)
        Else
            Return Nothing
        End If
    End Operator

    Public Shared Operator -(hash As Dictionary(Of V), id As String) As Dictionary(Of V)
        Call hash.Remove(id)
        Return hash
    End Operator
End Class

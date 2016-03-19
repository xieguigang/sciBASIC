Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

''' <summary>
''' Represents a collection of keys and values.To browse the .NET Framework source
''' code for this type, see the Reference Source.
''' </summary>
''' <typeparam name="V"></typeparam>
Public Class Dictionary(Of V As sIdEnumerable) : Inherits SortedDictionary(Of String, V)

    Sub New()
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

    ''' <summary>
    ''' Adds an element with the specified key and value into the System.Collections.Generic.SortedDictionary`2.
    ''' </summary>
    ''' <param name="item"></param>
    Public Overloads Sub Add(item As V)
        Call MyBase.Add(item.Identifier, item)
    End Sub

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
End Class

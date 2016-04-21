Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports System.Xml.Serialization

Namespace ComponentModel

    ''' <summary>
    ''' The key has 2 string value collection.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TripleKeyValuesPair : Implements sIdEnumerable
        Implements ITripleKeyValuesPair(Of String, String, String)

        <XmlAttribute> Public Property Key As String Implements sIdEnumerable.Identifier, ITripleKeyValuesPair(Of String, String, String).Identifier
        <XmlAttribute> Public Property Value1 As String Implements ITripleKeyValuesPair(Of String, String, String).Value2
        <XmlAttribute> Public Property Value2 As String Implements ITripleKeyValuesPair(Of String, String, String).Address

        Sub New()
        End Sub

        Sub New(Key As String, Value1 As String, Value2 As String)
            Me.Key = Key
            Me.Value1 = Value1
            Me.Value2 = Value2
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0} -> [{1}];  [{2}]", Key, Value1, Value2)
        End Function
    End Class

    Public Class TripleKeyValuesPair(Of T) : Inherits TripleKeyValuesPair(Of T, T, T)
        Implements ITripleKeyValuesPair(Of T, T, T)

        Sub New()
        End Sub

        Sub New(v1 As T, v2 As T, v3 As T)
            Call MyBase.New(v1, v2, v3)
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0} -> [{1}];  [{2}]", Value3, Value1, Value2)
        End Function

        Public Overloads Shared Operator +(list As List(Of TripleKeyValuesPair(Of T)), x As TripleKeyValuesPair(Of T)) As List(Of TripleKeyValuesPair(Of T))
            Call list.Add(x)
            Return list
        End Operator

        Public Overloads Shared Operator -(list As List(Of TripleKeyValuesPair(Of T)), x As TripleKeyValuesPair(Of T)) As List(Of TripleKeyValuesPair(Of T))
            Call list.Remove(x)
            Return list
        End Operator
    End Class

    Public Interface ITripleKeyValuesPair(Of T1, T2, T3)
        Property Address As T3
        Property Identifier As T1
        Property Value2 As T2
    End Interface

    Public Class TripleKeyValuesPair(Of T1, T2, T3)
        Implements ITripleKeyValuesPair(Of T1, T2, T3)

        Public Property Value3 As T3 Implements ITripleKeyValuesPair(Of T1, T2, T3).Address
        Public Property Value1 As T1 Implements ITripleKeyValuesPair(Of T1, T2, T3).Identifier
        Public Property Value2 As T2 Implements ITripleKeyValuesPair(Of T1, T2, T3).Value2

        Sub New()
        End Sub

        Sub New(v1 As T1, v2 As T2, v3 As T3)
            Value1 = v1
            Value2 = v2
            Value3 = v3
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0} -> [{1}];  [{2}]", Value3, Value1, Value2)
        End Function

        Public Shared Operator +(list As List(Of TripleKeyValuesPair(Of T1, T2, T3)), x As TripleKeyValuesPair(Of T1, T2, T3)) As List(Of TripleKeyValuesPair(Of T1, T2, T3))
            Call list.Add(x)
            Return list
        End Operator

        Public Shared Operator -(list As List(Of TripleKeyValuesPair(Of T1, T2, T3)), x As TripleKeyValuesPair(Of T1, T2, T3)) As List(Of TripleKeyValuesPair(Of T1, T2, T3))
            Call list.Remove(x)
            Return list
        End Operator
    End Class
End Namespace
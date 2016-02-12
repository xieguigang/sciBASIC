Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel

    ''' <summary>
    ''' The key has 2 string value collection.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TripleKeyValuesPair : Implements sIdEnumerable
        Implements ITripleKeyValuesPair(Of String, String, String)

        <System.Xml.Serialization.XmlAttribute> Public Property Key As String Implements sIdEnumerable.Identifier, ITripleKeyValuesPair(Of String, String, String).locusId
        <System.Xml.Serialization.XmlAttribute> Public Property Value1 As String Implements ITripleKeyValuesPair(Of String, String, String).Value2
        <System.Xml.Serialization.XmlAttribute> Public Property Value2 As String Implements ITripleKeyValuesPair(Of String, String, String).Value3

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
    End Class

    Public Interface ITripleKeyValuesPair(Of T1, T2, T3)
        Property Value3 As T3
        Property locusId As T1
        Property Value2 As T2
    End Interface

    Public Class TripleKeyValuesPair(Of T1, T2, T3)
        Implements ITripleKeyValuesPair(Of T1, T2, T3)

        Public Property Value3 As T3 Implements ITripleKeyValuesPair(Of T1, T2, T3).Value3
        Public Property Value1 As T1 Implements ITripleKeyValuesPair(Of T1, T2, T3).locusId
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
    End Class
End Namespace
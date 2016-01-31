Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel

    ''' <summary>
    ''' The key has 2 string value collection.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TripleKeyValuesPair : Implements sIdEnumerable
        <Xml.Serialization.XmlAttribute> Public Property Key As String Implements sIdEnumerable.Identifier
        <Xml.Serialization.XmlAttribute> Public Property Value1 As String
        <Xml.Serialization.XmlAttribute> Public Property Value2 As String

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

    Public Class TripleKeyValuesPair(Of T)
        Public Property Value3 As T
        Public Property Value1 As T
        Public Property Value2 As T

        Sub New()
        End Sub

        Sub New(v1 As T, v2 As T, v3 As T)
            Value1 = v1
            Value2 = v2
            Value3 = v3
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0} -> [{1}];  [{2}]", Value3, Value1, Value2)
        End Function
    End Class
End Namespace
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports System.Xml.Serialization

Namespace ComponentModel

    ''' <summary>
    ''' An object for the text file format xml data storage.(用于存储与XML文件之中的字符串键值对对象)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class KeyValuePair : Inherits KeyValuePairObject(Of String, String)
        Implements sIdEnumerable
        Implements IKeyValuePair

        Public Sub New()

        End Sub

        Sub New(item As KeyValuePair(Of String, String))
            Key = item.Key
            Value = item.Value
        End Sub

        Sub New(Key As String, Value As String)
            Me.Key = Key
            Me.Value = Value
        End Sub

        ''' <summary>
        ''' Defines a key/value pair that can be set or retrieved.(特化的<see cref="IKeyValuePairObject(Of String, String)"></see>字符串属性类型)
        ''' </summary>
        ''' <remarks></remarks>
        Public Interface IKeyValuePair : Inherits IKeyValuePairObject(Of String, String)
        End Interface

#Region "ComponentModel.Collection.Generic.KeyValuePairObject(Of String, String) property overrides"

        ''' <summary>
        ''' Gets the key in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>在这里可能用不了<see cref="XmlAttributeAttribute"></see>自定义属性，因为其基本类型之中的Key和Value可以是任意的类型的，Attribute格式无法序列化复杂的数据类型</remarks>
        Public Overrides Property Key As String Implements sIdEnumerable.Identifier
            Get
                Return MyBase.Key
            End Get
            Set(value As String)
                MyBase.Key = value
            End Set
        End Property
#End Region

        Public Overloads Shared Widening Operator CType(obj As KeyValuePair(Of String, String)) As KeyValuePair
            Return New KeyValuePair With {.Key = obj.Key, .Value = obj.Value}
        End Operator

        Public Overloads Shared Widening Operator CType(obj As String()) As KeyValuePair
            Return New KeyValuePair With {.Key = obj.First, .Value = obj.Get(1)}
        End Operator

        Public Overrides Function ToString() As String
            Return String.Format("{0} -> {1}", Key, Value)
        End Function

        Public Overloads Shared Function CreateObject(key As String, value As String) As KeyValuePair
            Return New KeyValuePair With {
                .Key = key,
                .Value = value
            }
        End Function

        Public Shared Function ToDictionary(ListData As IEnumerable(Of KeyValuePair)) As Dictionary(Of String, String)
            Dim Dictionary As Dictionary(Of String, String) =
                ListData.ToDictionary(Function(obj) obj.Key, Function(obj) obj.Value)
            Return Dictionary
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj Is KeyValuePair Then
                Dim KeyValuePair As KeyValuePair = DirectCast(obj, KeyValuePair)

                Return String.Equals(KeyValuePair.Key, Key) AndAlso
                    String.Equals(KeyValuePair.Value, Value)
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="strict">If strict is TRUE then the function of the string compares will case sensitive.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Equals(obj As KeyValuePair, Optional strict As Boolean = True) As Boolean
            If strict Then
                Return String.Equals(obj.Key, Key) AndAlso String.Equals(obj.Value, Value)
            Else
                Return String.Equals(obj.Key, Key, StringComparison.OrdinalIgnoreCase) AndAlso
                       String.Equals(obj.Value, Value, StringComparison.OrdinalIgnoreCase)
            End If
        End Function

        Public Shared Function Distinct(source As KeyValuePair()) As KeyValuePair()
            Dim List = (From obj In source Select obj Order By obj.Key Ascending).ToList
            For i As Integer = 0 To List.Count - 1
                If i >= List.Count Then
                    Exit For
                End If
                Dim item = List(i)

                For j As Integer = i + 1 To List.Count - 1
                    If j >= List.Count Then
                        Exit For
                    End If
                    If item.Equals(List(j)) Then
                        Call List.RemoveAt(j)
                        j -= 1
                    End If
                Next
            Next

            Return List.ToArray
        End Function
    End Class

    ''' <summary>
    ''' {Key, strArray()} The value of this data type object is a string collection.(本类型对象的值属性类型为一个字符串集合)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Key_strArrayValuePair : Inherits KeyValuePairObject(Of String, String())
        Implements sIdEnumerable

        <XmlAttribute> Public Overrides Property Key As String Implements sIdEnumerable.Identifier
            Get
                Return MyBase.Key
            End Get
            Set(value As String)
                MyBase.Key = value
            End Set
        End Property
        <XmlElement> Public Overrides Property Value As String()
            Get
                Return MyBase.Value
            End Get
            Set(value As String())
                MyBase.Value = value
            End Set
        End Property

        Public Overloads Shared Widening Operator CType(obj As KeyValuePair(Of String, String())) As Key_strArrayValuePair
            Return New Key_strArrayValuePair With {
                .Key = obj.Key,
                .Value = obj.Value
            }
        End Operator

        Public Overloads Shared Function CreateObject(key As String, value As String()) As Key_strArrayValuePair
            Return New Key_strArrayValuePair With {
                .Key = key,
                .Value = value
            }
        End Function

        Public Overrides Function ToString() As String
            Return Key
        End Function

        Public Shared Function ToDictionary(ListData As Generic.IEnumerable(Of Key_strArrayValuePair)) As Dictionary(Of String, String())
            Dim Dictionary As Dictionary(Of String, String()) = New Dictionary(Of String, String())
            For Each item In ListData
                Call Dictionary.Add(item.Key, item.Value)
            Next

            Return Dictionary
        End Function
    End Class
End Namespace

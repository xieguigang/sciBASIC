Namespace ComponentModel

    ''' <summary>
    ''' you can applying this data type into a dictionary object to makes the mathematics calculation more easily.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Value(Of T)

        ''' <summary>
        ''' The object value with a specific type define.
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As T

        Sub New(value As T)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return Scripting.InputHandler.ToString(Value)
        End Function
    End Class

    ''' <summary>
    ''' 假若需要通过字典对象实现一些统计操作，则这个对象类型可能十分有用
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class DictHashValue(Of T)
        ''' <summary>
        ''' 由于字典对象的元素为值对象，所以无法进行元素值的修改，所以可以使用这个对象进行修改
        ''' </summary>
        ''' <returns></returns>
        Public Property value As T

        Public Overrides Function ToString() As String
            Return value.ToString
        End Function
    End Class

    ''' <summary>
    ''' The key has 2 string value collection.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TripleKeyValuesPair : Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.IdEnumerable
        <Xml.Serialization.XmlAttribute> Public Property Key As String Implements Collection.Generic.IDEnumerable.Identifier
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

    ''' <summary>
    ''' An object for the text file format xml data storage.(用于存储与XML文件之中的字符串键值对对象)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class KeyValuePair : Inherits ComponentModel.Collection.Generic.KeyValuePairObject(Of String, String)
        Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.IdEnumerable
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
        ''' Defines a key/value pair that can be set or retrieved.(特化的<see cref="ComponentModel.Collection.Generic.IKeyValuePairObject(Of String, String)"></see>字符串属性类型)
        ''' </summary>
        ''' <remarks></remarks>
        Public Interface IKeyValuePair : Inherits ComponentModel.Collection.Generic.IKeyValuePairObject(Of String, String)
        End Interface

#Region "ComponentModel.Collection.Generic.KeyValuePairObject(Of String, String) property overrides"

        ''' <summary>
        ''' Gets the key in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>在这里可能用不了<see cref="Xml.Serialization.XmlAttributeAttribute"></see>自定义属性，因为其基本类型之中的Key和Value可以是任意的类型的，Attribute格式无法序列化复杂的数据类型</remarks>
        Public Overrides Property Key As String Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.IdEnumerable.Identifier
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
            Return New KeyValuePair With {.Key = key, .Value = value}
        End Function

        Public Shared Function ToDictionary(ListData As Generic.IEnumerable(Of KeyValuePair)) As Dictionary(Of String, String)
            Dim Dictionary As Dictionary(Of String, String) = ListData.ToDictionary(Function(obj) obj.Key, elementSelector:=Function(obj) obj.Value)
            Return Dictionary
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj Is KeyValuePair Then
                Dim KeyValuePair As KeyValuePair = DirectCast(obj, KeyValuePair)
                Return String.Equals(KeyValuePair.Key, Key) AndAlso String.Equals(KeyValuePair.Value, Value)
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

        Public Shared Function Distinct(Collection As KeyValuePair()) As KeyValuePair()
            Dim List = (From obj In Collection Select obj Order By obj.Key Ascending).ToList
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
    Public Class Key_strArrayValuePair : Inherits ComponentModel.Collection.Generic.KeyValuePairObject(Of String, String())
        Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.IdEnumerable

        <Xml.Serialization.XmlAttribute> Public Overrides Property Key As String Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.IdEnumerable.Identifier
            Get
                Return MyBase.Key
            End Get
            Set(value As String)
                MyBase.Key = value
            End Set
        End Property
        <Xml.Serialization.XmlElement> Public Overrides Property Value As String()
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

Namespace ComponentModel.Collection.Generic

    ''' <summary>
    ''' Defines a key/value pair that can be set or retrieved.
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <remarks></remarks>
    Public Interface IKeyValuePairObject(Of TKey, TValue)
        ''' <summary>
        ''' Gets the key in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Identifier As TKey
        ''' <summary>
        ''' Gets the value in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Value As TValue
    End Interface

    ''' <summary>
    ''' Defines a key/value pair that only can be retrieved.
    ''' </summary>
    ''' <typeparam name="TValue"></typeparam>
    ''' <remarks></remarks>
    Public Interface IReadOnlyDataSource(Of TValue)
        ReadOnly Property Key As String
        ReadOnly Property Value As TValue
    End Interface

    ''' <summary>
    ''' Defines a key/value pair that can be set or retrieved.
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <remarks></remarks>
    Public Class KeyValuePairObject(Of TKey, TValue) : Implements IKeyValuePairObject(Of TKey, TValue)

        ''' <summary>
        ''' Gets the key in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property Key As TKey Implements IKeyValuePairObject(Of TKey, TValue).Identifier
        ''' <summary>
        ''' Gets the value in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property Value As TValue Implements IKeyValuePairObject(Of TKey, TValue).Value

        Sub New()
        End Sub

        Sub New(KEY As TKey, VALUE As TValue)
            Me.Key = KEY
            Me.Value = VALUE
        End Sub

        Sub New(raw As System.Collections.Generic.KeyValuePair(Of TKey, TValue))
            Key = raw.Key
            Value = raw.Value
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0} -> {1}", Key.ToString, Value.ToString)
        End Function

        Public Shared Function CreateObject(key As TKey, value As TValue) As KeyValuePairObject(Of TKey, TValue)
            Return New KeyValuePairObject(Of TKey, TValue) With {.Key = key, .Value = value}
        End Function

        Public Shared Widening Operator CType(args As Object()) As KeyValuePairObject(Of TKey, TValue)
            If args.IsNullOrEmpty Then
                Return New KeyValuePairObject(Of TKey, TValue)
            End If
            If args.Length = 1 Then
                Return New KeyValuePairObject(Of TKey, TValue) With {
                    .Key = DirectCast(args(Scan0), TKey)
                }
            End If

            Return New KeyValuePairObject(Of TKey, TValue) With {
                .Key = DirectCast(args(Scan0), TKey),
                .Value = DirectCast(args(1), TValue)
            }
        End Operator
    End Class

    Public Class KeyValuePairData(Of T) : Inherits KeyValuePairObject(Of String, String)
        Implements IKeyValuePairObject(Of String, String)

        Public Property DataObject As T
    End Class
End Namespace

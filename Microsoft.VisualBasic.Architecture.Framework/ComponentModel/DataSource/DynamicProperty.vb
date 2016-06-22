Imports System.Xml.Serialization
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel.DataSourceModel

    Public Class PropertyValue(Of T) : Inherits Value(Of T)

        ReadOnly __get As Func(Of T)
        ReadOnly __set As Action(Of T)

        Public Overrides Property Value As T
            Get
                Return __get()
            End Get
            Set(value As T)
                Call __set(value)
                MyBase.Value = value
            End Set
        End Property

        Sub New([get] As Func(Of T), [set] As Action(Of T))
            __get = [get]
            __set = [set]
        End Sub

        Public Overloads Shared Narrowing Operator CType(x As PropertyValue(Of T)) As T
            Return x.Value
        End Operator

        Public Overrides Function ToString() As String
            Return Value.GetJson
        End Function

        Public Shared Function GetValue(Of Cls As ClassObject)(x As Cls, name As String) As PropertyValue(Of T)
            Dim value As Object = x.Extension.DynamicHash(name)
            Dim pv As PropertyValue(Of T) = DirectCast(value, PropertyValue(Of T))
            Return pv
        End Function

        Public Shared Sub SetValue(Of Cls As ClassObject)(x As Cls, name As String, value As T)
            Dim pvo As Object = x.Extension.DynamicHash(name)
            Dim pv As PropertyValue(Of T) = DirectCast(pvo, PropertyValue(Of T))
            pv.Value = value
        End Sub

        Public Shared Function [New](Of Cls As ClassObject)(x As Cls, name As String) As PropertyValue(Of T)
            Dim value As New PropertyValue(Of T)(
                   Function() PropertyValue(Of T).GetValue(Of Cls)(x, NameOf(Uid)).Value,
                   Sub(u) Call PropertyValue(Of T).SetValue(Of Cls)(x, NameOf(Uid), u))
            x.Extension.DynamicHash.Value(name) = value
            Return value
        End Function

        ''' <summary>
        ''' 读取<see cref="ClassObject"/>对象之中的一个拓展属性
        ''' </summary>
        ''' <typeparam name="Cls"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Shared Function Read(Of Cls As ClassObject)(x As Cls, name As String) As PropertyValue(Of T)
            If x.Extension Is Nothing Then
                x.Extension = New ExtendedProps
            End If
            Dim prop As Object = x.Extension.DynamicHash(name)
            If prop Is Nothing Then
                prop = PropertyValue(Of T).[New](Of Cls)(x, name)
            End If
            Return DirectCast(prop, PropertyValue(Of T))
        End Function
    End Class

    Public Interface IDynamicMeta(Of T)

        Property Properties As Dictionary(Of String, T)
    End Interface

    ''' <summary>
    ''' Has a dictionary as a dynamics property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public MustInherit Class DynamicPropertyBase(Of T)
        Implements IDynamicMeta(Of T)

        ''' <summary>
        ''' The dynamics property object with specific type of value.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Can not serialize the dictionary object in to xml document.</remarks>
        <XmlIgnore> Public Overridable Property Properties As Dictionary(Of String, T) Implements IDynamicMeta(Of T).Properties
            Get
                If _propHash Is Nothing Then
                    _propHash = New Dictionary(Of String, T)
                End If
                Return _propHash
            End Get
            Set(value As Dictionary(Of String, T))
                _propHash = value
            End Set
        End Property

        Dim _propHash As Dictionary(Of String, T)

        ''' <summary>
        ''' Get value by property name.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public Property Value(name As String) As T
            Get
                If Properties.ContainsKey(name) Then
                    Return Properties(name)
                Else
                    Return Nothing
                End If
            End Get
            Set(value As T)
                Properties(name) = value
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return $"{Properties.Count} Property(s)."
        End Function
    End Class

    ''' <summary>
    ''' Dictionary for [<see cref="String"/>, <typeparamref name="T"/>]
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class [Property](Of T) : Inherits DynamicPropertyBase(Of T)

        Sub New()
        End Sub

        Sub New(initKey As String, initValue As T)
            Call Properties.Add(initKey, initValue)
        End Sub

        <ScriptIgnore> Public Property source As NamedValue(Of T)()
            Get
                Return Properties.ToArray(Function(x) New NamedValue(Of T) With {.Name = x.Key, .x = x.Value})
            End Get
            Set(value As NamedValue(Of T)())
                Properties = value.ToDictionary(Function(x) x.Name, Function(x) x.x)
            End Set
        End Property
    End Class

    ''' <summary>
    ''' The value object have a name string.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NamedValue(Of T) : Implements sIdEnumerable

        ''' <summary>
        ''' Identifier
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property Name As String Implements sIdEnumerable.Identifier
        ''' <summary>
        ''' Object value
        ''' </summary>
        ''' <returns></returns>
        <XmlElement> Public Property x As T

        <ScriptIgnore> Public ReadOnly Property IsEmpty As Boolean
            Get
                Return String.IsNullOrEmpty(Name) AndAlso x Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' Creates a object bind with a specific <see cref="Name"/>.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        Sub New(name As String, value As T)
            Me.Name = name
            Me.x = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name} --> {x.GetJson}"
        End Function

        Public Function FixValue(h As Func(Of T, T)) As NamedValue(Of T)
            Return New NamedValue(Of T)(Name, h(x))
        End Function
    End Structure
End Namespace
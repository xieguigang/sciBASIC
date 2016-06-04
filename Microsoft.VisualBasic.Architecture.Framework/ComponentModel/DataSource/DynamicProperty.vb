Imports System.Xml.Serialization
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel.DataSourceModel

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
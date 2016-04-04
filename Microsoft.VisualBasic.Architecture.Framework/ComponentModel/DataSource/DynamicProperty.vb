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
        '''
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

        <ScriptIgnore> Public Property source As Value(Of T)()
            Get
                Return Properties.ToArray(Function(x) New Value(Of T) With {.Name = x.Key, .x = x.Value})
            End Get
            Set(value As Value(Of T)())
                Properties = value.ToDictionary(Function(x) x.Name, Function(x) x.x)
            End Set
        End Property
    End Class

    Public Structure Value(Of T) : Implements sIdEnumerable

        <XmlAttribute>
        Public Property Name As String Implements sIdEnumerable.Identifier
        <XmlElement> Public Property x As T

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
#Region "Microsoft.VisualBasic::43f86369ec8e0e5f428fd785a2b4ce95, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\Property\DynamicProperty.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Web.Script.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' Abstracts for the dynamics property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IDynamicMeta(Of T)

        ''' <summary>
        ''' Properties
        ''' </summary>
        ''' <returns></returns>
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
        Default Public Property Value(name$) As T
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

        ''' <summary>
        ''' Determines whether the System.Collections.Generic.Dictionary`2 contains the specified
        ''' key.
        ''' </summary>
        ''' <param name="name$">The key to locate in the System.Collections.Generic.Dictionary`2.</param>
        ''' <returns>
        ''' true if the System.Collections.Generic.Dictionary`2 contains an element with
        ''' the specified key; otherwise, false.
        ''' </returns>
        Public Function HasProperty(name$) As Boolean
            If _propHash Is Nothing Then
                Return False
            Else
                Return _propHash.ContainsKey(name)
            End If
        End Function

        ''' <summary>
        ''' 枚举这个动态字典类型之中的所有的键名
        ''' </summary>
        ''' <param name="joinProperties">是否包括属性名称</param>
        ''' <returns></returns>
        Public Function EnumerateKeys(Optional joinProperties As Boolean = False) As String()
            Dim out As New List(Of String)

            If joinProperties Then
                out += MyClass.GetType _
                    .GetProperties(PublicProperty) _
                    .Where(Function(p) p.GetIndexParameters.IsNullOrEmpty) _
                    .Select(Function(p) p.Name) _
                    .ToArray
            End If

            If Not _propHash Is Nothing Then
                out += _propHash.Keys
            End If

            Return out.Distinct.ToArray
        End Function

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

        ''' <summary>
        ''' New with a init property value
        ''' </summary>
        ''' <param name="initKey"></param>
        ''' <param name="initValue"></param>
        Sub New(initKey$, initValue As T)
            Call Properties.Add(initKey, initValue)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        <ScriptIgnore> Public Iterator Property source As IEnumerable(Of NamedValue(Of T))
            Get
                For Each x In Properties
                    Yield New NamedValue(Of T) With {
                        .Name = x.Key,
                        .Value = x.Value
                    }
                Next
            End Get
            Set(value As IEnumerable(Of NamedValue(Of T)))
                Properties = value.ToDictionary(Function(x) x.Name, Function(x) x.Value)
            End Set
        End Property
    End Class
End Namespace

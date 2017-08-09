#Region "Microsoft.VisualBasic::11680c49bf26c1c644badad0d6db83a1, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\Property\DynamicProperty.vb"

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
    ''' This abstract object has a <see cref="propertyTable"/> dictionary keeps as a dynamics property source.
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
                If propertyTable Is Nothing Then
                    propertyTable = New Dictionary(Of String, T)
                End If
                Return propertyTable
            End Get
            Set(value As Dictionary(Of String, T))
                propertyTable = value
            End Set
        End Property

        ''' <summary>
        ''' 动态属性表
        ''' </summary>
        Dim propertyTable As Dictionary(Of String, T)

        ''' <summary>
        ''' Gets/sets item value by using property name.
        ''' (这个函数为安全的函数，当目标属性不存在的时候，会返回空值)
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public Overloads Property ItemValue(name$) As T
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

        Public Function TakeValues(keys$()) As T()
            Return keys.Select(Function(s) Me(s)).ToArray
        End Function

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
            If propertyTable Is Nothing Then
                Return False
            Else
                Return propertyTable.ContainsKey(name)
            End If
        End Function

        ''' <summary>
        ''' 枚举这个动态字典类型之中的所有的键名，这个函数是默认不包含有类型自有的属性名称的
        ''' </summary>
        ''' <param name="joinProperties">是否包括属性名称，默认不包含</param>
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

            If Not propertyTable Is Nothing Then
                out += propertyTable.Keys
            End If

            Return out.Distinct.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"{Properties.Count} Property(s)."
        End Function

        ''' <summary>
        ''' Using for debugger view, this property is usually usefull for the dictionary view 
        ''' to see if any duplicated was existed? 
        ''' </summary>
        ''' <returns></returns>
        Protected ReadOnly Property MyHashCode As Integer
            Get
                Return GetHashCode()
            End Get
        End Property
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

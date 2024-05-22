#Region "Microsoft.VisualBasic::f26dac78c8cc846104843d743440f9b9, Microsoft.VisualBasic.Core\src\Text\Xml\Models\ValueTuples\NamedValues.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 143
    '    Code Lines: 77 (53.85%)
    ' Comment Lines: 44 (30.77%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 22 (15.38%)
    '     File Size: 4.49 KB


    '     Class NamedValue
    ' 
    '         Properties: name, text
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: ToString
    '         Operators: +
    ' 
    '     Class [Property]
    ' 
    '         Properties: comment, name, value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Structure NamedVector
    ' 
    '         Properties: attributes, name, vector
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language

Namespace Text.Xml.Models

    ''' <summary>
    ''' A key-value pair data for xml
    ''' </summary>
    ''' <remarks>
    ''' 键值对集合的键值<see cref="text"/>可能是一大段文本
    ''' </remarks>
    <XmlType("data")> Public Class NamedValue
        Implements INamedValue
        Implements Value(Of String).IValueOf

        ''' <summary>
        ''' The term category/key
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property name As String Implements INamedValue.Key
        ''' <summary>
        ''' The term value
        ''' </summary>
        ''' <returns></returns>
        <XmlText>
        Public Property text As String Implements Value(Of String).IValueOf.Value

        Sub New(name$, value$)
            Me.name = name
            Me.text = value
        End Sub

        ''' <summary>
        ''' get name and value from the given tuple
        ''' </summary>
        ''' <param name="namedValue"></param>
        Sub New(namedValue As NamedValue(Of String))
            Me.name = namedValue.Name
            Me.text = namedValue.Value
        End Sub

        Sub New(kv As KeyValuePair)
            Call Me.New(kv.Key, kv.Value)
        End Sub

        <DebuggerStepThrough>
        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{name}: {text}"
        End Function

        Public Shared Operator +(table As Dictionary(Of String, String), value As NamedValue) As Dictionary(Of String, String)
            Call table.Add(value.name, value.text)
            Return table
        End Operator
    End Class

    ''' <summary>
    ''' Property Info (Property Name and Property Value).
    ''' </summary>
    ''' <remarks>
    ''' (和<see cref="NamedValue"/>所不同的是，这个对象之中的键值对集合的键值都是小段字符串)
    ''' </remarks>
    <XmlType("property")> Public Class [Property] : Implements INamedValue

        ''' <summary>
        ''' The property name
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property name As String Implements INamedValue.Key

        ''' <summary>
        ''' The property value in text string type 
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property value As String

        ''' <summary>
        ''' Data annotation comment.
        ''' </summary>
        ''' <returns></returns>
        <XmlText>
        Public Property comment As String

        Sub New(name$, value$, comment$)
            Me.name = name
            Me.value = value
            Me.comment = comment
        End Sub

        Sub New(term As NamedValue)
            Call Me.New(term.name, term.text, "")
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{name} = ""{value}"""
        End Function
    End Class

    ''' <summary>
    ''' 在这里不实现<see cref="IEnumerable(Of T)"/>是为了方便的实现XML序列化操作
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NamedVector(Of T)
        Implements INamedValue

        <XmlAttribute>
        Public Property name As String Implements IKeyedEntity(Of String).Key
        Public Property vector As T()
        ''' <summary>
        ''' 在这里不使用字典是因为Xml序列化无法序列化字典对象
        ''' </summary>
        ''' <returns></returns>
        Public Property attributes As NamedValue()

        Sub New(namedCollection As NamedCollection(Of T))
            With namedCollection
                name = .Name
                vector = .Value
            End With
        End Sub

        Sub New(name$, vector As T())
            Me.name = name
            Me.vector = vector
        End Sub

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Structure
End Namespace

#Region "Microsoft.VisualBasic::31a753351be9474b091cf5e0bb7c3d03, Microsoft.VisualBasic.Core\src\Text\Xml\Models\ValueTuples\ValuePair.vb"

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

    '   Total Lines: 141
    '    Code Lines: 95
    ' Comment Lines: 24
    '   Blank Lines: 22
    '     File Size: 5.63 KB


    '     Class KeyValuePair
    ' 
    '         Properties: Key, Value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: CreateObject, Distinct, (+2 Overloads) Equals, ToDictionary, ToString
    '         Interface IKeyValuePair
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Text.Xml.Models

    ''' <summary>
    ''' An object for the text file format xml data storage.(用于存储与XML文件之中的字符串键值对对象)
    ''' </summary>
    ''' <remarks>
    ''' + 2016-05-24 为了更好的构建GCModeller项目的数据文档的格式，本类型对象不再继承自<see cref="KeyValuePairObject(Of String, String)"/>类型
    ''' </remarks>
    ''' 
    <XmlType("hashEntry")> Public Class KeyValuePair
        Implements INamedValue
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
        <XmlAttribute> Public Property Key As String Implements INamedValue.Key, IKeyValuePair.Key
        <XmlAttribute> Public Property Value As String Implements IKeyValuePairObject(Of String, String).Value
#End Region

        Public Overloads Shared Widening Operator CType(obj As KeyValuePair(Of String, String)) As KeyValuePair
            Return New KeyValuePair With {
                .Key = obj.Key,
                .Value = obj.Value
            }
        End Operator

        Public Overloads Shared Widening Operator CType(obj As String()) As KeyValuePair
            Return New KeyValuePair With {
                .Key = obj.First,
                .Value = obj.ElementAtOrDefault(1)
            }
        End Operator

        Public Overloads Shared Widening Operator CType(nameValue As NamedValue(Of String)) As KeyValuePair
            Return New KeyValuePair With {
                .Key = nameValue.Name,
                .Value = nameValue.Value
            }
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

        Public Shared Function ToDictionary(list As IEnumerable(Of KeyValuePair)) As Dictionary(Of String, String)
            Dim Dictionary As Dictionary(Of String, String) =
                list.ToDictionary(
                    Function(obj) obj.Key,
                    Function(obj) obj.Value)
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
            Dim List = (From obj In source Select obj Order By obj.Key Ascending).AsList

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
End Namespace

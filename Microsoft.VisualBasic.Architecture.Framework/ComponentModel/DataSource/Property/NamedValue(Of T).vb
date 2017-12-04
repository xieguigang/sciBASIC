#Region "Microsoft.VisualBasic::93635673d157d017d442597e1b45ca01, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\Property\NamedValue(Of T).vb"

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
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' The value object have a name string.(一个具有自己的名称的变量值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NamedValue(Of T) : Implements INamedValue
        Implements IKeyValuePairObject(Of String, T)

        ''' <summary>
        ''' Identifier tag data. you can using this property value as a dictionary key.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property Name As String Implements INamedValue.Key, IKeyValuePairObject(Of String, T).Key

        ''' <summary>
        ''' Object value
        ''' </summary>
        ''' <returns></returns>
        <XmlElement> Public Property Value As T Implements IKeyValuePairObject(Of String, T).Value

        ''' <summary>
        ''' Additional description text about this variable <see cref="Value"/>
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property Description As String

        ''' <summary>
        ''' Does this object have value?
        ''' </summary>
        ''' <returns></returns>
        <XmlIgnore, ScriptIgnore, DataIgnored>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return String.IsNullOrEmpty(Name) AndAlso Value Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' Returns object gettype
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ValueType As Type
            Get
                ' 假若类型参数T是基类型的话，则直接使用GetType(T)只能够得到基类型的信息，无法得到当前的实现类型的信息
                ' 故而要在这里使用对象自身的GetType方法来获取真正的类型信息
                Return Value.GetType
            End Get
        End Property

        ''' <summary>
        ''' Creates a object bind with a specific <see cref="Name"/>.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        Sub New(name$, value As T, Optional describ As String = Nothing)
            Me.Name = name
            Me.Value = value
            Me.Description = describ
        End Sub

        ''' <summary>
        ''' View object.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Try
                If Description.StringEmpty Then
                    Return $"{Name} --> {Value.GetJson}"
                Else
                    Return $"{Name} --> {Value.GetJson} ({Description})"
                End If
            Catch ex As Exception
                Return Name
            End Try
        End Function

        Public Function FixValue(h As Func(Of T, T)) As NamedValue(Of T)
            Return New NamedValue(Of T)(Name, h(Value))
        End Function

        Public Shared Operator +(obj As NamedValue(Of T)) As T
            Return obj.Value
        End Operator

        ''' <summary>
        ''' 这个函数会将<paramref name="value"/>插入为<paramref name="table"/>的第一个元素
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="table"></param>
        ''' <returns></returns>
        Public Shared Operator +(value As NamedValue(Of T), table As Dictionary(Of String, T)) As Dictionary(Of String, T)
            Dim newTable As New Dictionary(Of String, T) From {
                {value.Name, value.Value}
            }

            For Each map As KeyValuePair(Of String, T) In table
                Call newTable.Add(map.Key, map.Value)
            Next

            Return newTable
        End Operator
    End Structure
End Namespace

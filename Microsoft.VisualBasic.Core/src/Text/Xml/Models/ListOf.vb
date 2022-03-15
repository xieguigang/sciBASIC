#Region "Microsoft.VisualBasic::9c531d94a39ce5108889a1b1346f006c, sciBASIC#\Microsoft.VisualBasic.Core\src\Text\Xml\Models\ListOf.vb"

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

    '   Total Lines: 109
    '    Code Lines: 71
    ' Comment Lines: 22
    '   Blank Lines: 16
    '     File Size: 3.64 KB


    '     Interface IList
    ' 
    '         Properties: size
    ' 
    '     Class ListOf
    ' 
    '         Properties: size
    ' 
    '         Function: GenericEnumerator, GetEnumerator
    ' 
    '     Class XmlList
    ' 
    '         Properties: items, TypeComment
    ' 
    '         Function: getCollection, getSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
#If netcore5 = 0 Then
Imports System.Web.Script.Serialization
#End If
Imports System.Xml
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
#If netcore5 = 1 Then
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
#End If
Imports Microsoft.VisualBasic.Linq

Namespace Text.Xml.Models

    Public Interface IList(Of T) : Inherits Enumeration(Of T)

        ReadOnly Property size As Integer

    End Interface

    ''' <summary>
    ''' 可以通过<see cref="AsEnumerable"/>拓展函数转换这个列表对象为枚举类型
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public MustInherit Class ListOf(Of T) : Implements Enumeration(Of T)

        ''' <summary>
        ''' 在这个列表之中的元素数量的长度
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个属性值会在进行XML序列化的时候自动生成，无需设置
        ''' </remarks>
        <XmlAttribute> Public Property size As Integer
            Get
                Return getSize()
            End Get
            Set(value As Integer)
                ' do nothing
            End Set
        End Property

        Public Iterator Function GenericEnumerator() As IEnumerator(Of T) Implements Enumeration(Of T).GenericEnumerator
            For Each item As T In getCollection()
                Yield item
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of T).GetEnumerator
            Yield GenericEnumerator()
        End Function

        Protected MustOverride Function getSize() As Integer
        Protected MustOverride Function getCollection() As IEnumerable(Of T)

    End Class

    Public Class XmlList(Of T) : Inherits ListOf(Of T)
        Implements XmlDataModel.IXmlType

        ''' <summary>
        ''' ReadOnly, Data model type tracking use Xml Comment.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' JSON存储的时候,这个属性会被自动忽略掉
        ''' </remarks>
        <DataMember>
        <IgnoreDataMember>
        <ScriptIgnoreAttribute>
        <SoapIgnore>
        <XmlAnyElement>
        Public Property TypeComment As XmlComment Implements XmlDataModel.IXmlType.TypeComment
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return XmlDataModel.CreateTypeReferenceComment(GetType(T))
            End Get
            Set(value As XmlComment)
                ' Do Nothing
                ' 2018-6-5 this xml comment node cause bug 
                ' when using xml deserialization
            End Set
        End Property

        <XmlElement("item")> Public Property items As T()

        Protected Overrides Function getSize() As Integer
            If items Is Nothing Then
                Return 0
            Else
                Return items.Length
            End If
        End Function

        Protected Overrides Function getCollection() As IEnumerable(Of T)
            If items Is Nothing Then
                Return {}
            Else
                Return items
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(array As T()) As XmlList(Of T)
            Return New XmlList(Of T) With {.items = array}
        End Operator
    End Class
End Namespace

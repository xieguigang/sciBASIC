#Region "Microsoft.VisualBasic::3f0b263a722e820cb244b5f83fcf977a, sciBASIC#\Microsoft.VisualBasic.Core\src\My\JavaScript\JavaScriptObject\JavaScriptObject.vb"

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

    '   Total Lines: 191
    '    Code Lines: 139
    ' Comment Lines: 21
    '   Blank Lines: 31
    '     File Size: 7.35 KB


    '     Class JavaScriptObject
    ' 
    '         Properties: length, this
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetDescription, GetEnumerator, GetGenericJson, GetMemberValue, GetNames
    '                   IEnumerable_GetEnumerator, IEnumerable_GetEnumerator1, Join, ToString
    ' 
    '         Sub: Delete
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace My.JavaScript

    ''' <summary>
    ''' javascript object
    ''' </summary>
    Public Class JavaScriptObject : Implements IEnumerable(Of String), IEnumerable(Of NamedValue(Of Object)), IJavaScriptObjectAccessor

        Dim members As New Dictionary(Of String, JavaScriptValue)

        ''' <summary>
        ''' This javascript object instance
        ''' </summary>
        ''' <returns></returns>
        Protected ReadOnly Property this As JavaScriptObject = Me

        Public ReadOnly Property length As Integer
            Get
                Return members.Count
            End Get
        End Property

        Public Const undefined$ = "UNDEFINED"

        ''' <summary>
        ''' 只针对Public的属性或者字段有效
        ''' </summary>
        ''' <param name="memberName"></param>
        ''' <returns></returns>
        Default Public Property Item(memberName As String) As Object Implements IJavaScriptObjectAccessor.Accessor
            Get
                Return GetMemberValue(memberName, Nothing)
            End Get
            Set(value As Object)
                If members.ContainsKey(memberName) Then
                    If members(memberName) Is Nothing Then
                        members(memberName) = New JavaScriptValue With {.Literal = value}
                    Else
                        members(memberName).SetValue(value)
                    End If
                Else
                    ' 添加一个新的member
                    members(memberName) = New JavaScriptValue With {
                        .Literal = value
                    }
                End If
            End Set
        End Property

        ''' <summary>
        ''' 如果存在无参数的函数，则也会被归类为只读属性？
        ''' </summary>
        Sub New()
            Dim type As Type = MyClass.GetType
            Dim properties As PropertyInfo() = type.GetProperties(PublicProperty).ToArray
            Dim fields As FieldInfo() = type.GetFields(PublicProperty).ToArray
            Dim value As JavaScriptValue

            For Each prop As PropertyInfo In properties
                If prop.Name = NameOf(Me.length) OrElse Not prop.GetIndexParameters.IsNullOrEmpty Then
                    Continue For
                End If

                value = New JavaScriptValue(New BindProperty(Of DataFrameColumnAttribute)(prop), Me)
                members(prop.Name) = value
            Next
            For Each field As FieldInfo In fields
                value = New JavaScriptValue(New BindProperty(Of DataFrameColumnAttribute)(field), Me)
                members(field.Name) = value
            Next
        End Sub

        Sub New(obj As Dictionary(Of String, Object))
            Call Me.New

            For Each item As KeyValuePair(Of String, Object) In obj
                Me(item.Key) = item.Value
            Next
        End Sub

        Public Shared Function Join(left As JavaScriptObject, right As JavaScriptObject) As JavaScriptObject
            Dim leftObj As Dictionary(Of String, Object) = left.GetGenericJson

            For Each item As NamedValue(Of Object) In DirectCast(right, IEnumerable(Of NamedValue(Of Object)))
                If Not leftObj.ContainsKey(item.Name) Then
                    leftObj.Add(item.Name, item.Value)
                End If
            Next

            Return New JavaScriptObject(leftObj)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Delete(name As String)
            Call members.Remove(name)
        End Sub

        Public Function GetMemberValue(memberName As String, ByRef access As MemberAccessorResult) As Object
            If members.ContainsKey(memberName) Then
                Dim value As JavaScriptValue = members(memberName)

                If value Is Nothing Then
                    access = MemberAccessorResult.Undefined
                    Return Nothing
                ElseIf value.IsConstant Then
                    access = MemberAccessorResult.ExtensionProperty
                Else
                    access = MemberAccessorResult.ClassMemberProperty
                End If

                Return value.GetValue
            Else
                ' Returns undefined in javascript
                access = MemberAccessorResult.Undefined
            End If

            Return Nothing
        End Function

        Public Shared Function GetDescription(jsObj As JavaScriptObject) As Dictionary(Of String, Descriptor)
            Dim desc As New Dictionary(Of String, Descriptor)

            For Each p As String In jsObj
                desc(p) = New Descriptor With {
                    .value = jsObj(p),
                    .configurable = True,
                    .enumerable = True,
                    .writable = True
                }
            Next

            Return desc
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetNames() As String()
            Return members.Keys.ToArray
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of String) Implements IEnumerable(Of String).GetEnumerator
            For Each key As String In members.Keys
                Yield key
            Next
        End Function

        Protected Overridable Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
            Yield IEnumerable_GetEnumerator1()
        End Function

        Private Iterator Function IEnumerable_GetEnumerator1() As IEnumerator(Of NamedValue(Of Object)) Implements IEnumerable(Of NamedValue(Of Object)).GetEnumerator
            Dim access As MemberAccessorResult = MemberAccessorResult.Undefined
            Dim value As Object
            Dim pop As NamedValue(Of Object)

            For Each key As String In members.Keys
                value = GetMemberValue(key, access)
                pop = New NamedValue(Of Object) With {
                    .Name = key,
                    .Value = value,
                    .Description = access.Description
                }

                Yield pop
            Next
        End Function

        ''' <summary>
        ''' populate all members data
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetGenericJson() As Dictionary(Of String, Object)
            Return members.ToDictionary(Function(a) a.Key, Function(a) a.Value.GetValue)
        End Function

        Public Overrides Function ToString() As String
            Return GetGenericJson.GetJson(knownTypes:={
                GetType(Integer),
                GetType(String),
                GetType(Double),
                GetType(Boolean)
            })
        End Function
    End Class
End Namespace

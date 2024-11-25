#Region "Microsoft.VisualBasic::8eb9c478161cc10da46fc191567ea6e4, Microsoft.VisualBasic.Core\src\My\JavaScript\JavaScriptObject\JavaScriptObject.vb"

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

    '   Total Lines: 320
    '    Code Lines: 230 (71.88%)
    ' Comment Lines: 37 (11.56%)
    '    - Xml Docs: 91.89%
    ' 
    '   Blank Lines: 53 (16.56%)
    '     File Size: 12.79 KB


    '     Class JavaScriptObject
    ' 
    '         Properties: data, length, this
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) CreateDynamicObject, (+2 Overloads) CreateObject, GetDescription, GetEnumerator, GetGenericJSON
    '                   GetMemberValue, GetNames, IEnumerable_GetEnumerator, IEnumerable_GetEnumerator1, Join
    '                   ToString
    ' 
    '         Sub: Delete, loadObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace My.JavaScript

    ''' <summary>
    ''' javascript object
    ''' </summary>
    Public Class JavaScriptObject
        Implements IEnumerable(Of String),          ' for each key name, implements javascript foreach
            IEnumerable(Of NamedValue(Of Object)),  ' for each key-value pair tuple value
            IJavaScriptObjectAccessor

        Dim members As New Dictionary(Of String, JavaScriptValue)

        ''' <summary>
        ''' This javascript object instance
        ''' </summary>
        ''' <returns></returns>
        Protected ReadOnly Property this As JavaScriptObject = Me

        ''' <summary>
        ''' the size of the member collection in this javascript object
        ''' </summary>
        ''' <returns></returns>
        <DataIgnored>
        Public ReadOnly Property length As Integer
            Get
                Return members.Count
            End Get
        End Property

        Public Const undefined$ = "UNDEFINED"

        <DataIgnored>
        Public ReadOnly Property data As JavaScriptValue()
            Get
                Return members.Values.ToArray
            End Get
        End Property

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
            Call loadObject(this:=Me)
        End Sub

        Private Shared Sub loadObject(ByRef this As JavaScriptObject)
            Dim type As Type = this.GetType
            Dim value As JavaScriptValue

            Static properties As New Dictionary(Of Type, PropertyInfo())
            Static fields As New Dictionary(Of Type, FieldInfo())

            ' check of the static cache hit for the clr type schema
            If Not properties.ContainsKey(type) Then
                Call properties.Add(type, type _
                    .GetProperties(PublicProperty) _
                    .Where(Function(t)
                               Return t.Name <> NameOf(JavaScriptObject.length) AndAlso
                                    t.Name <> NameOf(JavaScriptObject.data) AndAlso
                                    t.GetCustomAttribute(Of DataIgnoredAttribute) Is Nothing AndAlso
                                    t.GetIndexParameters _
                                     .IsNullOrEmpty
                           End Function) _
                    .ToArray)
            End If
            If Not fields.ContainsKey(type) Then
                Call fields.Add(type, type.GetFields(PublicProperty).ToArray)
            End If

            For Each prop As PropertyInfo In properties(type)
                value = New JavaScriptValue(New BindProperty(Of DataFrameColumnAttribute)(prop), this)
                this.members(prop.Name) = value
            Next
            For Each field As FieldInfo In fields(type)
                value = New JavaScriptValue(New BindProperty(Of DataFrameColumnAttribute)(field), this)
                this.members(field.Name) = value
            Next
        End Sub

        Sub New(obj As Dictionary(Of String, Object))
            Call Me.New

            For Each item As KeyValuePair(Of String, Object) In obj
                Me(item.Key) = item.Value
            Next
        End Sub

        Sub New(keys As String(), values As Object())
            Call Me.New()

            If keys.Length <> values.Length Then
                Throw New InvalidExpressionException($"this size of the keys({keys.Length}) should be equals to the size of the values data({values.Length})!")
            Else
                For i As Integer = 0 To keys.Length - 1
                    Me(keys(i)) = values(i)
                Next
            End If
        End Sub

        Public Shared Function Join(left As JavaScriptObject, right As JavaScriptObject) As JavaScriptObject
            Dim leftObj As Dictionary(Of String, Object) = left.GetGenericJSON

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
        ''' <returns>
        ''' Apply for create json in dynamics
        ''' </returns>
        ''' <remarks>
        ''' only works for the object in primitive type
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetGenericJSON() As Dictionary(Of String, Object)
            Return members.ToDictionary(Function(a) a.Key, Function(a) a.Value.GetValue)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return GetGenericJSON.GetJson(knownTypes:=DataFramework.GetPrimitiveTypes)
        End Function

        ''' <summary>
        ''' A wrapper for <see cref="DynamicType.Create"/>
        ''' </summary>
        ''' <param name="names"></param>
        ''' <param name="values"></param>
        ''' <returns></returns>
        Public Shared Function CreateDynamicObject(names As String(), values As Object()) As Object
            Dim obj As New Dictionary(Of String, Object)

            For i As Integer = 0 To names.Length - 1
                obj(names(i)) = values(i)
            Next

            Return DynamicType.Create(obj)
        End Function

        Public Shared Function CreateDynamicObject(dynamic As Type, values As IEnumerable(Of KeyValuePair(Of String, Object))) As Object
            Dim obj As Object = Activator.CreateInstance(dynamic)
            Dim schema = DataFramework.Schema(dynamic, flag:=PropertyAccess.Writeable, nonIndex:=True)

            For Each tuple As KeyValuePair(Of String, Object) In values
                Dim value As Object = tuple.Value
                Dim prop As PropertyInfo = schema(tuple.Key)

                Call prop.SetValue(obj, value)
            Next

            Return obj
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateObject(Of T As Class)() As T
            Return CreateObject(GetType(T))
        End Function

        Public Function CreateObject(type As Type) As Object
            Dim propWriter As Dictionary(Of String, PropertyInfo) = type _
                .GetProperties(PublicProperty) _
                .Where(Function(p) p.GetIndexParameters.IsNullOrEmpty) _
                .ToDictionary(Function(p)
                                  Return p.Name
                              End Function)
            Dim obj As Object = Activator.CreateInstance(type)

            For Each name As String In Me
                Dim value As Object = Me(name)

                If Not propWriter.ContainsKey(name) Then
                    Continue For
                End If

                Dim target As PropertyInfo = propWriter(name)

                If DataFramework.IsPrimitive(target.PropertyType) Then
                    value = Conversion.CTypeDynamic(value, target.PropertyType)
                ElseIf target.PropertyType.IsArray Then
                    If DataFramework.IsPrimitive(target.PropertyType.GetElementType) Then
                        value = DirectCast(value, Array).CTypeDynamic(target.PropertyType)
                    Else
                        Dim template As Type = target.PropertyType.GetElementType
                        Dim src As JavaScriptObject() = DirectCast(value, Array).DirectCast(GetType(JavaScriptObject))
                        Dim vec As Array = Array.CreateInstance(template, src.Length)

                        For i As Integer = 0 To src.Length - 1
                            vec.SetValue(src(i).CreateObject(template), i)
                        Next

                        value = vec
                    End If
                ElseIf TypeOf value Is JavaScriptObject Then
                    value = DirectCast(value, JavaScriptObject).CreateObject(target.GetType)
                Else
                    value = Nothing
                End If

                Call target.SetValue(obj, value)
            Next

            Return obj
        End Function
    End Class
End Namespace

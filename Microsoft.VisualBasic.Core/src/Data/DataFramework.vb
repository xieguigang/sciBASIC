#Region "Microsoft.VisualBasic::d8db0a7ee800255f64486642b63ffb37, Microsoft.VisualBasic.Core\src\Data\DataFramework.vb"

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

    '   Total Lines: 489
    '    Code Lines: 305
    ' Comment Lines: 120
    '   Blank Lines: 64
    '     File Size: 21.71 KB


    '     Module DataFramework
    ' 
    '         Properties: Flags, StringBuilders, StringParsers
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateObject, DictionaryTable, getOrCache, GetValue, IsComplexType
    '                   ParseSchemaInternal, (+2 Overloads) Schema, ValueTable
    '         Delegate Function
    ' 
    '             Function: GetPrimitiveTypes, IsCollection, IsIntegerType, IsNullable, IsNumericCollection
    '                       IsNumericType, IsPrimitive, valueToString
    '         Enum EnumCastTo
    ' 
    '             [integer], [string], none
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Scripting.Runtime.NumberConversionRoutines
Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' 在目标对象中必须要具有一个属性有自定义属性<see cref="DataFrameColumnAttribute"></see>
    ''' </summary>
    ''' <remarks></remarks>
    Public Module DataFramework

        ''' <summary>
        ''' Controls for <see cref="PropertyAccess"/> on <see cref="PropertyInfo"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Flags As IReadOnlyDictionary(Of PropertyAccess, Predicate(Of PropertyInfo))

        Sub New()
            Flags = New Dictionary(Of PropertyAccess, Predicate(Of PropertyInfo)) From {
                                                                                        _
                {PropertyAccess.Readable, Function(p) p.CanRead},
                {PropertyAccess.ReadWrite, Function(p) p.CanRead AndAlso p.CanWrite},
                {PropertyAccess.Writeable, Function(p) p.CanWrite},
                {PropertyAccess.NotSure, Function(p) True}
            }
        End Sub

        ''' <summary>
        ''' 获取类型之中的属性列表
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="flag"></param>
        ''' <param name="nonIndex"><see cref="PropertyInfo.GetIndexParameters"/> IsNullOrEmpty</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个函数不会缓存类型信息，因为参数组合条件过多
        ''' </remarks>
        Public Function Schema(Of T)(flag As PropertyAccess,
                                     Optional nonIndex As Boolean = False,
                                     Optional primitive As Boolean = False,
                                     Optional binds As BindingFlags = PublicProperty) As Dictionary(Of String, PropertyInfo)

            With GetType(T).Schema(flag, binds, nonIndex)
                If primitive Then
                    Return .Keys _
                        .Where(Function(k)
                                   Return IsPrimitive(.ByRef(k).PropertyType)
                               End Function) _
                        .ToDictionary(Function(key) key,
                                      Function(key)
                                          Return .ByRef(key)
                                      End Function)
                Else
                    Return .ByRef
                End If
            End With
        End Function

        ''' <summary>
        ''' (instance) ``Public Property xxxxx As xxxxx``
        ''' </summary>
        Public Const PublicProperty As BindingFlags = BindingFlags.Public Or BindingFlags.Instance
        ''' <summary>
        ''' (statics) ``Public Shared Property xxxx As xxxx``
        ''' </summary>
        Public Const PublicShared As BindingFlags = BindingFlags.Public Or BindingFlags.Static

        ''' <summary>
        ''' 请注意：对于VisualBasic的My.Resources.Resources类型而言，里面的属性都是Friend Shared访问类型的，
        ''' 所以在解析内部资源管理器对象的时候应该要特别注意<paramref name="binds"/>参数值的设置，
        ''' 因为这个参数默认是<see cref="PublicProperty"/>
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="flag"></param>
        ''' <param name="binds"></param>
        ''' <param name="nonIndex"><see cref="PropertyInfo.GetIndexParameters"/> IsNullOrEmpty</param>
        ''' <returns></returns>
        <Extension>
        Public Function Schema(type As Type, flag As PropertyAccess,
                               Optional binds As BindingFlags = PublicProperty,
                               Optional nonIndex As Boolean = False) As Dictionary(Of String, PropertyInfo)

            Dim props As IEnumerable(Of PropertyInfo) = type _
                .GetProperties(binds) _
                .ToArray

            props = props _
                .Where(Flags(flag).AsLambda) _
                .ToArray

            If nonIndex Then
                props = props _
                    .Where(Function(p)
                               Return p.GetIndexParameters.IsNullOrEmpty
                           End Function)
            End If

            Return props.ToDictionary(Function(x) x.Name)
        End Function

        ReadOnly alwaysTrue As [Default](Of Predicate(Of Object)) = New Predicate(Of Object)(Function() True)

        ''' <summary>
        ''' 将对象之中的所有属性值都取出来以字符串的形式生成一个字典对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="where">用来判断属性值是否应该被添加进入字典之中</param>
        ''' <returns></returns>
        <Extension>
        Public Function DictionaryTable(Of T)(x As T,
                                              Optional primitiveType As Boolean = False,
                                              Optional where As Predicate(Of Object) = Nothing) As Dictionary(Of String, String)

            Dim schema As Dictionary(Of String, PropertyInfo) = GetType(T).getOrCache
            Dim table As New Dictionary(Of String, String)
            Dim obj

            where = where Or alwaysTrue

            If primitiveType Then
                For Each key As String In schema.Keys.ToArray
                    If Not IsPrimitive(schema(key).PropertyType) Then
                        Call schema.Remove(key)
                    End If
                Next
            End If

            For Each key As String In schema.Keys
                obj = schema(key).GetValue(x)

                If where(obj) Then
                    table(key) = Scripting.ToString(obj)
                End If
            Next

            Return table
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ValueTable(Of T)(x As T) As Dictionary(Of String, Object)
            Return GetType(T).getOrCache _
                             .ToDictionary(Function(p) p.Key,
                                           Function(p)
                                               Return p.Value.GetValue(x)
                                           End Function)
        End Function

        ''' <summary>
        ''' Helper for <see cref="DictionaryTable(Of T)"/>
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Private Function getOrCache(type As Type) As Dictionary(Of String, PropertyInfo)
            Static schemaCache As New Dictionary(Of Type, Dictionary(Of String, PropertyInfo))

            If Not schemaCache.ContainsKey(type) Then
                ' Gets all object instance property and also 
                ' the properties should be public access 
                ' without index access
                schemaCache(type) = type.Schema(
                    PropertyAccess.Readable,
                    PublicProperty,
                    nonIndex:=True
                )
            End If

            Return New Dictionary(Of String, PropertyInfo)(schemaCache(type))
        End Function

#If NET_40 = 0 Then

        ''' <summary>
        ''' Converts the .NET primitive types from string.(将字符串数据类型转换为其他的数据类型)
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property StringParsers As New Dictionary(Of Type, IStringParser) From {
                                                                                               _
            {GetType(String), Function(strValue As String) strValue},
            {GetType(Boolean), AddressOf ParseBoolean},
            {GetType(DateTime), Function(strValue As String) CType(strValue, DateTime)},
            {GetType(Double), AddressOf Val},
            {GetType(Integer), Function(strValue As String) CInt(strValue)},
            {GetType(Long), Function(strValue As String) CLng(strValue)},
            {GetType(Single), Function(s) CSng(Val(s))},
            {GetType(Char), Function(s) s.FirstOrDefault}
        }

        ''' <summary>
        ''' Object <see cref="Object.ToString"/> methods.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 在这个和Scripting命名空间下的基础类型的判断不一样
        ''' 在这里只会判断.NET框架中的基础类型
        ''' 在Scripting命名空间下的基础类型,是规定为所有包含有类型和字符串值之间的隐式转换的类型
        ''' </remarks>
        Public ReadOnly Property StringBuilders As New Dictionary(Of Type, IStringBuilder) From {
                                                                                                 _
            {GetType(String), Function(s) If(s Is Nothing, "", CStr(s))},
            {GetType(Boolean), AddressOf DataFramework.valueToString},
            {GetType(DateTime), AddressOf DataFramework.valueToString},
            {GetType(Double), AddressOf DataFramework.valueToString},
            {GetType(Integer), AddressOf DataFramework.valueToString},
            {GetType(Long), AddressOf DataFramework.valueToString},
            {GetType(Byte), AddressOf DataFramework.valueToString},
            {GetType(ULong), AddressOf DataFramework.valueToString},
            {GetType(UInteger), AddressOf DataFramework.valueToString},
            {GetType(Short), AddressOf DataFramework.valueToString},
            {GetType(UShort), AddressOf DataFramework.valueToString},
            {GetType(Char), AddressOf DataFramework.valueToString},
            {GetType(Single), AddressOf DataFramework.valueToString},
            {GetType(SByte), AddressOf DataFramework.valueToString}
        }

        Public Delegate Function CTypeDynamics(obj As Object, ConvertType As Type) As Object

        ''' <summary>
        ''' 这个函数是为了提供转换的方法给字典对象<see cref="StringBuilders"/>
        ''' </summary>
        ''' <param name="o">
        ''' 因为<see cref="StringBuilders"/>要求的是<see cref="IStringBuilder"/>，
        ''' 即<see cref="Object"/>类型转换为字符串，所以在这里就不适用T泛型了，而是直接
        ''' 使用<see cref="Object"/>类型
        ''' </param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function valueToString(o) As String
            Return CStrSafe(o)
        End Function

        ''' <summary>
        ''' Is one of the primitive type in the hash <see cref="StringBuilders"/>?
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsPrimitive(type As Type) As Boolean
            Return StringBuilders.ContainsKey(type)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPrimitiveTypes() As IEnumerable(Of Type)
            Return StringBuilders.Keys
        End Function

        Public Function IsNullable(type As Type) As Boolean
            Return False
        End Function

        ReadOnly numerics As Type() = {
            GetType(Integer), GetType(Long), GetType(Short), GetType(Double), GetType(Byte),
            GetType(UInteger), GetType(ULong), GetType(UShort), GetType(Single), GetType(SByte), GetType(Decimal)
        }

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T">the data type of the element inside a data collection</typeparam>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function IsCollection(Of T)(type As Type) As Boolean
            If type.IsArray Then
                Return type.GetElementType Is GetType(T)
            Else
                If type _
                    .ImplementInterface(GetType(IEnumerable(Of )) _
                    .MakeGenericType(GetType(T))) Then

                    Return True
                End If
            End If

            Return False
        End Function

        ''' <summary>
        ''' Does the given type is any kind of numeric collection type?
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function IsNumericCollection(type As Type) As Boolean
            If type.IsArray Then
                Return IsNumericType(type.GetElementType)
            Else
                Static numerics As Type() = DataFramework.numerics _
                    .Select(Function(t) GetType(IEnumerable(Of )).MakeGenericType(t)) _
                    .ToArray

                For Each num As Type In numerics
                    If type.ImplementInterface(num) Then
                        Return True
                    End If
                Next
            End If

            Return False
        End Function

        ''' <summary>
        ''' the given <paramref name="type"/> is any of the CLR numeric primitive type?
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="includeComplex">
        ''' and also should treat the complex number as the primitive numeric type?
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' This function testing all of the possible numeric type at here
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsNumericType(type As Type, Optional includeComplex As Boolean = False) As Boolean
            Static test As New Dictionary(Of String, Boolean)

            Dim key As String = includeComplex.ToString & type.FullName

            SyncLock test
                If Not test.ContainsKey(key) Then
                    test(key) = numerics.Any(Function(num) num Is type) OrElse (includeComplex AndAlso type Is GetType(Complex))
                End If

                Return test(key)
            End SyncLock
        End Function

        Public Function IsIntegerType(type As Type) As Boolean
            Static ints As Type() = {
                GetType(Integer), GetType(Short), GetType(Byte), GetType(Long),
                GetType(UInteger), GetType(UShort), GetType(SByte), GetType(ULong)
            }

            Return ints.Any(Function(int) int Is type)
        End Function

        Public Enum EnumCastTo
            none
            [string]
            [integer]
        End Enum

        ''' <summary>
        ''' 如果目标类型的属性之中值包含有基础类型，则是一个非复杂类型，反之包含任意一个非基础类型，则是一个复杂类型
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="enumCast">by default we treat the enum type as non-primitive type.</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsComplexType(type As Type, Optional enumCast As EnumCastTo = EnumCastTo.none) As Boolean
            Return Not type _
                .Schema(PropertyAccess.NotSure, PublicProperty, True) _
                .Values _
                .Where(Function(t)
                           Dim ptype As Type = t.PropertyType

                           If ptype.IsEnum AndAlso enumCast <> EnumCastTo.none Then
                               Return False
                           Else
                               Return Not IsPrimitive(ptype)
                           End If
                       End Function) _
                .FirstOrDefault Is Nothing
        End Function
#End If

        ''' <summary>
        ''' Convert target data object collection into a datatable for the data source of the ``DataGridView``.
        ''' (将目标对象集合转换为一个数据表对象，用作DataGridView控件的数据源)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateObject(Of T)(source As IEnumerable(Of T)) As DataTable
            Dim columns = ParseSchemaInternal(GetType(T))
            Dim table As New DataTable
            Dim type As Type

            For Each column In columns.Values
                type = DirectCast(column.member, PropertyInfo).PropertyType
                Call table.Columns.Add(column.Identity, type)
            Next

            Dim fields As IEnumerable(Of BindProperty(Of DataFrameColumnAttribute)) =
                columns.Values

            For Each row As T In source
                Dim LQuery = LinqAPI.Exec(Of Object) _
 _
                    () <= From column As BindProperty(Of DataFrameColumnAttribute)
                          In fields
                          Select column.GetValue(row)

                Call table.Rows.Add(LQuery)
            Next

            Return table
        End Function

        ''' <summary>
        ''' Retrive data from a specific datatable object.(从目标数据表中获取数据)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="dataTable"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetValue(Of T)(dataTable As DataTable) As T()
            Dim Columns = ParseSchemaInternal(GetType(T))
            Dim rtvlData As T() = New T(dataTable.Rows.Count - 1) {}
            Dim i As Integer = 0

            Dim schema As List(Of KeyValuePair(Of Integer, PropertyInfo)) = New List(Of KeyValuePair(Of Integer, PropertyInfo))
            For Each column As DataColumn In dataTable.Columns
                Dim LQuery As BindProperty(Of DataFrameColumnAttribute) =
                    LinqAPI.DefaultFirst(Of BindProperty(Of DataFrameColumnAttribute)) <=
                        From schemaColumn As BindProperty(Of DataFrameColumnAttribute)
                        In Columns.Values
                        Where String.Equals(schemaColumn.Identity, column.ColumnName)
                        Select schemaColumn

                If Not LQuery.IsNull Then
                    Call schema.Add(New KeyValuePair(Of Integer, PropertyInfo)(column.Ordinal, LQuery.member))
                End If
            Next

            For Each row As DataRow In dataTable.Rows
                Dim obj As T = Activator.CreateInstance(Of T)()
                For Each column In schema
                    Dim value = row.Item(column.Key)
                    If IsDBNull(value) OrElse value Is Nothing Then
                        Continue For
                    End If
                    Call column.Value.SetValue(obj, value, Nothing)
                Next

                rtvlData(i) = obj
                i += 1
            Next
            Return rtvlData
        End Function

        Private Function ParseSchemaInternal(type As Type) As Dictionary(Of String, BindProperty(Of DataFrameColumnAttribute))
            Dim dataType As Type = GetType(DataFrameColumnAttribute)
            Dim props As PropertyInfo() = type.GetProperties
            Dim columns = (From [property] As PropertyInfo
                           In props
                           Let attrs As Object() = [property].GetCustomAttributes(dataType, True)
                           Where Not attrs.IsNullOrEmpty
                           Select colMaps =
                               DirectCast(attrs.First, DataFrameColumnAttribute), [property]
                           Order By colMaps.Index Ascending).AsList

            For Each column In columns
                If String.IsNullOrEmpty(column.colMaps.Name) Then
                    Call column.colMaps.SetNameValue(column.property.Name)
                End If
            Next

            Dim unIndexColumn = (From col
                                 In columns
                                 Where col.colMaps.Index <= 0
                                 Select col  ' 未建立索引的对象按照名称排序
                                 Order By col.colMaps.Name Ascending).ToArray ' 由于在后面会涉及到修改list对象，所以在这里使用ToArray来隔绝域list的关系，避免出现冲突

            ' 将未建立索引的对象放置到列表的最末尾
            For Each col In unIndexColumn
                Call columns.Remove(col)
                Call columns.Add(col)
            Next

            Return columns.ToDictionary(
                Function(x) x.colMaps.Name,
                Function(x)
                    Return New BindProperty(Of DataFrameColumnAttribute)(x.colMaps, x.property)
                End Function)
        End Function
    End Module
End Namespace

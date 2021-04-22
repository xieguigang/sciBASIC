#Region "Microsoft.VisualBasic::10f2dec9ca2a1b39d0e800ba8f229484, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Property\NamedCollection.vb"

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

    '     Structure NamedCollection
    ' 
    '         Properties: description, IsEmpty, Length, name, value
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: GetEnumerator, GetValues, IEnumerable_GetEnumerator, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace ComponentModel.DataSourceModel

    REM 2017-7-18 Add(T) not working on the Add(Object)???

    ' System.InvalidOperationException: 要使 XML 可序列化，从 IEnumerable 继承的类型必须在继承层次结构的所有级别上实现 Add(System.Object)。Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedCollection`1[[Biodeep.Reference.Library.mysql.data_lc_ms_metadb,
    ' Biodeep.Reference.Library, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]] 不支持实现 Add(System.Object)。
    '   在 System.Xml.Serialization.TypeScope.GetEnumeratorElementType(Type type, TypeFlags& flags)
    '   在 System.Xml.Serialization.TypeScope.ImportTypeDesc(Type type, MemberInfo memberInfo, Boolean directReference)
    '   在 System.Xml.Serialization.TypeScope.GetTypeDesc(Type type, MemberInfo source, Boolean directReference, Boolean throwOnError)
    '   在 System.Xml.Serialization.TypeScope.ImportTypeDesc(Type type, MemberInfo memberInfo, Boolean directReference)
    '   在 System.Xml.Serialization.TypeScope.GetTypeDesc(Type type, MemberInfo source, Boolean directReference, Boolean throwOnError)
    '   在 System.Xml.Serialization.ModelScope.GetTypeModel(Type type, Boolean directReference)
    '   在 System.Xml.Serialization.XmlReflectionImporter.ImportTypeMapping(Type type, XmlRootAttribute root, String defaultNamespace)
    '   在 System.Xml.Serialization.XmlSerializer..ctor(Type type, String defaultNamespace)
    '   在 System.Xml.Serialization.XmlSerializer..ctor(Type type)
    '   在 Microsoft.VisualBasic.XmlExtensions.GetXml(Object obj, Type type, Boolean throwEx, XmlEncodings xmlEncoding) 位置 D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Doc\XmlExtensions.vb:行号 193
    '   --- 内部异常堆栈跟踪的结尾 ---
    '   在 Microsoft.VisualBasic.XmlExtensions.GetXml(Object obj, Type type, Boolean throwEx, XmlEncodings xmlEncoding) 位置 D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Doc\XmlExtensions.vb:行号 207
    '   在 Microsoft.VisualBasic.XmlExtensions.GetXml[T](T obj, Boolean ThrowEx, XmlEncodings xmlEncoding) 位置 D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Doc\XmlExtensions.vb:行号 162
    '   在 Biodeep.Reference.Library.Build.CLI.Grouping(CommandLine args) 位置 D:\smartnucl_integrative\biodeepDB\Build.Ref\CLI\Database.vb:行号 42
    '   --- 内部异常堆栈跟踪的结尾 ---
    '   在 System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor)
    '   在 System.Reflection.RuntimeMethodInfo.UnsafeInvokeInternal(Object obj, Object[] parameters, Object[] arguments)
    '   在 System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
    '   在 System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters)
    '   在 Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.__directInvoke(Object[] callParameters, Object target, Boolean Throw) 位置 D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\EntryPoints\APIEntryPoint.vb:行号 209
    '   --- 内部异常堆栈跟踪的结尾 ---

    ''' <summary>
    ''' The value object collection that have a name string, using <see cref="NamedVector(Of T)"/> 
    ''' for XML serialization instead of using this data model.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NamedCollection(Of T) : Implements INamedValue
        Implements IKeyValuePairObject(Of String, T())
        Implements Value(Of T()).IValueOf
        Implements IEnumerable(Of T)
        Implements IGrouping(Of String, T)
        Implements IsEmpty

        ''' <summary>
        ''' 这个集合对象的标识符名称
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property name As String Implements _
            IKeyedEntity(Of String).Key,
            IKeyValuePairObject(Of String, T()).Key,
            IGrouping(Of String, T).Key

        ''' <summary>
        ''' 目标集合对象
        ''' </summary>
        ''' <returns></returns>
        Public Property value As T() Implements IKeyValuePairObject(Of String, T()).Value, Value(Of T()).IValueOf.Value

        ''' <summary>
        ''' 目标集合对象的描述信息
        ''' </summary>
        ''' <returns></returns>
        Public Property description As String

        ''' <summary>
        ''' 当前的这个命名的目标集合对象是否是空对象？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return name Is Nothing AndAlso value Is Nothing
            End Get
        End Property

        Default Public Property Item(index As Integer) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return value(index)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As T)
                Me.value(index) = value
            End Set
        End Property

        ''' <summary>
        ''' <see cref="Value"/> array length
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If value Is Nothing Then
                    Return 0
                Else
                    Return value.Length
                End If
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="source">名称属性<see cref="NamedValue(Of T).Name"/></param>必须是相同的
        Sub New(source As IEnumerable(Of NamedValue(Of T)))
            Dim array = source.ToArray

            name = array(Scan0).Name
            value = array.Select(Function(x) x.Value).ToArray
            description = array _
                .Select(Function(x) x.Description) _
                .Where(Function(s) Not s.StringEmpty) _
                .Distinct _
                .JoinBy("; ")
        End Sub

        Sub New(xmlNode As NamedVector(Of T))
            With xmlNode
                name = .name
                value = .vector
            End With
        End Sub

        Sub New(name$, data As IEnumerable(Of T), Optional description$ = Nothing)
            Call Me.New(name, data.SafeQuery.ToArray, description)
        End Sub

        Sub New(name$, value As T(), Optional description$ = Nothing)
            Me.name = name
            Me.description = description
            Me.value = value
        End Sub

        Public Function GetValues() As NamedValue(Of T)()
            Dim name$ = Me.name
            Dim describ$ = description

            Return value _
                .Select(Function(v)
                            Return New NamedValue(Of T) With {
                                .Name = name,
                                .Description = describ,
                                .Value = v
                            }
                        End Function) _
                .ToArray
        End Function

        Public Overrides Function ToString() As String
            If IsEmpty Then
                Return "NULL"
            Else
                Return name
            End If
        End Function

        <DebuggerStepThrough>
        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each obj As T In value.SafeQuery
                Yield obj
            Next
        End Function

        <DebuggerStepThrough>
        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

#If NET_48 Or netcore5 = 1 Then

        <DebuggerStepThrough>
        Public Shared Widening Operator CType(tuple As (name$, value As T())) As NamedCollection(Of T)
            Return New NamedCollection(Of T)(tuple.name, tuple.value)
        End Operator

#End If

        <DebuggerStepThrough>
        Public Shared Operator =(list As NamedCollection(Of T), count As Integer) As Boolean
            Return list.Count = count
        End Operator

        <DebuggerStepThrough>
        Public Shared Operator <>(list As NamedCollection(Of T), count As Integer) As Boolean
            Return Not list = count
        End Operator
    End Structure
End Namespace

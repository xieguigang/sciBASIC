#Region "Microsoft.VisualBasic::5c8d2e60b4a19f3c7e0d8b6a2f41c935, vs_solutions\JavaScript\TypeDefinitions\TsModel.vb"

    ' Author:
    ' 
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
    ' GNU General PUBLIC License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.


    ' /********************************************************************************/

    ' Summaries:

    '     Enum TsTypeKind

    '     Class TsConstructor
    ' 
    '         Properties: Info, Parameters

    '     Class TsField
    ' 
    '         Properties: FieldType, Info, IsLiteral, IsReadOnly, IsStatic, Name

    '     Class TsIndexer
    ' 
    '         Properties: CanRead, CanWrite, KeyType, ValueType

    '     Class TsMethod
    ' 
    '         Properties: GenericArguments, Info, IsStatic, Name, Parameters, ReturnType

    '     Class TsParameter
    ' 
    '         Properties: DefaultValue, HasDefault, IsByRef, IsOptional, IsParamArray, Name, Type

    '     Class TsProperty
    ' 
    '         Properties: CanRead, CanWrite, Info, IsStatic, Name, PropertyType

    '     Class TsType
    ' 
    '         Properties: ClrType, Constructors, EnumMembers, Fields, GenericParameters
    '                     Interfaces, Invoke, IsAbstract, IsObsolete, IsSealed, Kind
    '                     Methods, Nested, Parent, Properties, ShortName, TsNamespace

    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace TypeDefinitions

    ''' <summary>The TypeScript declaration shape of a CLR type.</summary>
    Public Enum TsTypeKind
        ''' <summary>class (includes sealed / MustInherit classes).</summary>
        ClassType
        ''' <summary>interface.</summary>
        InterfaceType
        ''' <summary>enum.</summary>
        EnumType
        ''' <summary>structure (ValueType).</summary>
        StructType
        ''' <summary>VB Module / C# static class (abstract + sealed).</summary>
        ModuleType
        ''' <summary>delegate (emitted as a function type).</summary>
        DelegateType
    End Enum

    ''' <summary>One parameter of a method or constructor.</summary>
    Public Class TsParameter

        ''' <summary>The CLR parameter name.</summary>
        Public Property Name As String
        ''' <summary>The declared parameter type (ByRef types keep their <c>&amp;</c> here).</summary>
        Public Property Type As Type
        ''' <summary>Optional parameter (has a default value slot).</summary>
        Public Property IsOptional As Boolean
        ''' <summary><c>ParamArray</c> tail — emitted as a TypeScript rest parameter.</summary>
        Public Property IsParamArray As Boolean
        ''' <summary>ByRef / out / ref parameter.</summary>
        Public Property IsByRef As Boolean
        ''' <summary>Whether <see cref="DefaultValue"/> carries a value.</summary>
        Public Property HasDefault As Boolean
        ''' <summary>The declared default value, if any.</summary>
        Public Property DefaultValue As Object

    End Class

    ''' <summary>A readable/writable property of a scanned type.</summary>
    Public Class TsProperty

        Public Property Name As String
        Public Property PropertyType As Type
        Public Property CanRead As Boolean
        Public Property CanWrite As Boolean
        Public Property IsStatic As Boolean
        ''' <summary>The reflected property, used for XML doc lookup.</summary>
        Public Property Info As PropertyInfo

    End Class

    ''' <summary>A public field of a scanned type.</summary>
    Public Class TsField

        Public Property Name As String
        Public Property FieldType As Type
        Public Property IsStatic As Boolean
        ''' <summary><c>readonly</c> (InitOnly).</summary>
        Public Property IsReadOnly As Boolean
        ''' <summary><c>const</c> (literal).</summary>
        Public Property IsLiteral As Boolean
        Public Property Info As FieldInfo

    End Class

    ''' <summary>One method overload.</summary>
    Public Class TsMethod

        Public Property Name As String
        Public Property ReturnType As Type
        Public Property Parameters As List(Of TsParameter)
        Public Property IsStatic As Boolean
        ''' <summary>Method-level generic parameter names, Nothing when not generic.</summary>
        Public Property GenericArguments As List(Of String)
        Public Property Info As MethodInfo

    End Class

    ''' <summary>One public constructor overload.</summary>
    Public Class TsConstructor

        Public Property Parameters As List(Of TsParameter)
        Public Property Info As ConstructorInfo

    End Class

    ''' <summary>A single-parameter indexer (the only form the bridge supports).</summary>
    Public Class TsIndexer

        Public Property KeyType As Type
        Public Property ValueType As Type
        Public Property CanRead As Boolean
        Public Property CanWrite As Boolean

    End Class

    ''' <summary>The reflection model of one exported CLR type.</summary>
    Public Class TsType

        Public Property ClrType As Type
        ''' <summary>The CLR namespace ("" for the global namespace).</summary>
        Public Property TsNamespace As String
        ''' <summary>The type name with the generic arity suffix stripped.</summary>
        Public Property ShortName As String
        Public Property Kind As TsTypeKind
        ''' <summary>Generic parameter names of the type itself.</summary>
        Public Property GenericParameters As List(Of String)
        ''' <summary>Base class when it is exported by the same assembly.</summary>
        Public Property BaseType As Type
        ''' <summary>Implemented interfaces (inherited ones filtered out).</summary>
        Public Property Interfaces As List(Of Type)
        Public Property IsAbstract As Boolean
        Public Property IsSealed As Boolean
        Public Property IsObsolete As Boolean

        Public Property Properties As New List(Of TsProperty)
        Public Property Fields As New List(Of TsField)
        Public Property Methods As New List(Of TsMethod)
        Public Property Constructors As New List(Of TsConstructor)
        Public Property Indexers As New List(Of TsIndexer)

        ''' <summary>Enum name/value pairs (Kind = EnumType only).</summary>
        Public Property EnumMembers As List(Of KeyValuePair(Of String, Long))
        ''' <summary>The <c>Invoke</c> signature of a delegate type.</summary>
        Public Property Invoke As TsMethod

        ''' <summary>The declaring type for nested types, Nothing for top-level types.</summary>
        Public Property Parent As TsType
        ''' <summary>Directly nested exported types.</summary>
        Public Property Nested As New List(Of TsType)

        ''' <summary>Total number of emitted members (for CLI statistics).</summary>
        Public ReadOnly Property MemberCount As Integer
            Get
                Return Properties.Count + Fields.Count + Methods.Count +
                       Constructors.Count + Indexers.Count +
                       If(EnumMembers Is Nothing, 0, EnumMembers.Count)
            End Get
        End Property

    End Class

End Namespace

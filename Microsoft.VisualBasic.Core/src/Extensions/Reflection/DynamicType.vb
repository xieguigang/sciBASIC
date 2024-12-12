#Region "Microsoft.VisualBasic::48a39c10a84cbbb59fc115151a210e2a, Microsoft.VisualBasic.Core\src\Extensions\Reflection\DynamicType.vb"

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

    '   Total Lines: 238
    '    Code Lines: 156 (65.55%)
    ' Comment Lines: 44 (18.49%)
    '    - Xml Docs: 97.73%
    ' 
    '   Blank Lines: 38 (15.97%)
    '     File Size: 9.58 KB


    ' Class DynamicType
    ' 
    '     Properties: GeneratedType
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Add, (+2 Overloads) Create, CreateValidSymbolName, GetTypeBuilder
    ' 
    '     Sub: AddDescription, AddDisplayName, AddProperty
    '     Structure PropertyInfo
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.My.JavaScript

''' <summary>
''' Build dynamics clr runtime type
''' </summary>
''' <remarks>
''' https://blog.wedport.co.uk/2020/06/10/generating-c-net-core-classes-at-runtime/
''' </remarks>
Public Class DynamicType

    Public ReadOnly Property GeneratedType As Type

    ReadOnly inheritsFrom As Type
    ReadOnly properties As New List(Of PropertyInfo)

    Public Structure PropertyInfo

        ''' <summary>
        ''' the property name
        ''' </summary>
        Dim Name As String
        ''' <summary>
        ''' the property data type
        ''' </summary>
        Dim PropertyType As Type
        ''' <summary>
        ''' [optional] the description text for tagged the <see cref="DescriptionAttribute"/> value to current property.
        ''' </summary>
        Dim Description As String
        ''' <summary>
        ''' [optional] the name display string for tagged the <see cref="DisplayNameAttribute"/> value to current property.
        ''' </summary>
        Dim DisplayName As String

        Sub New(name As String, type As Type)
            Me.Name = name
            Me.PropertyType = type
        End Sub

        Public Overrides Function ToString() As String
            Return $"Public Property {Name} As {PropertyType.ToString}"
        End Function

    End Structure

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(ParamArray properties As PropertyInfo())
        Me.properties = New List(Of PropertyInfo)(properties)
    End Sub

    Public Function Add(name As String, type As Type,
                        Optional description As String = Nothing,
                        Optional displayName As String = Nothing) As DynamicType

        Call properties.Add(
            New PropertyInfo(name, type) With {
                .Description = description,
                .DisplayName = displayName
            }
        )

        Return Me
    End Function

    Const flag As TypeAttributes = TypeAttributes.Public Or
                                   TypeAttributes.Class Or
                                   TypeAttributes.AutoClass Or
                                   TypeAttributes.AnsiClass Or
                                   TypeAttributes.BeforeFieldInit Or
                                   TypeAttributes.AutoLayout

    Public Shared Function GetTypeBuilder(Optional name As String = Nothing,
                                          Optional inheritsFrom As Type = Nothing,
                                          Optional isAbstract As Boolean = False,
                                          Optional sealed As Boolean = False) As TypeBuilder

        Dim newTypeName As String = Guid.NewGuid.ToString
        Dim assemblyName = New AssemblyName(newTypeName)
        Dim dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
        Dim dynamicModule = dynamicAssembly.DefineDynamicModule("Main")
        Dim flag As TypeAttributes = DynamicType.flag

        If isAbstract Then
            flag = flag Or TypeAttributes.Abstract
        End If
        If sealed Then
            flag = flag Or TypeAttributes.Sealed
        End If

        Return dynamicModule.DefineType(If(name, newTypeName), flag, inheritsFrom)
    End Function

    Public Function Create() As DynamicType
        Dim dynamicType As TypeBuilder = GetTypeBuilder(Nothing, inheritsFrom)

        Call dynamicType.DefineDefaultConstructor(MethodAttributes.Public Or
                                                  MethodAttributes.SpecialName Or
                                                  MethodAttributes.RTSpecialName)

        For Each [property] As PropertyInfo In properties
            Call AddProperty(dynamicType, [property].Name, [property].PropertyType,
                             desc:=[property].Description,
                             display:=[property].DisplayName)
        Next

        _GeneratedType = dynamicType.CreateType()

        Return Me
    End Function

    Private Shared Sub AddProperty(typeBuilder As TypeBuilder, propertyName As String, propertyType As Type, desc As String, display As String)
        Dim fieldBuilder = typeBuilder.DefineField("_" & propertyName, propertyType, FieldAttributes.Private)
        Dim propertyBuilder As PropertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null)
        Dim getMethod = typeBuilder.DefineMethod("get_" & propertyName,
            MethodAttributes.Public Or
            MethodAttributes.SpecialName Or
            MethodAttributes.HideBySig, propertyType, Type.EmptyTypes)
        Dim getMethodIL = getMethod.GetILGenerator()
        getMethodIL.Emit(OpCodes.Ldarg_0)
        getMethodIL.Emit(OpCodes.Ldfld, fieldBuilder)
        getMethodIL.Emit(OpCodes.Ret)
        Dim setMethod = typeBuilder.DefineMethod("set_" & propertyName,
            MethodAttributes.Public Or
            MethodAttributes.SpecialName Or
            MethodAttributes.HideBySig,
            null, New Type() {propertyType})
        Dim setMethodIL = setMethod.GetILGenerator()
        Dim modifyProperty = setMethodIL.DefineLabel()
        Dim exitSet = setMethodIL.DefineLabel()

        setMethodIL.MarkLabel(modifyProperty)
        setMethodIL.Emit(OpCodes.Ldarg_0)
        setMethodIL.Emit(OpCodes.Ldarg_1)
        setMethodIL.Emit(OpCodes.Stfld, fieldBuilder)
        setMethodIL.Emit(OpCodes.Nop)
        setMethodIL.MarkLabel(exitSet)
        setMethodIL.Emit(OpCodes.Ret)

        If Not desc.StringEmpty Then
            Call AddDescription(propertyBuilder, desc)
        End If
        If Not display.StringEmpty Then
            Call AddDisplayName(propertyBuilder, display)
        End If

        Call propertyBuilder.SetGetMethod(getMethod)
        Call propertyBuilder.SetSetMethod(setMethod)
    End Sub

    ''' <summary>
    ''' add <see cref="DisplayNameAttribute"/>
    ''' </summary>
    ''' <param name="propertyBuilder"></param>
    ''' <param name="display"></param>
    Private Shared Sub AddDisplayName(propertyBuilder As PropertyBuilder, display As String)
        Dim ctorSig = New Type() {GetType(String)}
        Dim classInfo As ConstructorInfo = GetType(DisplayNameAttribute).GetConstructor(ctorSig)
        Dim attr As New CustomAttributeBuilder(classInfo, New Object() {display})

        Call propertyBuilder.SetCustomAttribute(attr)
    End Sub

    ''' <summary>
    ''' add <see cref="DescriptionAttribute"/>
    ''' </summary>
    ''' <param name="propertyBuilder"></param>
    ''' <param name="desc"></param>
    Private Shared Sub AddDescription(propertyBuilder As PropertyBuilder, desc As String)
        Dim ctorSig = New Type() {GetType(String)}
        Dim classInfo As ConstructorInfo = GetType(DescriptionAttribute).GetConstructor(ctorSig)
        Dim attr As New CustomAttributeBuilder(classInfo, New Object() {desc})

        Call propertyBuilder.SetCustomAttribute(attr)
    End Sub

    ''' <summary>
    ''' A helper function for create a valid symbol name
    ''' </summary>
    ''' <param name="key"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function CreateValidSymbolName(key As String) As String
        Return key.NormalizePathString().StringReplace("\s+", "_")
    End Function

    Public Shared Function CreateEnum(members As Dictionary(Of String, String), Optional typeName As String = "DynamicEnumType") As Type
        Dim assemblyName As New AssemblyName(Guid.NewGuid.ToString)
        Dim assemblyBuilder As AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
        Dim moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule")
        Dim enumBuilder = moduleBuilder.DefineEnum(typeName, TypeAttributes.Public, GetType(Integer))
        Dim value As i32 = 0

        For Each member As KeyValuePair(Of String, String) In members
            Dim fieldBuilder = enumBuilder.DefineLiteral(member.Key, ++value)
            Dim constructor = GetType(DescriptionAttribute).GetConstructor(New Type() {GetType(String)})
            Dim attributeBuilder As New CustomAttributeBuilder(constructor, New Object() {member.Value})

            Call fieldBuilder.SetCustomAttribute(attributeBuilder)
        Next

        Dim enumType As Type = enumBuilder.CreateType()
        Return enumType
    End Function

    ''' <summary>
    ''' Create dynamics object in debug view
    ''' </summary>
    ''' <param name="metadata"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' the type object that generated at here has different class guid, for create array of the dynamics type object, 
    ''' you should create the template dynamics clr type at first, and then set property value for each data in the 
    ''' array seperately.
    ''' </remarks>
    Public Shared Function Create(metadata As Dictionary(Of String, Object)) As Object
        Dim properties As New List(Of PropertyInfo)
        Dim normalized As New List(Of KeyValuePair(Of String, Object))(capacity:=metadata.Count)

        For Each meta As KeyValuePair(Of String, Object) In metadata
            Dim symbol As String = CreateValidSymbolName(meta.Key)
            Dim type As Type
            Dim value As Object = meta.Value

            If value Is Nothing Then
                type = GetType(String)
                value = "NULL"
            Else
                type = value.GetType
            End If

            Call properties.Add(New PropertyInfo With {
                .Name = symbol,
                .PropertyType = type,
                .Description = "",
                .DisplayName = meta.Key
            })
            Call normalized.Add(New KeyValuePair(Of String, Object)(symbol, meta.Value))
        Next

        Return JavaScriptObject.CreateDynamicObject(New DynamicType(properties.ToArray).Create, normalized)
    End Function

    Public Shared Narrowing Operator CType(dynamic As DynamicType) As Type
        If dynamic.GeneratedType Is Nothing Then
            Call dynamic.Create()
        End If

        Return dynamic.GeneratedType
    End Operator
End Class

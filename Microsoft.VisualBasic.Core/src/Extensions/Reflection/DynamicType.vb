#Region "Microsoft.VisualBasic::c26ebb2b2a87d5ecb1466044a688379e, Microsoft.VisualBasic.Core\src\Extensions\Reflection\DynamicType.vb"

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

    '   Total Lines: 167
    '    Code Lines: 130
    ' Comment Lines: 6
    '   Blank Lines: 31
    '     File Size: 6.83 KB


    ' Class DynamicType
    ' 
    '     Properties: GeneratedType
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: (+2 Overloads) Create, GetTypeBuilder
    ' 
    '     Sub: AddDescription, AddDisplayName, AddProperty
    '     Structure PropertyInfo
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Reflection.Emit
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' 
''' </summary>
''' <remarks>
''' https://blog.wedport.co.uk/2020/06/10/generating-c-net-core-classes-at-runtime/
''' </remarks>
Public Class DynamicType

    Public ReadOnly Property GeneratedType As Type

    ReadOnly inheritsFrom As Type
    ReadOnly properties As PropertyInfo()

    Public Structure PropertyInfo
        Dim Name As String
        Dim PropertyType As Type
        Dim Description As String
        Dim DisplayName As String
    End Structure

    Sub New(ParamArray properties As PropertyInfo())
        Me.properties = properties
    End Sub

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

    Private Shared Sub AddDisplayName(propertyBuilder As PropertyBuilder, display As String)
        Dim ctorSig = New Type() {GetType(String)}
        Dim classInfo As ConstructorInfo = GetType(DisplayNameAttribute).GetConstructor(ctorSig)
        Dim attr As New CustomAttributeBuilder(classInfo, New Object() {display})

        Call propertyBuilder.SetCustomAttribute(attr)
    End Sub

    Private Shared Sub AddDescription(propertyBuilder As PropertyBuilder, desc As String)
        Dim ctorSig = New Type() {GetType(String)}
        Dim classInfo As ConstructorInfo = GetType(DescriptionAttribute).GetConstructor(ctorSig)
        Dim attr As New CustomAttributeBuilder(classInfo, New Object() {desc})

        Call propertyBuilder.SetCustomAttribute(attr)
    End Sub

    Public Shared Function Create(metadata As Dictionary(Of String, Object)) As Object
        Dim properties As New List(Of PropertyInfo)

        For Each meta In metadata
            Dim symbol As String = meta.Key.NormalizePathString().StringReplace("\s+", "_")
            Dim type As Type
            Dim value As Object = meta.Value

            If value Is Nothing Then
                type = GetType(String)
                value = "NULL"
            Else
                type = value.GetType
            End If

            properties.Add(New PropertyInfo With {
                .Name = symbol,
                .PropertyType = type,
                .Description = "",
                .DisplayName = meta.Key
            })
        Next

        Dim obj As Object = New DynamicType(properties.ToArray).Create.GeneratedType.DoCall(AddressOf Activator.CreateInstance)
        Dim schema = DataFramework.Schema(obj.GetType, flag:=PropertyAccess.Writeable, nonIndex:=True)

        For Each meta In properties
            Dim value As Object = metadata(meta.DisplayName)
            Dim prop = schema(meta.Name)

            Call prop.SetValue(obj, value)
        Next

        Return obj
    End Function
End Class

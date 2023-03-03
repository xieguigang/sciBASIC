Imports System.Reflection
Imports System.Reflection.Emit
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

''' <summary>
''' 
''' </summary>
''' <remarks>
''' https://blog.wedport.co.uk/2020/06/10/generating-c-net-core-classes-at-runtime/
''' </remarks>
Public Class DynamicType

    Public ReadOnly Property GeneratedType As Type

    ReadOnly inheritsFrom As Type
    ReadOnly properties As NamedValue(Of Type)()

    Sub New(ParamArray properties As NamedValue(Of Type)())
        Me.properties = properties
    End Sub

    Const flag As TypeAttributes = TypeAttributes.Public Or
                                   TypeAttributes.Class Or
                                   TypeAttributes.AutoClass Or
                                   TypeAttributes.AnsiClass Or
                                   TypeAttributes.BeforeFieldInit Or
                                   TypeAttributes.AutoLayout

    Public Function Create() As DynamicType
        Dim newTypeName As String = Guid.NewGuid.ToString
        Dim assemblyName = New AssemblyName(newTypeName)
        Dim dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
        Dim dynamicModule = dynamicAssembly.DefineDynamicModule("Main")
        Dim dynamicType = dynamicModule.DefineType(newTypeName, flag, inheritsFrom)

        Call dynamicType.DefineDefaultConstructor(MethodAttributes.Public Or
                                                  MethodAttributes.SpecialName Or
                                                  MethodAttributes.RTSpecialName)

        For Each [property] As NamedValue(Of Type) In properties
            Call AddProperty(dynamicType, [property].Name, [property].Value)
        Next

        _GeneratedType = dynamicType.CreateType()
    End Function

    Private Shared Sub AddProperty(typeBuilder As TypeBuilder, propertyName As String, propertyType As Type)
        Dim fieldBuilder = typeBuilder.DefineField("_" & propertyName, propertyType, FieldAttributes.Private)
        Dim propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null)
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
          null, New Object() {propertyType})
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

        propertyBuilder.SetGetMethod(getMethod)
        propertyBuilder.SetSetMethod(setMethod)
    End Sub
End Class

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

    Public Function Create() As DynamicType
        Dim newTypeName As String = Guid.NewGuid.ToString
        Dim assemblyName = New AssemblyName(newTypeName)
        Dim dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
        Dim dynamicModule = dynamicAssembly.DefineDynamicModule("Main")
        Dim dynamicType = dynamicModule.DefineType(newTypeName, flag, inheritsFrom)

        Call dynamicType.DefineDefaultConstructor(MethodAttributes.Public Or
                                                  MethodAttributes.SpecialName Or
                                                  MethodAttributes.RTSpecialName)

        For Each [property] As PropertyInfo In properties
            Call AddProperty(dynamicType, [property].Name, [property].PropertyType,
                             desc:=[property].Description,
                             display:=[property].DisplayName)
        Next

        _GeneratedType = dynamicType.CreateType()
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
            Dim symbol As String = meta.Key.NormalizePathString()
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

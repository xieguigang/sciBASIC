Imports System
Imports System.Linq
Imports System.Reflection.Emit
Imports System.Reflection

Namespace FeatherDotNet.Impl
    Friend NotInheritable Class ValueCaster(Of T)
        Private Shared ReadOnly [Delegate] As Func(Of Value, CategoryEnumMapType, T)

        Shared Sub New()
            Dim dyn = New DynamicMethod("Cast_Value_To_" & GetType(T).Name, GetType(T), {GetType(Value), GetType(CategoryEnumMapType)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            Dim needsCategoryList As Boolean

            If GetType(T) Is GetType(Value) Then

                il.Emit(OpCodes.Ldarg_0)                 ' Value
                il.Emit(OpCodes.Ret)                     ' --empty--
            Else
                Dim tIsEnum = GetType(T).IsEnum
                Dim tIsNullableEnum = If(Nullable.GetUnderlyingType(GetType(T))?.IsEnum, False)

                Dim conversionStaticMethod As MethodInfo

                If tIsEnum Then
                    Dim convertEnumGen = GetType(Value).GetMethod("ConvertEnum", BindingFlags.NonPublic Or BindingFlags.Static)
                    Dim convertEnum = convertEnumGen.MakeGenericMethod(GetType(T))

                    needsCategoryList = True
                    conversionStaticMethod = convertEnum
                Else
                    If tIsNullableEnum Then
                        Dim enumType = Nullable.GetUnderlyingType(GetType(T))
                        Dim convertNullableEnumGen = GetType(Value).GetMethod("ConvertNullableEnum", BindingFlags.NonPublic Or BindingFlags.Static)
                        Dim convertNullableEnum = convertNullableEnumGen.MakeGenericMethod(enumType)

                        needsCategoryList = True
                        conversionStaticMethod = convertNullableEnum
                    Else
                        needsCategoryList = False
                        conversionStaticMethod = GetType(Value).GetMethods(BindingFlags.Static Or BindingFlags.Public).Where(Function(mtd) Equals(mtd.Name, "op_Implicit") AndAlso mtd.ReturnType Is GetType(T)).[Single]()
                    End If
                End If

                il.Emit(OpCodes.Ldarg_0)                        ' Value

                If needsCategoryList Then
                    il.Emit(OpCodes.Ldarg_1)                    ' Value CategoryEnumMapType
                End If

                il.Emit(OpCodes.Call, conversionStaticMethod)   ' T
                il.Emit(OpCodes.Ret)                            ' --empty--
            End If

            [Delegate] = CType(dyn.CreateDelegate(GetType(Func(Of Value, CategoryEnumMapType, T))), Func(Of Value, CategoryEnumMapType, T))
        End Sub

        Friend Shared Function Cast(value As Value, enumMapConfig As CategoryEnumMapType) As T
            Return [Delegate](value, enumMapConfig)
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::10ae7a4aa78e92a49cff836812e0305c, Data\BinaryData\Feather\Impl\ValueCaster.vb"

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

    '   Total Lines: 63
    '    Code Lines: 49
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 2.94 KB


    '     Class ValueCaster
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Cast
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Linq
Imports System.Reflection.Emit
Imports System.Reflection

Namespace Impl
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


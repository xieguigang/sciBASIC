Imports System
Imports System.Reflection.Emit

Namespace FeatherDotNet.Impl
    Friend NotInheritable Class EnumMapper(Of T)
        Private Shared ReadOnly [Delegate] As Func(Of Integer, T)
        Shared Sub New()
            Dim dyn = New DynamicMethod("Map_Enum_" & GetType(T).Name, GetType(T), {GetType(Integer)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            Dim underlying = [Enum].GetUnderlyingType(GetType(T))

            il.Emit(OpCodes.Ldarg_0)            ' ---int---
            If underlying Is GetType(Byte) Then
                il.Emit(OpCodes.Conv_U1)
            Else
                If underlying Is GetType(SByte) Then
                    il.Emit(OpCodes.Conv_I1)
                Else
                    If underlying Is GetType(Short) Then
                        il.Emit(OpCodes.Conv_I2)
                    Else
                        If underlying Is GetType(UShort) Then
                            il.Emit(OpCodes.Conv_U2)
                        Else
                            ' Nothing to be done
                            If underlying Is GetType(Integer) Then
                            Else
                                If underlying Is GetType(UInteger) Then
                                    il.Emit(OpCodes.Conv_U4)
                                Else
                                    If underlying Is GetType(Long) Then
                                        il.Emit(OpCodes.Conv_I8)
                                    Else
                                        If underlying Is GetType(ULong) Then
                                            il.Emit(OpCodes.Conv_U8)
                                        Else
                                            Throw New InvalidOperationException($"""Impossible"" (don't get cute) enum found, underlying type isn't an integral type {GetType(T).Name}")
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If

            il.Emit(OpCodes.Ret)

            [Delegate] = CType(dyn.CreateDelegate(GetType(Func(Of Integer, T))), Func(Of Integer, T))
        End Sub

        Public Shared Function Map(underlying As Integer) As T
            Return [Delegate](underlying)
        End Function
    End Class
End Namespace

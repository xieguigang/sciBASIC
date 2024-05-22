#Region "Microsoft.VisualBasic::d99a17e89261a847799e906959bdebb9, Data\BinaryData\Feather\Impl\EnumMapper.vb"

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

    '   Total Lines: 58
    '    Code Lines: 50 (86.21%)
    ' Comment Lines: 1 (1.72%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (12.07%)
    '     File Size: 2.51 KB


    '     Class EnumMapper
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Map
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Reflection.Emit

Namespace Impl

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

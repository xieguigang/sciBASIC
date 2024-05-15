#Region "Microsoft.VisualBasic::9524e6ad4bc151e89905e83dbfcff0a6, Data\BinaryData\Feather\Impl\EnumDetails.vb"

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

    '   Total Lines: 118
    '    Code Lines: 96
    ' Comment Lines: 3
    '   Blank Lines: 19
    '     File Size: 5.34 KB


    '     Module EnumDetails
    ' 
    '         Function: GetConvertToLongDelegate, GetLevelIndexLookup, GetLevels, LoadLevels, MakeConvertToLongDelegate
    '                   MakeLevelLookup
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection.Emit

Namespace Impl
    Friend Module EnumDetails
        Private ReadOnly LevelsLookup As Dictionary(Of Type, String()) = New Dictionary(Of Type, String())()
        Private ReadOnly LevelIndexLookupLookup As Dictionary(Of Type, Dictionary(Of Long, Integer)) = New Dictionary(Of Type, Dictionary(Of Long, Integer))()
        Private ReadOnly ConversionLookup As Dictionary(Of Type, [Delegate]) = New Dictionary(Of Type, [Delegate])()

        Public Function GetLevels(enumType As Type) As String()
            ' should be a low contention lock
            SyncLock LevelsLookup
                Dim levels As String() = Nothing
                If Not LevelsLookup.TryGetValue(enumType, levels) Then
                    levels = LoadLevels(enumType)
                    LevelsLookup(enumType) = levels
                End If

                Return levels
            End SyncLock
        End Function

        Public Function GetLevelIndexLookup(enumType As Type) As Dictionary(Of Long, Integer)
            ' should be a low contention lock
            SyncLock LevelIndexLookupLookup
                Dim levelIndexLookup As Dictionary(Of Long, Integer) = Nothing
                If Not LevelIndexLookupLookup.TryGetValue(enumType, levelIndexLookup) Then
                    levelIndexLookup = MakeLevelLookup(enumType)
                    LevelIndexLookupLookup(enumType) = levelIndexLookup
                End If

                Return levelIndexLookup
            End SyncLock
        End Function

        Public Function GetConvertToLongDelegate(Of TEnum As Structure)() As Func(Of TEnum, Long)
            ' should be a low contention lock
            SyncLock ConversionLookup
                Dim convert As [Delegate] = Nothing
                If Not ConversionLookup.TryGetValue(GetType(TEnum), convert) Then
                    convert = MakeConvertToLongDelegate(Of TEnum)()
                    ConversionLookup(GetType(TEnum)) = convert
                End If

                Return CType(convert, Func(Of TEnum, Long))
            End SyncLock
        End Function

        Private Function LoadLevels(enumType As Type) As String()
            enumType = If(Nullable.GetUnderlyingType(enumType), enumType)

            Return [Enum].GetNames(enumType).OrderBy(Function(__) __).ThenBy(Function(__) [Enum].Parse(enumType, __)).ToArray()
        End Function

        Private Function MakeLevelLookup(enumType As Type) As Dictionary(Of Long, Integer)
            Dim underlyingType = [Enum].GetUnderlyingType(enumType)
            Dim makeLong As Func(Of Object, Long)
            If underlyingType Is GetType(Byte) Then
                makeLong = Function(o) CByte(o)
            Else
                If underlyingType Is GetType(SByte) Then
                    makeLong = Function(o) CSByte(o)
                Else
                    If underlyingType Is GetType(Short) Then
                        makeLong = Function(o) CShort(o)
                    Else
                        If underlyingType Is GetType(UShort) Then
                            makeLong = Function(o) CUShort(o)
                        Else
                            If underlyingType Is GetType(Integer) Then
                                makeLong = Function(o) CInt(o)
                            Else
                                If underlyingType Is GetType(UInteger) Then
                                    makeLong = Function(o) CUInt(o)
                                Else
                                    If underlyingType Is GetType(Long) Then
                                        makeLong = Function(o) o
                                    Else
                                        If underlyingType Is GetType(ULong) Then
                                            makeLong = Function(o) CULng(o)
                                        Else
                                            Throw New InvalidOperationException($"Unexpected type underlying an enum {underlyingType.Name}")
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If


            Dim keys = LoadLevels(enumType)

            Dim ret = New Dictionary(Of Long, Integer)()

            For i = 0 To keys.Length - 1
                Dim key = keys(i)
                Dim val = [Enum].Parse(enumType, key)
                Dim asLong = makeLong(val)

                ret(asLong) = i
            Next

            Return ret
        End Function

        Private Function MakeConvertToLongDelegate(Of TEnum As Structure)() As Func(Of TEnum, Long)
            Dim dyn = New DynamicMethod("Convert_" & GetType(TEnum).Name & "_ToLong", GetType(Long), {GetType(TEnum)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            il.Emit(OpCodes.Ldarg_0)
            il.Emit(OpCodes.Conv_I8)
            il.Emit(OpCodes.Ret)

            Return CType(dyn.CreateDelegate(GetType(Func(Of TEnum, Long))), Func(Of TEnum, Long))
        End Function
    End Module
End Namespace


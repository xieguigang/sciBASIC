Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Reflection.Emit

Namespace FeatherDotNet.Impl
    Friend Module DataWidener
        Private ReadOnly WidenerLookup As Dictionary(Of Type, Func(Of IEnumerable, IEnumerable)) = New Dictionary(Of Type, Func(Of IEnumerable, IEnumerable)) From {
{GetType(String), Function(d) WidenToString(d)},
{GetType(Double), Function(d) WidenToDouble(d)},
{GetType(Double?), Function(d) WidenToNullableDouble(d)},
{GetType(Single), Function(d) WidenToFloat(d)},
{GetType(Single?), Function(d) WidenToNullableFloat(d)},
{GetType(Long), Function(d) WidenToLong(d)},
{GetType(Long?), Function(d) WidenToNullableLong(d)},
{GetType(ULong), Function(d) WidenToULong(d)},
{GetType(ULong?), Function(d) WidenToNullableULong(d)},
{GetType(Integer), Function(d) WidenToInt(d)},
{GetType(Integer?), Function(d) WidenToNullableInt(d)},
{GetType(UInteger), Function(d) WidenToUInt(d)},
{GetType(UInteger?), Function(d) WidenToNullableUInt(d)},
{GetType(Short), Function(d) WidenToShort(d)},
{GetType(Short?), Function(d) WidenToNullableShort(d)},
{GetType(UShort), Function(d) WidenToUShort(d)},
{GetType(UShort?), Function(d) WidenToNullableUShort(d)},
{GetType(SByte), Function(d) WidenToSByte(d)},
{GetType(SByte?), Function(d) WidenToNullableSByte(d)},
{GetType(Byte), Function(d) WidenToByte(d)},
{GetType(Byte?), Function(d) WidenToNullableByte(d)},
{GetType(Date), Function(d) WidenToDateTime(d)},
{GetType(Date?), Function(d) WidenToNullableDateTime(d)},
{GetType(DateTimeOffset), Function(d) WidenToDateTime(d)},
{GetType(DateTimeOffset?), Function(d) WidenToNullableDateTime(d)},
{GetType(TimeSpan), Function(d) WidenToTimeSpan(d)},
{GetType(TimeSpan?), Function(d) WidenToNullableTimeSpan(d)}
}

        Public Function GetWidener(widestType As Type) As Func(Of IEnumerable, IEnumerable)
            Dim nonNull = If(Nullable.GetUnderlyingType(widestType), widestType)

            Dim widener As Func(Of IEnumerable, IEnumerable)
            If Not WidenerLookup.TryGetValue(widestType, widener) Then
                If nonNull.IsEnum Then
                    Return GetEnumWidener(widestType)
                End If

                Throw New InvalidOperationException($"No widener available for {widestType.Name}")
            End If

            Return widener
        End Function

        Private ReadOnly WidenToEnumLookup As Dictionary(Of Type, Func(Of IEnumerable, IEnumerable)) = New Dictionary(Of Type, Func(Of IEnumerable, IEnumerable))()
        Private ReadOnly WidenToNullableEnumLookup As Dictionary(Of Type, Func(Of IEnumerable, IEnumerable)) = New Dictionary(Of Type, Func(Of IEnumerable, IEnumerable))()

        Private Function GetEnumWidener(widestType As Type) As Func(Of IEnumerable, IEnumerable)
            Dim isNullable = Nullable.GetUnderlyingType(widestType) IsNot Nothing
            Dim nonNullable = If(Nullable.GetUnderlyingType(widestType), widestType)

            If IsSynthetic(nonNullable) Then
                Return GetSyntheticEnumWidener(widestType)
            End If

            If isNullable Then
                ' assumed to be a low contention lock
                SyncLock WidenToNullableEnumLookup
                    Dim ret As Func(Of IEnumerable, IEnumerable)
                    If WidenToNullableEnumLookup.TryGetValue(nonNullable, ret) Then Return ret
                    ret = CreateNullableEnumWidener(nonNullable)
                    WidenToNullableEnumLookup(nonNullable) = ret

                    Return ret
                End SyncLock
            Else
                ' assumed to be a low contention lock
                SyncLock WidenToEnumLookup
                    Dim ret As Func(Of IEnumerable, IEnumerable)
                    If WidenToEnumLookup.TryGetValue(nonNullable, ret) Then Return ret
                    ret = CreateEnumWidener(nonNullable)
                    WidenToEnumLookup(nonNullable) = ret

                    Return ret
                End SyncLock
            End If
        End Function

        Private ReadOnly WidenToSyntehticEnumLookup As Dictionary(Of Type, Func(Of IEnumerable, IEnumerable)) = New Dictionary(Of Type, Func(Of IEnumerable, IEnumerable))()
        Private ReadOnly WidenToNullableSyntheticEnumLookup As Dictionary(Of Type, Func(Of IEnumerable, IEnumerable)) = New Dictionary(Of Type, Func(Of IEnumerable, IEnumerable))()
        Private Function GetSyntheticEnumWidener(widestType As Type) As Func(Of IEnumerable, IEnumerable)
            Dim isNullable = Nullable.GetUnderlyingType(widestType) IsNot Nothing
            Dim nonNullable = If(Nullable.GetUnderlyingType(widestType), widestType)

            If isNullable Then
                ' assumed to be a low contention lock
                SyncLock WidenToNullableSyntheticEnumLookup
                    Dim ret As Func(Of IEnumerable, IEnumerable)
                    If WidenToNullableSyntheticEnumLookup.TryGetValue(nonNullable, ret) Then Return ret
                    ret = CreateNullableSyntheticEnumWidener(nonNullable)
                    WidenToNullableSyntheticEnumLookup(nonNullable) = ret

                    Return ret
                End SyncLock
            Else
                ' assumed to be a low contention lock
                SyncLock WidenToSyntehticEnumLookup
                    Dim ret As Func(Of IEnumerable, IEnumerable)
                    If WidenToSyntehticEnumLookup.TryGetValue(nonNullable, ret) Then Return ret
                    ret = CreateSyntheticEnumWidener(nonNullable)
                    WidenToSyntehticEnumLookup(nonNullable) = ret

                    Return ret
                End SyncLock
            End If
        End Function

        Private Function CreateSyntheticEnumWidener(enumType As Type) As Func(Of IEnumerable, IEnumerable)
            Dim dyn = New DynamicMethod(NameOf(CreateSyntheticEnumWidener) & "_" & enumType.FullName, GetType(IEnumerable), {GetType(IEnumerable)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            Dim widenerGen = GetType(DataWidener).GetMethod(NameOf(WidenToSyntheticEnum), BindingFlags.NonPublic Or BindingFlags.Static)
            Dim widener = widenerGen.MakeGenericMethod(enumType)

            il.Emit(OpCodes.Ldarg_0)           ' IEnumerable
            il.Emit(OpCodes.Call, widener)     ' IEnumerable<enumType>
            il.Emit(OpCodes.Ret)               ' --empty--

            Return CType(dyn.CreateDelegate(GetType(Func(Of IEnumerable, IEnumerable))), Func(Of IEnumerable, IEnumerable))
        End Function

        Private Function CreateNullableSyntheticEnumWidener(enumType As Type) As Func(Of IEnumerable, IEnumerable)
            Dim dyn = New DynamicMethod(NameOf(CreateNullableEnumWidener) & "_" & enumType.FullName, GetType(IEnumerable), {GetType(IEnumerable)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            Dim widenerGen = GetType(DataWidener).GetMethod(NameOf(WidenToNullableSyntheticEnum), BindingFlags.NonPublic Or BindingFlags.Static)
            Dim widener = widenerGen.MakeGenericMethod(enumType)

            il.Emit(OpCodes.Ldarg_0)           ' IEnumerable
            il.Emit(OpCodes.Call, widener)     ' IEnumerable<enumType?>
            il.Emit(OpCodes.Ret)               ' --empty--

            Return CType(dyn.CreateDelegate(GetType(Func(Of IEnumerable, IEnumerable))), Func(Of IEnumerable, IEnumerable))
        End Function

        Private Iterator Function WidenToSyntheticEnum(Of TSyntheticEnum As Structure)(untyped As IEnumerable) As IEnumerable(Of TSyntheticEnum)
            For Each Val As Object In untyped
                Dim asStr = Val.ToString()

                Yield [Enum].Parse(GetType(TSyntheticEnum), asStr, ignoreCase:=True)
            Next
        End Function

        Private Iterator Function WidenToNullableSyntheticEnum(Of TSyntheticEnum As Structure)(untyped As IEnumerable) As IEnumerable(Of TSyntheticEnum?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                Dim asStr = Val.ToString()
                Yield CType([Enum].Parse(GetType(TSyntheticEnum), asStr, ignoreCase:=True), TSyntheticEnum)
            Next
        End Function

        Private Function CreateEnumWidener(enumType As Type) As Func(Of IEnumerable, IEnumerable)
            Dim dyn = New DynamicMethod(NameOf(CreateEnumWidener) & "_" & enumType.FullName, GetType(IEnumerable), {GetType(IEnumerable)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            Dim widenerGen = GetType(DataWidener).GetMethod(NameOf(WidenToEnum), BindingFlags.NonPublic Or BindingFlags.Static)
            Dim widener = widenerGen.MakeGenericMethod(enumType)

            il.Emit(OpCodes.Ldarg_0)           ' IEnumerable
            il.Emit(OpCodes.Call, widener)     ' IEnumerable<enumType>
            il.Emit(OpCodes.Ret)               ' --empty--

            Return CType(dyn.CreateDelegate(GetType(Func(Of IEnumerable, IEnumerable))), Func(Of IEnumerable, IEnumerable))
        End Function

        Private Function CreateNullableEnumWidener(enumType As Type) As Func(Of IEnumerable, IEnumerable)
            Dim dyn = New DynamicMethod(NameOf(CreateNullableEnumWidener) & "_" & enumType.FullName, GetType(IEnumerable), {GetType(IEnumerable)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            Dim widenerGen = GetType(DataWidener).GetMethod(NameOf(WidenToNullableEnum), BindingFlags.NonPublic Or BindingFlags.Static)
            Dim widener = widenerGen.MakeGenericMethod(enumType)

            il.Emit(OpCodes.Ldarg_0)           ' IEnumerable
            il.Emit(OpCodes.Call, widener)     ' IEnumerable<enumType?>
            il.Emit(OpCodes.Ret)               ' --empty--

            Return CType(dyn.CreateDelegate(GetType(Func(Of IEnumerable, IEnumerable))), Func(Of IEnumerable, IEnumerable))
        End Function

        Private Iterator Function WidenToEnum(Of TEnum As Structure)(untyped As IEnumerable) As IEnumerable(Of TEnum)
            For Each Val As Object In untyped
                Yield [Enum].Parse(GetType(TEnum), [Enum].GetName(GetType(TEnum), Val))
            Next
        End Function

        Private Iterator Function WidenToNullableEnum(Of TEnum As Structure)(untyped As IEnumerable) As IEnumerable(Of TEnum?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                Yield CType([Enum].Parse(GetType(TEnum), [Enum].GetName(GetType(TEnum), Val)), TEnum)
            Next
        End Function

        Private Iterator Function WidenToString(untyped As IEnumerable) As IEnumerable(Of String)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is String Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Date Then
                    Yield CDate(Val).ToString("u")
                    Continue For
                End If

                If TypeOf Val Is Date? Then
                    Yield CType(Val, Date?).Value.ToString("u")
                    Continue For
                End If

                If TypeOf Val Is DateTimeOffset Then
                    Yield CType(Val, DateTimeOffset).ToString("u")
                    Continue For
                End If

                If TypeOf Val Is DateTimeOffset? Then
                    Yield CType(Val, DateTimeOffset?).Value.ToString("u")
                    Continue For
                End If

                If TypeOf Val Is TimeSpan Then
                    Yield CType(Val, TimeSpan).ToString("c")
                    Continue For
                End If

                If TypeOf Val Is TimeSpan? Then
                    Yield CType(Val, TimeSpan?).Value.ToString("c")
                    Continue For
                End If

                If TypeOf Val Is Double Then
                    Yield CDbl(Val).ToString("R")
                    Continue For
                End If

                If TypeOf Val Is Double? Then
                    Yield CType(Val, Double?).Value.ToString("R")
                    Continue For
                End If

                If TypeOf Val Is Single Then
                    Yield CSng(Val).ToString("R")
                    Continue For
                End If

                If TypeOf Val Is Single? Then
                    Yield CType(Val, Single?).Value.ToString("R")
                    Continue For
                End If

                If TypeOf Val Is Long Then
                    Yield CLng(Val).ToString()
                    Continue For
                End If

                If TypeOf Val Is Long? Then
                    Yield CType(Val, Long?).Value.ToString()
                    Continue For
                End If

                If TypeOf Val Is ULong Then
                    Yield CULng(Val).ToString()
                    Continue For
                End If

                If TypeOf Val Is ULong? Then
                    Yield CType(Val, ULong?).Value.ToString()
                    Continue For
                End If

                If TypeOf Val Is Integer Then
                    Yield CInt(Val).ToString()
                    Continue For
                End If

                If TypeOf Val Is Integer? Then
                    Yield CType(Val, Integer?).Value.ToString()
                    Continue For
                End If

                If TypeOf Val Is UInteger Then
                    Yield CUInt(Val).ToString()
                    Continue For
                End If

                If TypeOf Val Is UInteger? Then
                    Yield CType(Val, UInteger?).Value.ToString()
                    Continue For
                End If

                If TypeOf Val Is Short Then
                    Yield CShort(Val).ToString()
                    Continue For
                End If

                If TypeOf Val Is Short? Then
                    Yield CType(Val, Short?).Value.ToString()
                    Continue For
                End If

                If TypeOf Val Is UShort Then
                    Yield CUShort(Val).ToString()
                    Continue For
                End If

                If TypeOf Val Is UShort? Then
                    Yield CType(Val, UShort?).Value.ToString()
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val).ToString()
                    Continue For
                End If

                If TypeOf Val Is SByte? Then
                    Yield CType(Val, SByte?).Value.ToString()
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val).ToString()
                    Continue For
                End If

                If TypeOf Val Is Byte? Then
                    Yield CType(Val, Byte?).Value.ToString()
                    Continue For
                End If

                ' assuming this is an enum
                Yield Val.ToString()
            Next
        End Function

        Private Iterator Function WidenToTimeSpan(untyped As IEnumerable) As IEnumerable(Of TimeSpan)
            For Each Val As Object In untyped
                If TypeOf Val Is TimeSpan Then
                    Yield Val
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to TimeSpan")
            Next
        End Function

        Private Iterator Function WidenToNullableTimeSpan(untyped As IEnumerable) As IEnumerable(Of TimeSpan?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is TimeSpan Then
                    Yield CType(Val, TimeSpan)
                    Continue For
                End If

                If TypeOf Val Is TimeSpan? Then
                    Yield Val
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to TimeSpan")
            Next
        End Function

        Private Iterator Function WidenToDateTime(untyped As IEnumerable) As IEnumerable(Of Date)
            For Each Val As Object In untyped
                If TypeOf Val Is Date Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is DateTimeOffset Then
                    Yield CType(Val, DateTimeOffset).UtcDateTime
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to DateTime")
            Next
        End Function

        Private Iterator Function WidenToNullableDateTime(untyped As IEnumerable) As IEnumerable(Of Date?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is Date Then
                    Yield CDate(Val)
                    Continue For
                End If

                If TypeOf Val Is Date? Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is DateTimeOffset Then
                    Yield CType(Val, DateTimeOffset).UtcDateTime
                    Continue For
                End If

                If TypeOf Val Is DateTimeOffset? Then
                    Yield CType(Val, DateTimeOffset?)?.UtcDateTime
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to DateTime?")
            Next
        End Function

        Private Iterator Function WidenToDouble(untyped As IEnumerable) As IEnumerable(Of Double)
            For Each Val As Object In untyped
                If TypeOf Val Is Double Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Single Then
                    Yield CSng(Val)
                    Continue For
                End If

                If TypeOf Val Is Long Then
                    Yield CLng(Val)
                    Continue For
                End If

                If TypeOf Val Is ULong Then
                    Yield CULng(Val)
                    Continue For
                End If

                If TypeOf Val Is Integer Then
                    Yield CInt(Val)
                    Continue For
                End If

                If TypeOf Val Is UInteger Then
                    Yield CUInt(Val)
                    Continue For
                End If

                If TypeOf Val Is Short Then
                    Yield CShort(Val)
                    Continue For
                End If

                If TypeOf Val Is UShort Then
                    Yield CUShort(Val)
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to double")
            Next
        End Function

        Private Iterator Function WidenToNullableDouble(untyped As IEnumerable) As IEnumerable(Of Double?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is Double Then
                    Yield CDbl(Val)
                    Continue For
                End If

                If TypeOf Val Is Double? Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Single Then
                    Yield CSng(Val)
                    Continue For
                End If

                If TypeOf Val Is Single? Then
                    Yield CType(Val, Single?)
                    Continue For
                End If

                If TypeOf Val Is Long Then
                    Yield CLng(Val)
                    Continue For
                End If

                If TypeOf Val Is Long? Then
                    Yield CType(Val, Long?)
                    Continue For
                End If

                If TypeOf Val Is ULong Then
                    Yield CULng(Val)
                    Continue For
                End If

                If TypeOf Val Is ULong? Then
                    Yield CType(Val, ULong?)
                    Continue For
                End If

                If TypeOf Val Is Integer Then
                    Yield CInt(Val)
                    Continue For
                End If

                If TypeOf Val Is Integer? Then
                    Yield CType(Val, Integer?)
                    Continue For
                End If

                If TypeOf Val Is UInteger Then
                    Yield CUInt(Val)
                    Continue For
                End If

                If TypeOf Val Is UInteger? Then
                    Yield CType(Val, UInteger?)
                    Continue For
                End If

                If TypeOf Val Is Short Then
                    Yield CShort(Val)
                    Continue For
                End If

                If TypeOf Val Is Short? Then
                    Yield CType(Val, Short?)
                    Continue For
                End If

                If TypeOf Val Is UShort Then
                    Yield CUShort(Val)
                    Continue For
                End If

                If TypeOf Val Is UShort? Then
                    Yield CType(Val, UShort?)
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                If TypeOf Val Is Byte? Then
                    Yield CType(Val, Byte?)
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                If TypeOf Val Is SByte? Then
                    Yield CType(Val, SByte?)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to double?")
            Next
        End Function

        Private Iterator Function WidenToFloat(untyped As IEnumerable) As IEnumerable(Of Single)
            For Each Val As Object In untyped
                If TypeOf Val Is Single Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Long Then
                    Yield CLng(Val)
                    Continue For
                End If

                If TypeOf Val Is ULong Then
                    Yield CULng(Val)
                    Continue For
                End If

                If TypeOf Val Is Integer Then
                    Yield CInt(Val)
                    Continue For
                End If

                If TypeOf Val Is UInteger Then
                    Yield CUInt(Val)
                    Continue For
                End If

                If TypeOf Val Is Short Then
                    Yield CShort(Val)
                    Continue For
                End If

                If TypeOf Val Is UShort Then
                    Yield CUShort(Val)
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to float")
            Next
        End Function

        Private Iterator Function WidenToNullableFloat(untyped As IEnumerable) As IEnumerable(Of Single?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is Single Then
                    Yield CSng(Val)
                    Continue For
                End If

                If TypeOf Val Is Single? Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Long Then
                    Yield CLng(Val)
                    Continue For
                End If

                If TypeOf Val Is Long? Then
                    Yield CType(Val, Long?)
                    Continue For
                End If

                If TypeOf Val Is ULong Then
                    Yield CULng(Val)
                    Continue For
                End If

                If TypeOf Val Is ULong? Then
                    Yield CType(Val, ULong?)
                    Continue For
                End If

                If TypeOf Val Is Integer Then
                    Yield CInt(Val)
                    Continue For
                End If

                If TypeOf Val Is Integer? Then
                    Yield CType(Val, Integer?)
                    Continue For
                End If

                If TypeOf Val Is UInteger Then
                    Yield CUInt(Val)
                    Continue For
                End If

                If TypeOf Val Is UInteger? Then
                    Yield CType(Val, UInteger?)
                    Continue For
                End If

                If TypeOf Val Is Short Then
                    Yield CShort(Val)
                    Continue For
                End If

                If TypeOf Val Is Short? Then
                    Yield CType(Val, Short?)
                    Continue For
                End If

                If TypeOf Val Is UShort Then
                    Yield CUShort(Val)
                    Continue For
                End If

                If TypeOf Val Is UShort? Then
                    Yield CType(Val, UShort?)
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                If TypeOf Val Is Byte? Then
                    Yield CType(Val, Byte?)
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                If TypeOf Val Is SByte? Then
                    Yield CType(Val, SByte?)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to float?")
            Next
        End Function

        Private Iterator Function WidenToLong(untyped As IEnumerable) As IEnumerable(Of Long)
            For Each Val As Object In untyped
                If TypeOf Val Is Long Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Integer Then
                    Yield CInt(Val)
                    Continue For
                End If

                If TypeOf Val Is Short Then
                    Yield CShort(Val)
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to long")
            Next
        End Function

        Private Iterator Function WidenToNullableLong(untyped As IEnumerable) As IEnumerable(Of Long?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is Long Then
                    Yield CLng(Val)
                    Continue For
                End If

                If TypeOf Val Is Long? Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Integer Then
                    Yield CInt(Val)
                    Continue For
                End If

                If TypeOf Val Is Integer? Then
                    Yield CType(Val, Integer?)
                    Continue For
                End If

                If TypeOf Val Is Short Then
                    Yield CShort(Val)
                    Continue For
                End If

                If TypeOf Val Is Short? Then
                    Yield CType(Val, Short?)
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                If TypeOf Val Is SByte? Then
                    Yield CType(Val, SByte?)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to long?")
            Next
        End Function

        Private Iterator Function WidenToULong(untyped As IEnumerable) As IEnumerable(Of ULong)
            For Each Val As Object In untyped
                If TypeOf Val Is ULong Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is UInteger Then
                    Yield CUInt(Val)
                    Continue For
                End If

                If TypeOf Val Is UShort Then
                    Yield CUShort(Val)
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to ulong")
            Next
        End Function

        Private Iterator Function WidenToNullableULong(untyped As IEnumerable) As IEnumerable(Of ULong?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is ULong Then
                    Yield CULng(Val)
                    Continue For
                End If

                If TypeOf Val Is ULong? Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is UInteger Then
                    Yield CUInt(Val)
                    Continue For
                End If

                If TypeOf Val Is UInteger? Then
                    Yield CType(Val, UInteger?)
                    Continue For
                End If

                If TypeOf Val Is UShort Then
                    Yield CUShort(Val)
                    Continue For
                End If

                If TypeOf Val Is UShort? Then
                    Yield CType(Val, UShort?)
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                If TypeOf Val Is Byte? Then
                    Yield CType(Val, Byte?)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to ulong?")
            Next
        End Function

        Private Iterator Function WidenToInt(untyped As IEnumerable) As IEnumerable(Of Integer)
            For Each Val As Object In untyped
                If TypeOf Val Is Integer Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Short Then
                    Yield CShort(Val)
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to int")
            Next
        End Function

        Private Iterator Function WidenToNullableInt(untyped As IEnumerable) As IEnumerable(Of Integer?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is Integer Then
                    Yield CInt(Val)
                    Continue For
                End If

                If TypeOf Val Is Integer? Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Short Then
                    Yield CShort(Val)
                    Continue For
                End If

                If TypeOf Val Is Short? Then
                    Yield CType(Val, Short?)
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                If TypeOf Val Is SByte? Then
                    Yield CType(Val, SByte?)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to int?")
            Next
        End Function

        Private Iterator Function WidenToUInt(untyped As IEnumerable) As IEnumerable(Of UInteger)
            For Each Val As Object In untyped
                If TypeOf Val Is UInteger Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is UShort Then
                    Yield CUShort(Val)
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to uint")
            Next
        End Function

        Private Iterator Function WidenToNullableUInt(untyped As IEnumerable) As IEnumerable(Of UInteger?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is UInteger Then
                    Yield CUInt(Val)
                    Continue For
                End If

                If TypeOf Val Is UInteger? Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is UShort Then
                    Yield CUShort(Val)
                    Continue For
                End If

                If TypeOf Val Is UShort? Then
                    Yield CType(Val, UShort?)
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                If TypeOf Val Is Byte? Then
                    Yield CType(Val, Byte?)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to uint?")
            Next
        End Function

        Private Iterator Function WidenToShort(untyped As IEnumerable) As IEnumerable(Of Short)
            For Each Val As Object In untyped
                If TypeOf Val Is Short Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to short")
            Next
        End Function

        Private Iterator Function WidenToNullableShort(untyped As IEnumerable) As IEnumerable(Of Short?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is Short Then
                    Yield CShort(Val)
                    Continue For
                End If

                If TypeOf Val Is Short? Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                If TypeOf Val Is SByte? Then
                    Yield CType(Val, SByte?)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to short?")
            Next
        End Function

        Private Iterator Function WidenToUShort(untyped As IEnumerable) As IEnumerable(Of UShort)
            For Each Val As Object In untyped
                If TypeOf Val Is UShort Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to uint")
            Next
        End Function

        Private Iterator Function WidenToNullableUShort(untyped As IEnumerable) As IEnumerable(Of UShort?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is UShort Then
                    Yield CUShort(Val)
                    Continue For
                End If

                If TypeOf Val Is UShort? Then
                    Yield Val
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                If TypeOf Val Is Byte? Then
                    Yield CType(Val, Byte?)
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to ushort?")
            Next
        End Function

        Private Iterator Function WidenToSByte(untyped As IEnumerable) As IEnumerable(Of SByte)
            For Each Val As Object In untyped
                If TypeOf Val Is SByte Then
                    Yield Val
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to sbyte")
            Next
        End Function

        Private Iterator Function WidenToNullableSByte(untyped As IEnumerable) As IEnumerable(Of SByte?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is SByte Then
                    Yield CSByte(Val)
                    Continue For
                End If

                If TypeOf Val Is SByte? Then
                    Yield Val
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to sbyte?")
            Next
        End Function

        Private Iterator Function WidenToByte(untyped As IEnumerable) As IEnumerable(Of Byte)
            For Each Val As Object In untyped
                If TypeOf Val Is Byte Then
                    Yield Val
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to byte")
            Next
        End Function

        Private Iterator Function WidenToNullableByte(untyped As IEnumerable) As IEnumerable(Of Byte?)
            For Each Val As Object In untyped
                If Val Is Nothing Then
                    Yield Nothing
                    Continue For
                End If

                If TypeOf Val Is Byte Then
                    Yield CByte(Val)
                    Continue For
                End If

                If TypeOf Val Is Byte? Then
                    Yield Val
                    Continue For
                End If

                Throw New InvalidOperationException($"Tried to widen {If(Val?.GetType()?.Name, "--NULL--")} to byte?")
            Next
        End Function
    End Module
End Namespace

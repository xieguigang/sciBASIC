#Region "Microsoft.VisualBasic::ad981123c51e4cb0cd4c632f75e13303, Data\BinaryData\Feather\Impl\WriterAdapterLookup.vb"

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

    '   Total Lines: 329
    '    Code Lines: 261 (79.33%)
    ' Comment Lines: 9 (2.74%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 59 (17.93%)
    '     File Size: 21.40 KB


    '     Module WriterAdapterLookup
    ' 
    '         Function: LookupAdapter, LookupArrayAdapter, LookupCollectionAdapter, LookupDefaultAdapter, LookupEnumerableAdapter
    '         Class AdapterKey
    ' 
    '             Properties: FromType, ToType
    ' 
    '             Function: (+2 Overloads) Equals, GetHashCode
    ' 
    '         Class WriterMethodLookup
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: GetBlitterMethod
    ' 
    ' 
    ' 
    '     Module ArrayAdapter
    ' 
    '         Function: Create
    ' 
    '     Module CollectionAdapter
    ' 
    '         Function: Create
    ' 
    '     Module EnumerableAdapter
    ' 
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Reflection.Emit

Namespace Impl
    Friend Module WriterAdapterLookup
        Friend Class AdapterKey
            Implements IEquatable(Of AdapterKey)
            Public Property FromType As Type
            Public Property ToType As ColumnType

            Public Overrides Function Equals(obj As Object) As Boolean
                If Not (TypeOf obj Is AdapterKey) Then Return False

                Return Equals(CType(obj, AdapterKey))
            End Function

            Public Overrides Function GetHashCode() As Integer
                Return (FromType.GetHashCode() * 17) + ToType.GetHashCode()
            End Function

            Public Overloads Function Equals(other As AdapterKey) As Boolean Implements IEquatable(Of AdapterKey).Equals
                Return other.FromType Is FromType AndAlso other.ToType = ToType
            End Function
        End Class

        Public Class WriterMethodLookup
            Private NonNullBoolMethodName As String
            Private NonNullDateTimeMethodName As String
            Private NonNullDateTimeOffsetMethodName As String
            Private NonNullTimeSpanMethodName As String
            Private NonNullCollectionMethodName As String
            Private NonNullStringMethodName As String
            Private NonNullEnumMethodName As String

            Private NullBoolMethodName As String
            Private NullDateTimeMethodName As String
            Private NullDateTimeOffsetMethodName As String
            Private NullTimeSpanMethodName As String
            Private NullCollectionMethodName As String
            Private NullStringMethodName As String
            Private NullEnumMethodName As String


            Public Sub New(nonNullBoolMethod As String, nonNullDateTimeMethod As String, nonNullDateTimeOffsetMethod As String, nonNullTimeSpanMethod As String, nonNullCollectionMethod As String, nonNullStringMethod As String, nonNullEnumMethod As String, nullBoolMethod As String, nullDateTimeMethod As String, nullDateTimeOffsetMethod As String, nullTimeSpanMethod As String, nullCollectionMethod As String, nullStringMethod As String, nullEnumMethod As String)
                NonNullBoolMethodName = nonNullBoolMethod
                NonNullDateTimeMethodName = nonNullDateTimeMethod
                NonNullDateTimeOffsetMethodName = nonNullDateTimeOffsetMethod
                NonNullTimeSpanMethodName = nonNullTimeSpanMethod
                NonNullCollectionMethodName = nonNullCollectionMethod
                NonNullStringMethodName = nonNullStringMethod
                NonNullEnumMethodName = nonNullEnumMethod

                NullBoolMethodName = nullBoolMethod
                NullDateTimeMethodName = nullDateTimeMethod
                NullDateTimeOffsetMethodName = nullDateTimeOffsetMethod
                NullTimeSpanMethodName = nullTimeSpanMethod
                NullCollectionMethodName = nullCollectionMethod
                NullStringMethodName = nullStringMethod
                NullEnumMethodName = nullEnumMethod
            End Sub

            Public Function GetBlitterMethod(elementType As Type, columnEnumerableType As Type, toType As ColumnType) As MethodInfo
                Dim blit As MethodInfo

                If elementType.IsValueType Then
                    If Nullable.GetUnderlyingType(elementType) Is Nothing Then
                        ' non-nullable primitives

                        If elementType Is GetType(Boolean) Then
                            blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NonNullBoolMethodName))
                        Else
                            If elementType Is GetType(TimeSpan) Then
                                blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NonNullTimeSpanMethodName))
                            Else
                                If elementType Is GetType(Date) Then
                                    blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NonNullDateTimeMethodName))
                                Else
                                    If elementType Is GetType(DateTimeOffset) Then
                                        blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NonNullDateTimeOffsetMethodName))
                                    Else
                                        If elementType.IsEnum Then
                                            Dim genBlit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NonNullEnumMethodName))
                                            blit = genBlit.MakeGenericMethod(elementType)
                                        Else
                                            blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NonNullCollectionMethodName) AndAlso d.GetParameters()(0).ParameterType Is columnEnumerableType)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Else
                        ' nullable primitives
                        If elementType Is GetType(Boolean?) Then
                            blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NullBoolMethodName))
                        Else
                            If elementType Is GetType(TimeSpan?) Then
                                blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NullTimeSpanMethodName))
                            Else
                                If elementType Is GetType(Date?) Then
                                    blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NullDateTimeMethodName))
                                Else
                                    If elementType Is GetType(DateTimeOffset?) Then
                                        blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NullDateTimeOffsetMethodName))
                                    Else
                                        If Nullable.GetUnderlyingType(elementType).IsEnum Then
                                            Dim genBlit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NullEnumMethodName))
                                            blit = genBlit.MakeGenericMethod(Nullable.GetUnderlyingType(elementType))
                                        Else
                                            blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NullCollectionMethodName) AndAlso d.GetParameters()(0).ParameterType Is columnEnumerableType)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                Else
                    If elementType Is GetType(String) Then
                        If toType = ColumnType.String Then
                            blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NonNullStringMethodName))
                        Else
                            If toType = ColumnType.NullableString Then
                                blit = GetType(FeatherWriter).GetMethods(BindingFlags.NonPublic Or BindingFlags.Instance).[Single](Function(d) Equals(d.Name, NullStringMethodName) AndAlso d.GetParameters()(0).ParameterType Is columnEnumerableType)
                            Else
                                Throw New InvalidOperationException($"Unexpected request for a blit method from a string ({elementType.Name}, {columnEnumerableType.Name}, {toType}")
                            End If
                        End If
                    Else
                        If elementType Is GetType([Enum]) Then
                            Throw New NotImplementedException()
                        Else
                            Throw New InvalidOperationException($"Unexpected request for a blit method from a nullable type ({elementType.Name}, {columnEnumerableType.Name}, {toType}")
                        End If
                    End If
                End If

                Return blit
            End Function
        End Class

        Public Function LookupAdapter(dataSource As Type, elementType As Type, toType As ColumnType) As Action(Of FeatherWriter, IEnumerable)
            Dim interfaces = dataSource.GetInterfaces()
            Dim isArray = dataSource.IsArray
            Dim iCollection = interfaces.FirstOrDefault(Function(i) i.IsGenericType AndAlso i.GetGenericTypeDefinition() Is GetType(ICollection(Of)))
            Dim iEnumerable = interfaces.FirstOrDefault(Function(i) i.IsGenericType AndAlso i.GetGenericTypeDefinition() Is GetType(IEnumerable(Of)))

            If isArray AndAlso dataSource.GetElementType() Is elementType Then
                Return LookupArrayAdapter(elementType, toType)
            End If

            If iCollection IsNot Nothing AndAlso iCollection.GetGenericArguments()(0) Is elementType Then
                Return LookupCollectionAdapter(elementType, toType)
            End If

            If iEnumerable IsNot Nothing AndAlso iEnumerable.GetGenericArguments()(0) Is elementType Then
                Return LookupEnumerableAdapter(elementType, toType)
            End If

            ' life just got interesting, need to do some work per-element
            Return LookupDefaultAdapter(elementType, toType)
        End Function

        Private ReadOnly ArrayAdapters As Dictionary(Of AdapterKey, Action(Of FeatherWriter, IEnumerable)) = New Dictionary(Of AdapterKey, Action(Of FeatherWriter, IEnumerable))()
        Private Function LookupArrayAdapter(elementType As Type, toType As ColumnType) As Action(Of FeatherWriter, IEnumerable)
            Dim key = New AdapterKey With {
                .FromType = elementType,
                .ToType = toType
            }

            ' making an assumption that actually using the adapter
            '   dominates creating it, making this lock acceptable
            SyncLock ArrayAdapters
                Dim ret As Action(Of FeatherWriter, IEnumerable)
                If ArrayAdapters.TryGetValue(key, ret) Then Return ret
                ret = ArrayAdapter.Create(elementType, toType)
                ArrayAdapters(key) = ret

                Return ret
            End SyncLock
        End Function

        Private ReadOnly CollectionAdapters As Dictionary(Of AdapterKey, Action(Of FeatherWriter, IEnumerable)) = New Dictionary(Of AdapterKey, Action(Of FeatherWriter, IEnumerable))()
        Private Function LookupCollectionAdapter(elementType As Type, toType As ColumnType) As Action(Of FeatherWriter, IEnumerable)
            Dim key = New AdapterKey With {
                .FromType = elementType,
                .ToType = toType
            }

            ' making an assumption that actually using the adapter
            '   dominates creating it, making this lock acceptable
            SyncLock CollectionAdapters
                Dim ret As Action(Of FeatherWriter, IEnumerable) = Nothing
                If CollectionAdapters.TryGetValue(key, ret) Then Return ret
                ret = CollectionAdapter.Create(elementType, toType)
                CollectionAdapters(key) = ret

                Return ret
            End SyncLock
        End Function

        Private ReadOnly EnumerableAdapters As Dictionary(Of AdapterKey, Action(Of FeatherWriter, IEnumerable)) = New Dictionary(Of AdapterKey, Action(Of FeatherWriter, IEnumerable))()
        Private Function LookupEnumerableAdapter(elementType As Type, toType As ColumnType) As Action(Of FeatherWriter, IEnumerable)
            Dim key = New AdapterKey With {
                .FromType = elementType,
                .ToType = toType
            }

            ' making an assumption that actually using the adapter
            '   dominates creating it, making this lock acceptable
            SyncLock EnumerableAdapters
                Dim ret As Action(Of FeatherWriter, IEnumerable)
                If EnumerableAdapters.TryGetValue(key, ret) Then Return ret
                ret = EnumerableAdapter.Create(elementType, toType)
                EnumerableAdapters(key) = ret

                Return ret
            End SyncLock
        End Function

        Private ReadOnly UntypedEnumerableAdapters As Dictionary(Of AdapterKey, Action(Of FeatherWriter, IEnumerable)) = New Dictionary(Of AdapterKey, Action(Of FeatherWriter, IEnumerable))()
        Private Function LookupDefaultAdapter(elementType As Type, toType As ColumnType) As Action(Of FeatherWriter, IEnumerable)
            Dim key = New AdapterKey With {
                .FromType = elementType,
                .ToType = toType
            }

            SyncLock UntypedEnumerableAdapters
                Dim ret As Action(Of FeatherWriter, IEnumerable) = Nothing
                If UntypedEnumerableAdapters.TryGetValue(key, ret) Then Return ret

                Dim widener = GetWidener(elementType)

                Dim typedAdapter = LookupEnumerableAdapter(elementType, toType)
                ret = Sub(writer, data)
                          Dim widend = widener(data)
                          typedAdapter(writer, widend)
                      End Sub
                UntypedEnumerableAdapters(key) = ret

                Return ret
            End SyncLock
        End Function
    End Module

    Friend Module ArrayAdapter

        Private ReadOnly Lookup As WriterMethodLookup = New WriterMethodLookup(NameOf(FeatherWriter.BlitNonNullableBoolArray), NameOf(FeatherWriter.BlitDateTimeArray), NameOf(FeatherWriter.BlitDateTimeOffsetArray), NameOf(FeatherWriter.BlitTimeSpanArray), NameOf(FeatherWriter.BlitNonNullableArray), NameOf(FeatherWriter.CopyStringArray), NameOf(FeatherWriter.BlitNonNullableEnumArray), NameOf(FeatherWriter.BlitNullableBoolArray), NameOf(FeatherWriter.BlitNullableDateTimeArray), NameOf(FeatherWriter.BlitNullableDateTimeOffsetArray), NameOf(FeatherWriter.BlitNullableTimeSpanArray), NameOf(FeatherWriter.BlitNullableArray), NameOf(FeatherWriter.CopyNullableStringArray), NameOf(FeatherWriter.BlitNullableEnumArray))

        Public Function Create(elementType As Type, toType As ColumnType) As Action(Of FeatherWriter, IEnumerable)
            Dim dyn = New DynamicMethod("ArrayAdapter_" & elementType.Name & "_" & toType.ToString(), Nothing, {GetType(FeatherWriter), GetType(IEnumerable)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            Dim arrType = elementType.MakeArrayType()
            Dim blit = Lookup.GetBlitterMethod(elementType, arrType, toType)

            Dim arr = il.DeclareLocal(arrType)

            il.Emit(OpCodes.Ldarg_1)                               ' IEnumerable
            il.Emit(OpCodes.Castclass, arrType)                    ' elementType[]
            il.Emit(OpCodes.Stloc, arr)                            ' --empty--

            il.Emit(OpCodes.Ldarg_0)                               ' FeatherWriter
            il.Emit(OpCodes.Ldloc, arr)                            ' FeatherWriter elementType[]
            il.Emit(OpCodes.Call, blit)                            ' --empty--
            il.Emit(OpCodes.Ret)                                   ' --empty--

            Dim ret = CType(dyn.CreateDelegate(GetType(Action(Of FeatherWriter, IEnumerable))), Action(Of FeatherWriter, IEnumerable))
            Return ret
        End Function
    End Module

    Friend Module CollectionAdapter

        Private ReadOnly Lookup As WriterMethodLookup = New WriterMethodLookup(NameOf(FeatherWriter.CopyNonNullableBoolCollection), NameOf(FeatherWriter.CopyDateTimeCollection), NameOf(FeatherWriter.CopyDateTimeOffsetCollection), NameOf(FeatherWriter.CopyTimeSpanCollection), NameOf(FeatherWriter.CopyNonNullableCollection), NameOf(FeatherWriter.CopyStringCollection), NameOf(FeatherWriter.CopyNonNullableEnumCollection), NameOf(FeatherWriter.CopyNullableBoolCollection), NameOf(FeatherWriter.CopyNullableDateTimeCollection), NameOf(FeatherWriter.CopyNullableDateTimeOffsetCollection), NameOf(FeatherWriter.CopyNullableTimeSpanCollection), NameOf(FeatherWriter.CopyNullableCollection), NameOf(FeatherWriter.CopyNullableStringCollection), NameOf(FeatherWriter.CopyNullableEnumCollection))

        Public Function Create(elementType As Type, toType As ColumnType) As Action(Of FeatherWriter, IEnumerable)
            Dim dyn = New DynamicMethod("CollectionAdapter_" & elementType.Name & "_" & toType.ToString(), Nothing, {GetType(FeatherWriter), GetType(IEnumerable)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            Dim collectionType = GetType(ICollection(Of)).MakeGenericType(elementType)

            Dim arr = il.DeclareLocal(collectionType)

            il.Emit(OpCodes.Ldarg_1)                                                   ' IEnumerable
            il.Emit(OpCodes.Castclass, collectionType)                                 ' ICollection<elementType>
            il.Emit(OpCodes.Stloc, arr)                                                ' --empty--

            Dim blit = Lookup.GetBlitterMethod(elementType, collectionType, toType)

            il.Emit(OpCodes.Ldarg_0)                                                   ' FeatherWriter
            il.Emit(OpCodes.Ldloc, arr)                                                ' FeatherWriter ICollection<elementType>
            il.Emit(OpCodes.Call, blit)                                                ' --empty--
            il.Emit(OpCodes.Ret)                                                       ' --empty--

            Dim ret = CType(dyn.CreateDelegate(GetType(Action(Of FeatherWriter, IEnumerable))), Action(Of FeatherWriter, IEnumerable))
            Return ret
        End Function
    End Module

    Friend Module EnumerableAdapter

        Private ReadOnly Lookup As WriterMethodLookup = New WriterMethodLookup(NameOf(FeatherWriter.CopyNonNullableBoolIEnumerable), NameOf(FeatherWriter.CopyDateTimeIEnumerable), NameOf(FeatherWriter.CopyDateTimeOffsetIEnumerable), NameOf(FeatherWriter.CopyTimeSpanIEnumerable), NameOf(FeatherWriter.CopyNonNullableIEnumerable), NameOf(FeatherWriter.CopyStringIEnumerable), NameOf(FeatherWriter.CopyNonNullableEnumIEnumerable), NameOf(FeatherWriter.CopyNullableBoolIEnumerable), NameOf(FeatherWriter.CopyNullableDateTimeIEnumerable), NameOf(FeatherWriter.CopyNullableDateTimeOffsetIEnumerable), NameOf(FeatherWriter.CopyNullableTimeSpanIEnumerable), NameOf(FeatherWriter.CopyNullableIEnumerable), NameOf(FeatherWriter.CopyNullableStringIEnumerable), NameOf(FeatherWriter.CopyNullableEnumIEnumerable))

        Public Function Create(elementType As Type, toType As ColumnType) As Action(Of FeatherWriter, IEnumerable)
            Dim dyn = New DynamicMethod("CollectionAdapter_" & elementType.Name & "_" & toType.ToString(), Nothing, {GetType(FeatherWriter), GetType(IEnumerable)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            Dim collectionType = GetType(IEnumerable(Of)).MakeGenericType(elementType)

            Dim blit = Lookup.GetBlitterMethod(elementType, collectionType, toType)

            Dim arr = il.DeclareLocal(collectionType)

            il.Emit(OpCodes.Ldarg_1)                                   ' IEnumerable
            il.Emit(OpCodes.Castclass, collectionType)                 ' IEnumerable<elementType>
            il.Emit(OpCodes.Stloc, arr)                                ' --empty--

            il.Emit(OpCodes.Ldarg_0)                                   ' FeatherWriter
            il.Emit(OpCodes.Ldloc, arr)                                ' FeatherWriter IEnumerable<elementType>
            il.Emit(OpCodes.Call, blit)                                ' --empty--
            il.Emit(OpCodes.Ret)                                       ' --empty--

            Dim ret = CType(dyn.CreateDelegate(GetType(Action(Of FeatherWriter, IEnumerable))), Action(Of FeatherWriter, IEnumerable))
            Return ret
        End Function
    End Module
End Namespace

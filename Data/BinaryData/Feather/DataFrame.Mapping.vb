#Region "Microsoft.VisualBasic::c375f4f7ecb300642b6cee7366f9b3bb, Data\BinaryData\Feather\DataFrame.Mapping.vb"

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

    '   Total Lines: 686
    '    Code Lines: 448
    ' Comment Lines: 124
    '   Blank Lines: 114
    '     File Size: 31.09 KB


    ' Class DataFrame
    ' 
    '     Function: MakeDefaultFactory, MakeMapper, MakeProxy, (+8 Overloads) Map, (+2 Overloads) Proxy
    '               TryInferMapping, (+8 Overloads) TryMap, (+2 Overloads) TryProxy, TrySuggestMapping
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl


Partial Public Class DataFrame
    ''' <summary>
    ''' Map the columns in this dataframe to a given type.
    ''' 
    ''' If the column names match (in a case insensitive, culture invariant comparison) the mapping can be automatic.
    ''' 
    ''' Otherwise, list out the member names in column order to map to.
    ''' 
    ''' Objects of TProxyType will be created by calling the default constructor.
    ''' 
    ''' Throws if the types are not compatible.
    ''' </summary>
    Public Function Proxy(Of TProxyType As New)(ParamArray membersToColumns As String()) As ProxyDataFrame(Of TProxyType)
        Return Proxy(MakeDefaultFactory(Of TProxyType)(), membersToColumns)
    End Function

    ''' <summary>
    ''' Map the columns in this dataframe to a given type.
    ''' 
    ''' If the column names match (in a case insensitive, culture invariant comparison) the mapping can be automatic.
    ''' 
    ''' Otherwise, list out the member names in column order to map to.
    ''' 
    ''' Objects of TProxyType will be created by calling the provided proxy function.
    ''' 
    ''' Throws if the types are not compatible.
    ''' </summary>
    Public Function Proxy(Of TProxyType)(factory As Func(Of TProxyType), ParamArray membersToColumns As String()) As ProxyDataFrame(Of TProxyType)
        If factory Is Nothing Then Throw New ArgumentNullException(NameOf(factory))

        Dim columnsToMembers As Dictionary(Of Long, MemberInfo)
        Dim errorMessage As String
        If Not TryInferMapping(Of TProxyType)(membersToColumns, columnsToMembers, errorMessage) Then
            Throw New ArgumentException(errorMessage, NameOf(membersToColumns))
        End If

        Return MakeProxy(columnsToMembers, factory)
    End Function

    ''' <summary>
    ''' Map the columns in this dataframe to a given type.
    ''' 
    ''' If the column names match (in a case insensitive, culture invariant comparison) the mapping can be automatic.
    ''' 
    ''' Otherwise, list out the member names in column order to map to.
    ''' 
    ''' Objects of TProxyType will be created by calling the default constructor.
    ''' 
    ''' Return false if the mapping cannot be made, and true otherwise.
    ''' </summary>
    Public Function TryProxy(Of TProxyType As New)(<Out> ByRef dataframe As ProxyDataFrame(Of TProxyType), ParamArray membersToColumns As String()) As Boolean
        Return TryProxy(MakeDefaultFactory(Of TProxyType)(), dataframe, membersToColumns)
    End Function

    ''' <summary>
    ''' Map the columns in this dataframe to a given type.
    ''' 
    ''' If the column names match (in a case insensitive, culture invariant comparison) the mapping can be automatic.
    ''' 
    ''' Otherwise, list out the member names in column order to map to.
    ''' 
    ''' Objects of TProxyType will be created by calling the provided proxy function.
    ''' 
    ''' Return false if the mapping cannot be made, and true otherwise.
    ''' </summary>
    Public Function TryProxy(Of TProxyType)(factory As Func(Of TProxyType), <Out> ByRef dataframe As ProxyDataFrame(Of TProxyType), ParamArray membersToColumns As String()) As Boolean
        If factory Is Nothing Then
            dataframe = Nothing
            Return False
        End If

        Dim columnsToMembers As Dictionary(Of Long, MemberInfo)
        Dim __ As String
        If Not TryInferMapping(Of TProxyType)(membersToColumns, columnsToMembers, __) Then
            dataframe = Nothing
            Return False
        End If

        dataframe = MakeProxy(columnsToMembers, factory)
        Return True
    End Function

    ''' <summary>
    ''' Maps this dataframe to a dataframe with a single column of the given type.
    ''' 
    ''' Throws if the mapping cannot be made.
    ''' </summary>
    Public Function Map(Of TCol1)() As TypedDataFrame(Of TCol1)
        If ColumnCount < 1 Then
            Throw New ArgumentException($"Cannot map dataframe, mapping has 1 column while dataframe has {ColumnCount:N0} columns")
        End If

        Dim ret As TypedDataFrame(Of TCol1)
        If Not TryMap(ret) Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Enumerable.ElementAt(AllColumns, 0).Type.Name}")
        End If

        Return ret
    End Function

    ''' <summary>
    ''' Tries to map this dataframe to a dataframe with a single column of the given type.
    ''' 
    ''' Return true if such a mapping was possible, and false otherwise.
    ''' </summary>
    Public Function TryMap(Of TCol1)(<Out> ByRef dataframe As TypedDataFrame(Of TCol1)) As Boolean
        If ColumnCount < 1 Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(0).CanMapTo(GetType(TCol1)) Then
            dataframe = Nothing
            Return False
        End If

        dataframe = New TypedDataFrame(Of TCol1)(Me)
        Return True
    End Function

    ''' <summary>
    ''' Maps this dataframe to a dataframe with two columns of the given types.
    ''' 
    ''' Throws if the mapping cannot be made.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2)() As TypedDataFrameType(Of TCol1, TCol2)
        If ColumnCount < 2 Then
            Throw New ArgumentException($"Cannot map dataframe, mapping has 2 columns while dataframe has {ColumnCount:N0} columns")
        End If

        Dim ret As TypedDataFrameType(Of TCol1, TCol2)
        If Not TryMap(ret) Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Enumerable.ElementAt(AllColumns, 0).Type.Name}, {GetType(TCol2).Name} = {Enumerable.ElementAt(AllColumns, 1).Type.Name}")
        End If

        Return ret
    End Function

    ''' <summary>
    ''' Tries to map this dataframe to a dataframe with two columns of the given type.
    ''' 
    ''' Return true if such a mapping was possible, and false otherwise.
    ''' </summary>
    Public Function TryMap(Of TCol1, TCol2)(<Out> ByRef dataframe As TypedDataFrameType(Of TCol1, TCol2)) As Boolean
        If ColumnCount < 2 Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(0).CanMapTo(GetType(TCol1)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(1).CanMapTo(GetType(TCol2)) Then
            dataframe = Nothing
            Return False
        End If

        dataframe = New TypedDataFrameType(Of TCol1, TCol2)(Me)
        Return True
    End Function

    ''' <summary>
    ''' Maps this dataframe to a dataframe with three columns of the given types.
    ''' 
    ''' Throws if the mapping cannot be made.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3)() As TypedDataFrameType1(Of TCol1, TCol2, TCol3)
        If ColumnCount < 2 Then
            Throw New ArgumentException($"Cannot map dataframe, mapping has 3 columns while dataframe has {ColumnCount:N0} columns")
        End If

        Dim ret As TypedDataFrameType1(Of TCol1, TCol2, TCol3)
        If Not TryMap(ret) Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Enumerable.ElementAt(AllColumns, 0).Type.Name}, {GetType(TCol2).Name} = {Enumerable.ElementAt(AllColumns, 1).Type.Name}, {GetType(TCol3).Name} = {Enumerable.ElementAt(AllColumns, 2).Type.Name}")
        End If

        Return ret
    End Function

    ''' <summary>
    ''' Tries to map this dataframe to a dataframe with three columns of the given type.
    ''' 
    ''' Return true if such a mapping was possible, and false otherwise.
    ''' </summary>
    Public Function TryMap(Of TCol1, TCol2, TCol3)(<Out> ByRef dataframe As TypedDataFrameType1(Of TCol1, TCol2, TCol3)) As Boolean
        If ColumnCount < 3 Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(0).CanMapTo(GetType(TCol1)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(1).CanMapTo(GetType(TCol2)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(2).CanMapTo(GetType(TCol3)) Then
            dataframe = Nothing
            Return False
        End If

        dataframe = New TypedDataFrameType1(Of TCol1, TCol2, TCol3)(Me)
        Return True
    End Function

    ''' <summary>
    ''' Maps this dataframe to a dataframe with four columns of the given types.
    ''' 
    ''' Throws if the mapping cannot be made.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3, TCol4)() As TypedDataFrameType2(Of TCol1, TCol2, TCol3, TCol4)
        If ColumnCount < 4 Then
            Throw New ArgumentException($"Cannot map dataframe, mapping has 4 columns while dataframe has {ColumnCount:N0} columns")
        End If

        Dim ret As TypedDataFrameType2(Of TCol1, TCol2, TCol3, TCol4)
        If Not TryMap(ret) Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Enumerable.ElementAt(AllColumns, 0).Type.Name}, {GetType(TCol2).Name} = {Enumerable.ElementAt(AllColumns, 1).Type.Name}, {GetType(TCol3).Name} = {Enumerable.ElementAt(AllColumns, 2).Type.Name}, {GetType(TCol4).Name} = {Enumerable.ElementAt(AllColumns, 3).Type.Name}")
        End If

        Return ret
    End Function

    ''' <summary>
    ''' Tries to map this dataframe to a dataframe with four columns of the given type.
    ''' 
    ''' Return true if such a mapping was possible, and false otherwise.
    ''' </summary>
    Public Function TryMap(Of TCol1, TCol2, TCol3, TCol4)(<Out> ByRef dataframe As TypedDataFrameType2(Of TCol1, TCol2, TCol3, TCol4)) As Boolean
        If ColumnCount < 4 Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(0).CanMapTo(GetType(TCol1)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(1).CanMapTo(GetType(TCol2)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(2).CanMapTo(GetType(TCol3)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(3).CanMapTo(GetType(TCol4)) Then
            dataframe = Nothing
            Return False
        End If

        dataframe = New TypedDataFrameType2(Of TCol1, TCol2, TCol3, TCol4)(Me)
        Return True
    End Function

    ''' <summary>
    ''' Maps this dataframe to a dataframe with five columns of the given types.
    ''' 
    ''' Throws if the mapping cannot be made.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3, TCol4, TCol5)() As TypedDataFrameType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)
        If ColumnCount < 5 Then
            Throw New ArgumentException($"Cannot map dataframe, mapping has 5 columns while dataframe has {ColumnCount:N0} columns")
        End If

        Dim ret As TypedDataFrameType3(Of TCol1, TCol2, TCol3, TCol4, TCol5) = Nothing
        If Not TryMap(ret) Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Enumerable.ElementAt(AllColumns, 0).Type.Name}, {GetType(TCol2).Name} = {Enumerable.ElementAt(AllColumns, 1).Type.Name}, {GetType(TCol3).Name} = {Enumerable.ElementAt(AllColumns, 2).Type.Name}, {GetType(TCol4).Name} = {Enumerable.ElementAt(AllColumns, 3).Type.Name}, {GetType(TCol5).Name} = {Enumerable.ElementAt(AllColumns, 4).Type.Name}")
        End If

        Return ret
    End Function

    ''' <summary>
    ''' Tries to map this dataframe to a dataframe with five columns of the given type.
    ''' 
    ''' Return true if such a mapping was possible, and false otherwise.
    ''' </summary>
    Public Function TryMap(Of TCol1, TCol2, TCol3, TCol4, TCol5)(<Out> ByRef dataframe As TypedDataFrameType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)) As Boolean
        If ColumnCount < 5 Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(0).CanMapTo(GetType(TCol1)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(1).CanMapTo(GetType(TCol2)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(2).CanMapTo(GetType(TCol3)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(3).CanMapTo(GetType(TCol4)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(4).CanMapTo(GetType(TCol5)) Then
            dataframe = Nothing
            Return False
        End If

        dataframe = New TypedDataFrameType3(Of TCol1, TCol2, TCol3, TCol4, TCol5)(Me)
        Return True
    End Function

    ''' <summary>
    ''' Maps this dataframe to a dataframe with six columns of the given types.
    ''' 
    ''' Throws if the mapping cannot be made.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)() As TypedDataFrameType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)
        If ColumnCount < 6 Then
            Throw New ArgumentException($"Cannot map dataframe, mapping has 6 columns while dataframe has {ColumnCount:N0} columns")
        End If

        Dim ret As TypedDataFrameType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)
        If Not TryMap(ret) Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Enumerable.ElementAt(AllColumns, 0).Type.Name}, {GetType(TCol2).Name} = {Enumerable.ElementAt(AllColumns, 1).Type.Name}, {GetType(TCol3).Name} = {Enumerable.ElementAt(AllColumns, 2).Type.Name}, {GetType(TCol4).Name} = {Enumerable.ElementAt(AllColumns, 3).Type.Name}, {GetType(TCol5).Name} = {Enumerable.ElementAt(AllColumns, 4).Type.Name}, {GetType(TCol6).Name} = {Enumerable.ElementAt(AllColumns, 5).Type.Name}")
        End If

        Return ret
    End Function

    ''' <summary>
    ''' Tries to map this dataframe to a dataframe with six columns of the given type.
    ''' 
    ''' Return true if such a mapping was possible, and false otherwise.
    ''' </summary>
    Public Function TryMap(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)(<Out> ByRef dataframe As TypedDataFrameType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)) As Boolean
        If ColumnCount < 6 Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(0).CanMapTo(GetType(TCol1)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(1).CanMapTo(GetType(TCol2)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(2).CanMapTo(GetType(TCol3)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(3).CanMapTo(GetType(TCol4)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(4).CanMapTo(GetType(TCol5)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(5).CanMapTo(GetType(TCol6)) Then
            dataframe = Nothing
            Return False
        End If

        dataframe = New TypedDataFrameType4(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6)(Me)
        Return True
    End Function

    ''' <summary>
    ''' Maps this dataframe to a dataframe with seven columns of the given types.
    ''' 
    ''' Throws if the mapping cannot be made.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)() As TypedDataFrameType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)
        If ColumnCount < 7 Then
            Throw New ArgumentException($"Cannot map dataframe, mapping has 7 columns while dataframe has {ColumnCount:N0} columns")
        End If

        Dim ret As TypedDataFrameType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)
        If Not TryMap(ret) Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Enumerable.ElementAt(AllColumns, 0).Type.Name}, {GetType(TCol2).Name} = {Enumerable.ElementAt(AllColumns, 1).Type.Name}, {GetType(TCol3).Name} = {Enumerable.ElementAt(AllColumns, 2).Type.Name}, {GetType(TCol4).Name} = {Enumerable.ElementAt(AllColumns, 3).Type.Name}, {GetType(TCol5).Name} = {Enumerable.ElementAt(AllColumns, 4).Type.Name}, {GetType(TCol6).Name} = {Enumerable.ElementAt(AllColumns, 5).Type.Name}, {GetType(TCol7).Name} = {Enumerable.ElementAt(AllColumns, 6).Type.Name}")
        End If

        Return ret
    End Function

    ''' <summary>
    ''' Tries to map this dataframe to a dataframe with seven columns of the given type.
    ''' 
    ''' Return true if such a mapping was possible, and false otherwise.
    ''' </summary>
    Public Function TryMap(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)(<Out> ByRef dataframe As TypedDataFrameType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)) As Boolean
        If ColumnCount < 7 Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(0).CanMapTo(GetType(TCol1)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(1).CanMapTo(GetType(TCol2)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(2).CanMapTo(GetType(TCol3)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(3).CanMapTo(GetType(TCol4)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(4).CanMapTo(GetType(TCol5)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(5).CanMapTo(GetType(TCol6)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(6).CanMapTo(GetType(TCol7)) Then
            dataframe = Nothing
            Return False
        End If

        dataframe = New TypedDataFrameType5(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7)(Me)
        Return True
    End Function

    ''' <summary>
    ''' Maps this dataframe to a dataframe with eight columns of the given types.
    ''' 
    ''' Throws if the mapping cannot be made.
    ''' </summary>
    Public Function Map(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)() As TypedDataFrameType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)
        If ColumnCount < 8 Then
            Throw New ArgumentException($"Cannot map dataframe, mapping has 8 columns while dataframe has {ColumnCount:N0} columns")
        End If

        Dim ret As TypedDataFrameType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8) = Nothing
        If Not TryMap(ret) Then
            Throw New ArgumentException($"Cannot map dataframe given mapping: {GetType(TCol1).Name} = {Enumerable.ElementAt(AllColumns, 0).Type.Name}, {GetType(TCol2).Name} = {Enumerable.ElementAt(AllColumns, 1).Type.Name}, {GetType(TCol3).Name} = {Enumerable.ElementAt(AllColumns, 2).Type.Name}, {GetType(TCol4).Name} = {Enumerable.ElementAt(AllColumns, 3).Type.Name}, {GetType(TCol5).Name} = {Enumerable.ElementAt(AllColumns, 4).Type.Name}, {GetType(TCol6).Name} = {Enumerable.ElementAt(AllColumns, 5).Type.Name}, {GetType(TCol7).Name} = {Enumerable.ElementAt(AllColumns, 6).Type.Name}, {GetType(TCol8).Name} = {Enumerable.ElementAt(AllColumns, 7).Type.Name}")
        End If

        Return ret
    End Function

    ''' <summary>
    ''' Tries to map this dataframe to a dataframe with eight columns of the given type.
    ''' 
    ''' Return true if such a mapping was possible, and false otherwise.
    ''' </summary>
    Public Function TryMap(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)(<Out> ByRef dataframe As TypedDataFrameType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)) As Boolean
        If ColumnCount < 8 Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(0).CanMapTo(GetType(TCol1)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(1).CanMapTo(GetType(TCol2)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(2).CanMapTo(GetType(TCol3)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(3).CanMapTo(GetType(TCol4)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(4).CanMapTo(GetType(TCol5)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(5).CanMapTo(GetType(TCol6)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(6).CanMapTo(GetType(TCol7)) Then
            dataframe = Nothing
            Return False
        End If

        If Not Metadata.Columns(7).CanMapTo(GetType(TCol8)) Then
            dataframe = Nothing
            Return False
        End If

        dataframe = New TypedDataFrameType6(Of TCol1, TCol2, TCol3, TCol4, TCol5, TCol6, TCol7, TCol8)(Me)
        Return True
    End Function

    Private Function MakeDefaultFactory(Of TProxyType As New)() As Func(Of TProxyType)
        Return Function() New TProxyType()
    End Function

    Private Shared ReadOnly Row_UnsafeGetTranslated_Generic As MethodInfo = GetType(Row).GetMethod("UnsafeGetTranslated", BindingFlags.NonPublic Or BindingFlags.Instance)
    Private Function MakeMapper(Of TProxyType)(columnsToMembers As Dictionary(Of Long, MemberInfo)) As Func(Of Row, TProxyType, TProxyType)
        Dim nameBuilder = New StringBuilder()
        nameBuilder.Append("Mapper_")
        nameBuilder.Append(GetType(TProxyType).Name)
        For Each kv In columnsToMembers
            nameBuilder.Append("_")
            nameBuilder.Append(kv.Key)
            nameBuilder.Append("_")
            nameBuilder.Append(kv.Value.Name)
        Next

        Dim name = nameBuilder.ToString()
        Dim dyn = New DynamicMethod(name, GetType(TProxyType), {GetType(Row), GetType(TProxyType)}, restrictedSkipVisibility:=True)
        Dim il = dyn.GetILGenerator()

        Dim retLocal = il.DeclareLocal(GetType(TProxyType))
        Dim loadRetRef As Action = Sub()
                                       If GetType(TProxyType).IsValueType Then
                                           il.Emit(OpCodes.Ldloca, retLocal)              ' TProxyType*
                                       Else
                                           il.Emit(OpCodes.Ldloc, retLocal)               ' TProxyType
                                       End If
                                   End Sub
        Dim loadColumnValue As Action(Of Long) = Sub(translatedColumnIndex)
                                                     Dim goingToMember = columnsToMembers(translatedColumnIndex)
                                                     Dim type = If(TryCast(goingToMember, PropertyInfo)?.PropertyType, TryCast(goingToMember, FieldInfo)?.FieldType)

                                                     Dim unsafeGetTranslated = Row_UnsafeGetTranslated_Generic.MakeGenericMethod(type)
                                                     il.Emit(OpCodes.Ldarga_S, 0)                       ' Row*
                                                     il.Emit(OpCodes.Ldc_I8, translatedColumnIndex)     ' Row* long
                                                     il.Emit(OpCodes.Call, unsafeGetTranslated)         ' whatever-the-appropriate-type-is
                                                 End Sub

        il.Emit(OpCodes.Ldarg_1)                           ' TProxyType
        il.Emit(OpCodes.Stloc, retLocal)                   ' --empty--

        For Each kv In columnsToMembers
            Dim translatedColumnIndex = kv.Key
            Dim member = kv.Value

            loadRetRef()                                   ' TProxyType(*)?
            loadColumnValue(translatedColumnIndex)         ' TProxyType(*?) whatever-the-appropriate-type-is

            Dim asField = TryCast(member, FieldInfo)
            If asField IsNot Nothing Then
                il.Emit(OpCodes.Stfld, asField)            ' --empty--
            Else
                Dim setMtd = CType(member, PropertyInfo).SetMethod
                il.Emit(OpCodes.Call, setMtd)              ' --empty--
            End If
        Next

        il.Emit(OpCodes.Ldloc, retLocal)                   ' TProxyType
        il.Emit(OpCodes.Ret)                               ' --empty--

        Dim ret = CType(dyn.CreateDelegate(GetType(Func(Of Row, TProxyType, TProxyType))), Func(Of Row, TProxyType, TProxyType))
        Return ret
    End Function

    Private Function MakeProxy(Of TProxyType)(columnsToMembers As Dictionary(Of Long, MemberInfo), factory As Func(Of TProxyType)) As ProxyDataFrame(Of TProxyType)
        Dim mapper = MakeMapper(Of TProxyType)(columnsToMembers)

        Return New ProxyDataFrame(Of TProxyType)(Me, mapper, factory)
    End Function

    Private Function TryInferMapping(Of TProxyType)(membersInColumnOrder As String(), <Out> ByRef translatedColumnIndexToMemberMapping As Dictionary(Of Long, MemberInfo), <Out> ByRef errorMessage As String) As Boolean
        Dim suggested As Dictionary(Of Long, MemberInfo) = Nothing
        If Not TrySuggestMapping(Of TProxyType)(membersInColumnOrder, suggested, errorMessage) Then
            translatedColumnIndexToMemberMapping = Nothing
            Return False
        End If

        For Each kv In suggested
            Dim translatedColumnIndex = kv.Key
            Dim member = kv.Value

            Dim memberType = If(TryCast(member, PropertyInfo)?.PropertyType, TryCast(member, FieldInfo)?.FieldType)

            Dim columnDetails = Metadata.Columns(translatedColumnIndex)
            Dim columnType = columnDetails.Type
            If Not columnType.CanMapTo(memberType, columnDetails.CategoryLevels) Then
                translatedColumnIndexToMemberMapping = Nothing
                errorMessage = $"Cannot map column {UntranslateIndex(translatedColumnIndex):N0} ""{columnDetails.Name}"" to member {member.Name} of type {memberType.Name}"
                Return False
            End If
        Next

        translatedColumnIndexToMemberMapping = suggested
        errorMessage = Nothing
        Return True
    End Function

    Private Function TrySuggestMapping(Of TProxyType)(membersInColumnOrder As String(), <Out> ByRef translatedColumnIndexToMemberMapping As Dictionary(Of Long, MemberInfo), <Out> ByRef errorMessage As String) As Boolean
        Dim publicFieldsAndPropertiesList = GetType(TProxyType).GetMembers(BindingFlags.Public Or BindingFlags.Instance).Where(Function(m)
                                                                                                                                   Dim asField = TryCast(m, FieldInfo)
                                                                                                                                   If asField IsNot Nothing Then Return True

                                                                                                                                   Dim asProperty = TryCast(m, PropertyInfo)
                                                                                                                                   If asProperty IsNot Nothing AndAlso asProperty.SetMethod IsNot Nothing Then Return True

                                                                                                                                   Return False
                                                                                                                               End Function)

        Dim publicFieldsAndPropertiesLookup = publicFieldsAndPropertiesList.ToDictionary(Function(m) m.Name.ToLowerInvariant(), Function(m) m)

        Dim ret = New Dictionary(Of Long, MemberInfo)()

        If membersInColumnOrder IsNot Nothing AndAlso membersInColumnOrder.Length > 0 Then
            If membersInColumnOrder.Length > ColumnCount Then
                translatedColumnIndexToMemberMapping = Nothing
                errorMessage = $"Too many members listed in mapping, there are only {ColumnCount:N0} columns but found {membersInColumnOrder.Length} members in map"
                Return False
            End If

            For i = 0 To membersInColumnOrder.Length - 1
                Dim memberName = membersInColumnOrder(i)
                Dim pairedMember As MemberInfo
                If Not publicFieldsAndPropertiesLookup.TryGetValue(memberName.ToLowerInvariant(), pairedMember) Then
                    translatedColumnIndexToMemberMapping = Nothing
                    errorMessage = $"Could not find public member named {memberName} to map column {TranslateIndex(i):N0} to"
                    Return False
                End If

                ret(i) = pairedMember
            Next

            translatedColumnIndexToMemberMapping = ret
            errorMessage = Nothing
            Return True
        End If

        For i = 0 To ColumnCount - 1
            Dim columnName = Metadata.Columns(i).Name
            Dim pairedMember As MemberInfo
            If publicFieldsAndPropertiesLookup.TryGetValue(columnName.ToLowerInvariant(), pairedMember) Then
                ret(i) = pairedMember
            Else
                translatedColumnIndexToMemberMapping = Nothing
                errorMessage = $"Cannot infer mapping, there aren't public members (fields or properties) with matching names for each column"
                Return False
            End If
        Next

        translatedColumnIndexToMemberMapping = ret
        errorMessage = Nothing
        Return True
    End Function
End Class

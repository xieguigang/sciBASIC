#Region "Microsoft.VisualBasic::ff85d8c11739908dca1c3faff8baa77c, Data\BinaryData\BinaryData\XDR\Reading\ReadMapper.vb"

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

    '     Class ReadMapper
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: BuildDelegate, CheckedReadLength, CreateEnumReader, CreateFixArrayReader, CreateFixListReader
    '                   CreateLinkedListReader, CreateNullableReader, CreateVarArrayReader, CreateVarListReader, EnumRead
    '                   GetCacheType, ReadBool, ReadFixArray, ReadFixList, ReadFixOpaque
    '                   ReadLinkedList, ReadNullable, ReadOption, ReadString, ReadVarArray
    '                   ReadVarList, ReadVarOpaque
    ' 
    '         Sub: AppendBuildRequest, AppendMethod, BuildCaches, Init, LockedAppendMethod
    '              SetFix, SetOne, SetVar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.Xdr.EmitContexts

Namespace Xdr
    Public MustInherit Class ReadMapper
        Private _sync As Object = New Object()
        Private _dependencySync As Object = New Object()
        Private _dependency As Queue(Of BuildRequest) = New Queue(Of BuildRequest)()
        Private _builders As Dictionary(Of OpaqueType, Func(Of Type, [Delegate])()) = New Dictionary(Of OpaqueType, Func(Of Type, [Delegate])())()

        Protected Sub New()
        End Sub

        Protected Sub Init()
            Call SetOne(Function(r) New Void())
            SetOne(Function(r) DecodeInt32(r.ByteReader))
            SetOne(Function(r) DecodeUInt32(r.ByteReader))
            SetOne(Function(r) DecodeInt64(r.ByteReader))
            SetOne(Function(r) DecodeUInt64(r.ByteReader))
            SetOne(Of Single)(Function(r) XdrEncoding.DecodeSingle(r.ByteReader))
            SetOne(Of Double)(Function(r) XdrEncoding.DecodeDouble(r.ByteReader))
            Call SetOne(New ReadOneDelegate(Of Boolean)(AddressOf ReadBool))
            Call SetFix(New ReadManyDelegate(Of Byte())(AddressOf ReadFixOpaque))
            Call SetVar(New ReadManyDelegate(Of Byte())(AddressOf ReadVarOpaque))
            Call SetVar(New ReadManyDelegate(Of String)(AddressOf ReadString))
            _builders.Add(OpaqueType.One, New Func(Of Type, [Delegate])() {AddressOf CreateEnumReader, AddressOf CreateNullableReader, AddressOf CreateLinkedListReader, AddressOf GetReader})
            _builders.Add(OpaqueType.Fix, New Func(Of Type, [Delegate])() {AddressOf CreateFixArrayReader, AddressOf CreateFixListReader})
            _builders.Add(OpaqueType.Var, New Func(Of Type, [Delegate])() {AddressOf CreateVarArrayReader, AddressOf CreateVarListReader})
        End Sub

        Private Shared Function ReadBool(r As Reader) As Boolean
            Dim val = DecodeUInt32(r.ByteReader)
            If val = 0 Then Return False
            If val = 1 Then Return True
            Throw New InvalidOperationException("unexpected value: " & val.ToString())
        End Function

        Private Shared Function ReadFixOpaque(r As Reader, len As UInteger) As Byte()
            Dim result = r.ByteReader.Read(len)
            Dim tail = len Mod 4UI
            If tail <> 0 Then r.ByteReader.Read(4UI - tail)
            Return result
        End Function

        Private Shared Function ReadVarOpaque(r As Reader, max As UInteger) As Byte()
            Return ReadFixOpaque(r, CheckedReadLength(r, max))
        End Function

        Private Shared Function ReadString(r As Reader, max As UInteger) As String
            Return Encoding.ASCII.GetString(ReadVarOpaque(r, max))
        End Function

        Private Function BuildDelegate(methodType As OpaqueType, targetType As Type) As [Delegate]
            Dim wrap As Exception = Nothing

            Try

                For Each build In _builders(methodType)
                    Dim result = build(targetType)
                    If result IsNot Nothing Then Return result
                Next

            Catch ex As Exception
                wrap = New InvalidOperationException(String.Format("impossible to create a {0} method type for `{1}'", methodType, targetType.FullName), ex)
            End Try

            If wrap Is Nothing Then wrap = New NotImplementedException(String.Format("unknown type `{0}' in {1} method type", targetType.FullName, methodType))

            If methodType = OpaqueType.One Then
                Return ReadOneDelegate(targetType, wrap)
            Else
                Return ReadManyDelegate(targetType, wrap)
            End If
        End Function

        Protected Sub SetOne(Of T)(method As ReadOneDelegate(Of T))
            GetOneCacheType().MakeGenericType(GetType(T)).GetField("Instance").SetValue(Nothing, method)
        End Sub

        Protected Sub SetFix(Of T)(method As ReadManyDelegate(Of T))
            GetFixCacheType().MakeGenericType(GetType(T)).GetField("Instance").SetValue(Nothing, method)
        End Sub

        Protected Sub SetVar(Of T)(method As ReadManyDelegate(Of T))
            GetVarCacheType().MakeGenericType(GetType(T)).GetField("Instance").SetValue(Nothing, method)
        End Sub

        Private Function GetCacheType(methodType As OpaqueType) As Type
            Select Case methodType
                Case OpaqueType.One
                    Return GetOneCacheType()
                Case OpaqueType.Fix
                    Return GetFixCacheType()
                Case OpaqueType.Var
                    Return GetVarCacheType()
                Case Else
                    Throw New NotImplementedException("unknown opaque type")
            End Select
        End Function

        Protected MustOverride Function GetOneCacheType() As Type
        Protected MustOverride Function GetFixCacheType() As Type
        Protected MustOverride Function GetVarCacheType() As Type

        Public Sub BuildCaches()
            SyncLock _sync

                While True
                    Dim bReq As BuildRequest = Nothing

                    SyncLock _dependencySync
                        If _dependency.Count <> 0 Then bReq = _dependency.Dequeue()
                    End SyncLock

                    If bReq Is Nothing Then Return
                    Dim fi = GetCacheType(bReq.Method).MakeGenericType(bReq.TargetType).GetField("Instance")
                    If fi.GetValue(Nothing) Is Nothing Then fi.SetValue(Nothing, BuildDelegate(bReq.Method, bReq.TargetType))
                End While
            End SyncLock
        End Sub

        Friend Sub AppendMethod(targetType As Type, methodType As OpaqueType, method As [Delegate])
            SyncLock _sync
                LockedAppendMethod(targetType, methodType, method)
            End SyncLock
        End Sub

        Private Sub LockedAppendMethod(targetType As Type, methodType As OpaqueType, method As [Delegate])
            Dim fi = GetCacheType(methodType).MakeGenericType(targetType).GetField("Instance")
            If fi.GetValue(Nothing) IsNot Nothing Then Throw New InvalidOperationException("type already mapped")
            fi.SetValue(Nothing, method)
        End Sub

        Protected Sub AppendBuildRequest(targetType As Type, methodType As OpaqueType)
            SyncLock _dependencySync
                _dependency.Enqueue(New BuildRequest With {
                    .TargetType = targetType,
                    .Method = methodType
                })
            End SyncLock
        End Sub

        Public Shared Function CreateFixArrayReader(collectionType As Type) As [Delegate]
            Dim itemType As Type = collectionType.ArraySubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(ReadMapper).GetMethod("ReadFixArray").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(ReadManyDelegate(Of)).MakeGenericType(collectionType), mi)
        End Function

        Public Shared Function ReadFixArray(Of T)(r As Reader, len As UInteger) As T()
            Dim i As UInteger = 0

            Try
                Dim result = New T(len - 1) {}

                While i < len
                    result(i) = r.Read(Of T)()
                    i += 1
                End While

                Return result
            Catch ex As Exception
                Throw New FormatException(String.Format("cant't read {0} item", i), ex)
            End Try
        End Function

        Public Shared Function CreateLinkedListReader(collectionType As Type) As [Delegate]
            Dim itemType As Type = collectionType.ListSubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(ReadMapper).GetMethod("ReadLinkedList").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(ReadOneDelegate(Of)).MakeGenericType(collectionType), mi)
        End Function

        Public Shared Function ReadLinkedList(Of T)(r As Reader) As List(Of T)
            Dim result As List(Of T) = New List(Of T)()

            While ReadOption(r)

                Try
                    result.Add(r.Read(Of T)())
                Catch ex As Exception
                    Throw New FormatException(String.Format("cant't read {0} item", result.Count + 1), ex)
                End Try
            End While

            Return result
        End Function

        Public Shared Function CreateFixListReader(collectionType As Type) As [Delegate]
            Dim itemType As Type = collectionType.ListSubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(ReadMapper).GetMethod("ReadFixList").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(ReadManyDelegate(Of)).MakeGenericType(collectionType), mi)
        End Function

        Public Shared Function ReadFixList(Of T)(r As Reader, len As UInteger) As List(Of T)
            Dim i As UInteger = 0

            Try
                Dim result As List(Of T) = New List(Of T)()

                While i < len
                    result.Add(r.Read(Of T)())
                    i += 1
                End While

                Return result
            Catch ex As Exception
                Throw New FormatException(String.Format("cant't read {0} item", i), ex)
            End Try
        End Function

        Public Shared Function CreateEnumReader(targetType As Type) As [Delegate]
            If Not targetType.IsEnum Then Return Nothing
            Dim mi = GetType(ReadMapper).GetMethod("EnumRead").MakeGenericMethod(targetType)
            Return [Delegate].CreateDelegate(GetType(ReadOneDelegate(Of)).MakeGenericType(targetType), mi)
        End Function

        Public Shared Function EnumRead(Of T As Structure)(reader As Reader) As T
            Return EnumHelper(Of T).IntToEnum(reader.Read(Of Integer)())
        End Function

        Public Shared Function CreateNullableReader(targetType As Type) As [Delegate]
            Dim itemType As Type = targetType.NullableSubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(ReadMapper).GetMethod("ReadNullable").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(ReadOneDelegate(Of)).MakeGenericType(targetType), mi)
        End Function

        Public Shared Function ReadNullable(Of T As Structure)(reader As Reader) As T?
            Dim exist = ReadOption(reader)

            Try

                If exist Then
                    Return reader.Read(Of T)()
                Else
                    Return Nothing
                End If

            Catch ex As SystemException
                Throw New FormatException("cant't read 'value'", ex)
            End Try
        End Function

        Private Shared Function ReadOption(reader As Reader) As Boolean
            Try
                Return reader.Read(Of Boolean)()
            Catch ex As SystemException
                Throw New FormatException("cant't read 'option'", ex)
            End Try
        End Function

        Public Shared Function CreateVarArrayReader(collectionType As Type) As [Delegate]
            Dim itemType As Type = collectionType.ArraySubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(ReadMapper).GetMethod("ReadVarArray").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(ReadManyDelegate(Of)).MakeGenericType(collectionType), mi)
        End Function

        Public Shared Function ReadVarArray(Of T)(r As Reader, max As UInteger) As T()
            Return ReadFixArray(Of T)(r, CheckedReadLength(r, max))
        End Function

        Public Shared Function CreateVarListReader(collectionType As Type) As [Delegate]
            Dim itemType As Type = collectionType.ListSubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(ReadMapper).GetMethod("ReadVarList").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(ReadManyDelegate(Of)).MakeGenericType(collectionType), mi)
        End Function

        Public Shared Function ReadVarList(Of T)(r As Reader, max As UInteger) As List(Of T)
            Return ReadFixList(Of T)(r, CheckedReadLength(r, max))
        End Function

        Private Shared Function CheckedReadLength(r As Reader, max As UInteger) As UInteger
            Dim len As UInteger

            Try
                len = DecodeUInt32(r.ByteReader)
            Catch ex As SystemException
                Throw New FormatException("cant't read 'length'", ex)
            End Try

            If len > max Then Throw New FormatException("unexpected length: " & len.ToString())
            Return len
        End Function
    End Class
End Namespace


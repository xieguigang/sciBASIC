#Region "Microsoft.VisualBasic::313d875a366955af6a1292fd1c1e144c, Data\BinaryData\BinaryData\XDR\Writing\WriteMapper.vb"

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

    '     Class WriteMapper
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: BuildDelegate, CreateEnumWriter, CreateFixArrayWriter, CreateFixListWriter, CreateLinkedListWriter
    '                   CreateNullableWriter, CreateVarArrayWriter, CreateVarListWriter, GetCacheType
    ' 
    '         Sub: AppendBuildRequest, AppendMethod, BuildCaches, EnumWriter, Init
    '              LockedAppendMethod, NoCheckWriteFixOpaque, SetFix, SetOne, SetVar
    '              WriteBool, WriteFixArray, WriteFixList, WriteFixOpaque, WriteLinkedList
    '              WriteNullable, WriteOption, WriteString, WriteVarArray, WriteVarList
    '              WriteVarOpaque
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.Xdr.EmitContexts

Namespace Xdr
    Public MustInherit Class WriteMapper
        Private _sync As Object = New Object()
        Private _dependencySync As Object = New Object()
        Private _dependency As Queue(Of BuildRequest) = New Queue(Of BuildRequest)()
        Private _builders As Dictionary(Of OpaqueType, Func(Of Type, [Delegate])()) = New Dictionary(Of OpaqueType, Func(Of Type, [Delegate])())()

        Protected Sub New()
        End Sub

        Protected Sub Init()
            SetOne(Of Void)(Sub(w, i)
                            End Sub)
            SetOne(Of Integer)(Sub(w, i) EncodeInt32(i, w.ByteWriter))
            SetOne(Of UInteger)(Sub(w, i) EncodeUInt32(i, w.ByteWriter))
            SetOne(Of Long)(Sub(w, i) EncodeInt64(i, w.ByteWriter))
            SetOne(Of ULong)(Sub(w, i) EncodeUInt64(i, w.ByteWriter))
            SetOne(Of Single)(Sub(w, i) XdrEncoding.EncodeSingle(i, w.ByteWriter))
            SetOne(Of Double)(Sub(w, i) XdrEncoding.EncodeDouble(i, w.ByteWriter))
            Call SetOne(New WriteOneDelegate(Of Boolean)(AddressOf WriteBool))
            Call SetFix(New WriteManyDelegate(Of Byte())(AddressOf WriteFixOpaque))
            Call SetVar(New WriteManyDelegate(Of Byte())(AddressOf WriteVarOpaque))
            Call SetVar(New WriteManyDelegate(Of String)(AddressOf WriteString))
            _builders.Add(OpaqueType.One, New Func(Of Type, [Delegate])() {AddressOf CreateEnumWriter, AddressOf CreateNullableWriter, AddressOf CreateLinkedListWriter, AddressOf GetWriter})
            _builders.Add(OpaqueType.Fix, New Func(Of Type, [Delegate])() {AddressOf CreateFixArrayWriter, AddressOf CreateFixListWriter})
            _builders.Add(OpaqueType.Var, New Func(Of Type, [Delegate])() {AddressOf CreateVarArrayWriter, AddressOf CreateVarListWriter})
        End Sub

        Private Shared Sub WriteBool(w As Writer, v As Boolean)
            w.Write(If(v, 1, 0))
        End Sub

        Private Shared _tails As Byte()() = New Byte()() {Nothing, New Byte() {&H00}, New Byte() {&H00, &H00}, New Byte() {&H00, &H00, &H00}}

        Private Shared Sub WriteFixOpaque(w As Writer, len As UInteger, v As Byte())
            If v.LongLength <> len Then Throw New FormatException("unexpected length: " & v.LongLength.ToString())
            NoCheckWriteFixOpaque(w, len, v)
        End Sub

        Private Shared Sub WriteVarOpaque(w As Writer, max As UInteger, v As Byte())
            Dim len As UInteger = v.LongLength
            If len > max Then Throw New FormatException("unexpected length: " & len.ToString())

            Try
                w.Write(len)
            Catch ex As SystemException
                Throw New FormatException("can't write length", ex)
            End Try

            NoCheckWriteFixOpaque(w, len, v)
        End Sub

        Private Shared Sub NoCheckWriteFixOpaque(w As Writer, len As UInteger, v As Byte())
            Try
                w.ByteWriter.Write(v)
                Dim tail = len Mod 4UI
                If tail <> 0 Then w.ByteWriter.Write(_tails(4UI - tail))
            Catch ex As SystemException
                Throw New FormatException("can't write byte array", ex)
            End Try
        End Sub

        Private Shared Sub WriteString(w As Writer, max As UInteger, v As String)
            WriteVarOpaque(w, max, Encoding.ASCII.GetBytes(v))
        End Sub

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
                Return WriteOneDelegate(targetType, wrap)
            Else
                Return WriteManyDelegate(targetType, wrap)
            End If
        End Function

        Protected Sub SetOne(Of T)(method As WriteOneDelegate(Of T))
            GetOneCacheType().MakeGenericType(GetType(T)).GetField("Instance").SetValue(Nothing, method)
        End Sub

        Protected Sub SetFix(Of T)(method As WriteManyDelegate(Of T))
            GetFixCacheType().MakeGenericType(GetType(T)).GetField("Instance").SetValue(Nothing, method)
        End Sub

        Protected Sub SetVar(Of T)(method As WriteManyDelegate(Of T))
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

        Public Shared Function CreateFixArrayWriter(collectionType As Type) As [Delegate]
            Dim itemType As Type = collectionType.ArraySubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(WriteMapper).GetMethod("WriteFixArray").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(WriteManyDelegate(Of)).MakeGenericType(collectionType), mi)
        End Function

        Public Shared Sub WriteFixArray(Of T)(w As Writer, len As UInteger, val As T())
            If val.LongLength <> len Then Throw New FormatException("unexpected length: " & val.LongLength.ToString())
            Dim i As UInteger = 0

            Try

                While i < len
                    w.Write(val(i))
                    i += 1
                End While

            Catch ex As SystemException
                Throw New FormatException(String.Format("can't write {0} item", i), ex)
            End Try
        End Sub

        Public Shared Function CreateLinkedListWriter(collectionType As Type) As [Delegate]
            Dim itemType As Type = collectionType.ListSubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(WriteMapper).GetMethod("WriteLinkedList").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(WriteOneDelegate(Of)).MakeGenericType(collectionType), mi)
        End Function

        Public Shared Sub WriteLinkedList(Of T)(w As Writer, val As List(Of T))
            For i = 0 To val.Count - 1
                WriteOption(w, True)

                Try
                    w.Write(val(i))
                Catch ex As SystemException
                    Throw New FormatException(String.Format("can't write {0} item", i), ex)
                End Try
            Next

            WriteOption(w, False)
        End Sub

        Public Shared Function CreateFixListWriter(collectionType As Type) As [Delegate]
            Dim itemType As Type = collectionType.ListSubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(WriteMapper).GetMethod("WriteFixList").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(WriteManyDelegate(Of)).MakeGenericType(collectionType), mi)
        End Function

        Public Shared Sub WriteFixList(Of T)(w As Writer, len As UInteger, val As List(Of T))
            If val.Count <> len Then Throw New FormatException("unexpected length: " & val.Count.ToString())
            Dim i = 0

            Try

                While i < val.Count
                    w.Write(val(i))
                    i += 1
                End While

            Catch ex As SystemException
                Throw New FormatException(String.Format("can't write {0} item", i), ex)
            End Try
        End Sub

        Public Shared Function CreateEnumWriter(targetType As Type) As [Delegate]
            If Not targetType.IsEnum Then Return Nothing
            Dim mi = GetType(WriteMapper).GetMethod("EnumWriter").MakeGenericMethod(targetType)
            Return [Delegate].CreateDelegate(GetType(WriteOneDelegate(Of)).MakeGenericType(targetType), mi)
        End Function

        Public Shared Sub EnumWriter(Of T As Structure)(writer As Writer, val As T)
            writer.Write(EnumHelper(Of T).EnumToInt(val))
        End Sub

        Public Shared Function CreateNullableWriter(targetType As Type) As [Delegate]
            Dim itemType As Type = targetType.NullableSubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(WriteMapper).GetMethod("WriteNullable").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(WriteOneDelegate(Of)).MakeGenericType(targetType), mi)
        End Function

        Public Shared Sub WriteNullable(Of T As Structure)(writer As Writer, val As T?)
            WriteOption(writer, val.HasValue)
            If Not val.HasValue Then Return

            Try
                writer.Write(val.Value)
            Catch ex As SystemException
                Throw New FormatException("can't write value", ex)
            End Try
        End Sub

        Private Shared Sub WriteOption(writer As Writer, val As Boolean)
            Try
                writer.Write(val)
            Catch ex As SystemException
                Throw New FormatException("can't write option", ex)
            End Try
        End Sub

        Public Shared Function CreateVarArrayWriter(collectionType As Type) As [Delegate]
            Dim itemType As Type = collectionType.ArraySubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(WriteMapper).GetMethod("WriteVarArray").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(WriteManyDelegate(Of)).MakeGenericType(collectionType), mi)
        End Function

        Public Shared Sub WriteVarArray(Of T)(w As Writer, max As UInteger, val As T())
            Dim len As UInteger = val.LongLength
            If len > max Then Throw New FormatException("unexpected length: " & len.ToString())

            Try
                w.Write(len)
            Catch ex As SystemException
                Throw New FormatException("can't write length", ex)
            End Try

            Dim i As UInteger = 0

            Try

                While i < len
                    w.Write(val(i))
                    i += 1
                End While

            Catch ex As SystemException
                Throw New FormatException(String.Format("can't write {0} item", i), ex)
            End Try
        End Sub

        Public Shared Function CreateVarListWriter(collectionType As Type) As [Delegate]
            Dim itemType As Type = collectionType.ListSubType()
            If itemType Is Nothing Then Return Nothing
            Dim mi = GetType(WriteMapper).GetMethod("WriteVarList").MakeGenericMethod(itemType)
            Return [Delegate].CreateDelegate(GetType(WriteManyDelegate(Of)).MakeGenericType(collectionType), mi)
        End Function

        Public Shared Sub WriteVarList(Of T)(w As Writer, max As UInteger, val As List(Of T))
            Dim len = val.Count
            If len > max Then Throw New FormatException("unexpected length: " & len.ToString())

            Try
                w.Write(Of UInteger)(len)
            Catch ex As SystemException
                Throw New FormatException("can't write length", ex)
            End Try

            Dim i = 0

            Try

                While i < len
                    w.Write(val(i))
                    i += 1
                End While

            Catch ex As SystemException
                Throw New FormatException(String.Format("can't write {0} item", i), ex)
            End Try
        End Sub
    End Class
End Namespace


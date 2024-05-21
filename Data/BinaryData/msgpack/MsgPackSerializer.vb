#Region "Microsoft.VisualBasic::47c2f542cf1efa9f5a331efdf71337a4, Data\BinaryData\msgpack\MsgPackSerializer.vb"

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

    '   Total Lines: 397
    '    Code Lines: 317
    ' Comment Lines: 7
    '   Blank Lines: 73
    '     File Size: 16.36 KB


    ' Class MsgPackSerializer
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: (+8 Overloads) Deserialize, (+2 Overloads) DeserializeObject, DeserializeObjectType, GetInfo, GetSerializer
    '               IsGenericDictionary, IsGenericList, IsSerializableGenericCollection, (+2 Overloads) Serialize, (+2 Overloads) SerializeObject
    ' 
    '     Sub: (+2 Overloads) BuildMap, (+2 Overloads) Serialize, (+2 Overloads) SerializeObject
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Constants
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization.Reflection
Imports Microsoft.VisualBasic.Language
Imports TypeInfo = Microsoft.VisualBasic.Data.IO.MessagePack.Serialization.Reflection.TypeInfo

Public Class MsgPackSerializer

    Public Shared ReadOnly DefaultContext As New SerializationContext()

    Shared ReadOnly typeInfos As New Dictionary(Of Type, TypeInfo)()

    Private propsByName As Dictionary(Of String, SerializableProperty)
    Private props As List(Of SerializableProperty)
    Private serializedType As Type

    Public Sub New(type As Type)
        serializedType = type
        BuildMap()
    End Sub

    Public Sub New(type As Type, propertyDefinitions As IList(Of MessagePackMemberDefinition))
        serializedType = type
        BuildMap(propertyDefinitions)
    End Sub

    Private Shared Function GetInfo(type As Type) As TypeInfo
        Dim info As TypeInfo = Nothing

#If MTA Then
        ' thread unsafe

        If Not typeInfos.TryGetValue(type, info) Then
            info = New TypeInfo(type)
            typeInfos(type) = info
        End If
#Else
        ' thread safe for winform application

        ' 20240511 typeInfos is a global shared object that contains the type
        ' schema cache for read messagepack data. multiple thread may cased
        ' the null reference error at here.
        ' due to the reason of typeinfos collection modification problem
        ' add synlock for avoid such multiple thread error.
        SyncLock typeInfos
            If Not typeInfos.TryGetValue(type, info) Then
                info = New TypeInfo(type)
                typeInfos(type) = info
            End If
        End SyncLock
#End If

        Return info
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function IsGenericList(type As Type) As Boolean
        Return GetInfo(type).IsGenericList
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function IsGenericDictionary(type As Type) As Boolean
        Return GetInfo(type).IsGenericDictionary
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function IsSerializableGenericCollection(type As Type) As Boolean
        Return GetInfo(type).IsSerializableGenericCollection
    End Function

    <DebuggerStepThrough>
    Private Shared Function GetSerializer(t As Type) As MsgPackSerializer
        Dim result As MsgPackSerializer = Nothing

        SyncLock DefaultContext._serializers
            If Not DefaultContext._serializers.TryGetValue(t, result) Then
                result = New MsgPackSerializer(t)
                DefaultContext._serializers(t) = result
            End If
        End SyncLock

        Return result
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function SerializeObject(o As Object) As Byte()
        Return GetSerializer(o.GetType()).Serialize(o)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Sub SerializeObject(o As Object, file As Stream, Optional closeFile As Boolean = False)
        Call GetSerializer(o.GetType()).Serialize(o, file)
        Call file.Flush()

        If closeFile Then
            Call file.Dispose()
        End If
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function SerializeObject(o As Object, buffer As Byte(), offset As Integer) As Integer
        Return GetSerializer(o.GetType()).Serialize(o, buffer, offset)
    End Function

    Public Sub Serialize(o As Object, stream As Stream)
        Dim writer As New BinaryWriter(stream)

        Call Serialize(o, writer)
        Call writer.Flush()
    End Sub

    Public Function Serialize(o As Object) As Byte()
        Using stream As New MemoryStream()
            Call Serialize(o, stream)
            Return stream.ToArray
        End Using
    End Function

    Public Function Serialize(o As Object, buffer As Byte(), offset As Integer) As Integer
        Dim endPos = 0

        Using stream As MemoryStream = New MemoryStream(buffer)
            Using writer As BinaryWriter = New BinaryWriter(stream)
                stream.Seek(offset, SeekOrigin.Begin)
                Serialize(o, writer)
                endPos = CInt(stream.Position)
            End Using
        End Using

        Return endPos
    End Function

    Public Shared Function Deserialize(Of T)(buffer As Stream) As T
        Using reader As New BinaryDataReader(buffer)
            Dim o = DeserializeObjectType(GetType(T), reader)
            Return Convert.ChangeType(o, GetType(T))
        End Using
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Deserialize(Of T)(buffer As Byte()) As T
        Return Deserialize(Of T)(New MemoryStream(buffer))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Deserialize(t As Type, buffer As Byte()) As Object
        Return Deserialize(t, buffer, 0)
    End Function

    Public Shared Function Deserialize(t As Type, buffer As Byte(), offset As Integer) As Object
        Using stream As New MemoryStream(buffer)
            Call stream.Seek(offset, SeekOrigin.Begin)

            Using reader As New BinaryDataReader(stream)
                Dim o = DeserializeObjectType(t, reader)
                Return Convert.ChangeType(o, t)
            End Using
        End Using
    End Function

    Public Shared Function Deserialize(t As Type, buffer As Stream) As Object
        Using reader As New BinaryDataReader(buffer)
            Dim o = DeserializeObjectType(t, reader)
            Return Convert.ChangeType(o, t)
        End Using
    End Function

    Public Shared Function DeserializeObject(o As Object, buffer As Byte(), offset As Integer) As Integer
        Dim numRead = 0

        Using stream As MemoryStream = New MemoryStream(buffer)
            stream.Seek(offset, SeekOrigin.Begin)

            Using reader As BinaryDataReader = New BinaryDataReader(stream)
                GetSerializer(o.GetType()).Deserialize(o, reader)
                numRead = CInt(stream.Position)
            End Using
        End Using

        Return numRead
    End Function

    Friend Shared Function DeserializeObject(o As Object, reader As BinaryDataReader, Optional nilImplication As NilImplication = NilImplication.MemberDefault) As Object
        Dim list = TryCast(o, IList)

        If list IsNot Nothing Then
            Return If(DeserializeCollection(list, reader), Nothing, o)
        End If

        Dim dictionary = TryCast(o, IDictionary)

        If dictionary IsNot Nothing Then
            Return If(DeserializeCollection(dictionary, reader), Nothing, o)
        End If

        Return GetSerializer(o.GetType()).Deserialize(o, reader)
    End Function

    Friend Shared Function DeserializeObjectType(type As Type, reader As BinaryDataReader, Optional nilImplication As NilImplication = NilImplication.MemberDefault) As Object
        If type.IsPrimitive OrElse type Is GetType(String) OrElse IsSerializableGenericCollection(type) Then
            Return DeserializeValue(type, reader, nilImplication)
        End If

        Dim constructorInfo = type.GetConstructor(Type.EmptyTypes)

        If constructorInfo Is Nothing Then
            Throw New ApplicationException($"Can't deserialize Type [{type}] in MsgPackSerializer because it has no default constructor")
        Else
            Return GetSerializer(type).Deserialize(constructorInfo.Invoke(SerializableProperty.EmptyObjArgs), reader)
        End If
    End Function

    Public Shared Function Deserialize(buffer As Stream) As Object
        Using reader As New BinaryDataReader(buffer)
            Return GetSerializer(GetType(Object)).Deserialize(Nothing, reader)
        End Using
    End Function

    Public Shared Function Deserialize(buffer As Byte(), Optional offset As Integer = Scan0) As Object
        Using stream As New MemoryStream(buffer)
            stream.Seek(offset, SeekOrigin.Begin)
            Return Deserialize(stream)
        End Using
    End Function

    Friend Function Deserialize(result As Object, reader As BinaryDataReader) As Object
        Dim header As Byte = reader.ReadByte()

        If header = MsgPackFormats.NIL Then
            result = Nothing
        Else
            If DefaultContext.SerializationMethod = SerializationMethod.Array Then
                If header = MsgPackFormats.ARRAY_16 Then
                    reader.ReadByte()
                    reader.ReadByte()
                ElseIf header = MsgPackFormats.ARRAY_32 Then
                    reader.ReadByte()
                    reader.ReadByte()
                    reader.ReadByte()
                    reader.ReadByte()
                ElseIf header < FixedArray.MIN OrElse header > FixedArray.MAX Then
                    Throw New ApplicationException("The serialized array format isn't valid for header [" & header & "]")
                End If

                For Each prop In props
                    prop.Deserialize(result, reader)
                Next
            Else
                Dim numElements As Integer

                If header >= FixedMap.MIN AndAlso header <= FixedMap.MAX Then
                    numElements = header And &HF
                ElseIf header = MsgPackFormats.MAP_16 Then
                    numElements = (CInt(reader.ReadByte) << 8) + reader.ReadByte()
                ElseIf header = MsgPackFormats.MAP_32 Then
                    numElements = (CInt(reader.ReadByte) << 24) + (CInt(reader.ReadByte) << 16) + (CInt(reader.ReadByte) << 8) + reader.ReadByte()
                Else
                    Throw New ApplicationException("The serialized map format isn't valid")
                End If

                For i As Integer = 0 To numElements - 1
                    Dim propName = CStr(ReadMsgPackString(reader, NilImplication.Null))
                    Dim propToProcess As SerializableProperty = Nothing

                    If propsByName.TryGetValue(propName, propToProcess) Then
                        propToProcess.Deserialize(result, reader)
                    End If
                Next
            End If
        End If

        Return result
    End Function

    Friend Shared Sub SerializeObject(o As Object, writer As BinaryWriter)
        GetSerializer(o.GetType()).Serialize(o, writer)
    End Sub

    Private Sub Serialize(o As Object, writer As BinaryWriter)
        If o Is Nothing Then
            writer.Write(MsgPackFormats.NIL)
        ElseIf serializedType.IsPrimitive OrElse serializedType Is GetType(String) OrElse IsSerializableGenericCollection(serializedType) Then
            SerializeValue(o, writer, DefaultContext.SerializationMethod)
        ElseIf DefaultContext.SerializationMethod = SerializationMethod.Map Then
            If props.Count <= 15 Then
                Dim arrayVal As Byte = FixedMap.MIN + props.Count
                writer.Write(arrayVal)
            ElseIf props.Count <= UShort.MaxValue Then
                writer.Write(MsgPackFormats.MAP_16)
                Dim data = BitConverter.GetBytes(CUShort(props.Count))
                If BitConverter.IsLittleEndian Then Array.Reverse(data)
                writer.Write(data)
            Else
                writer.Write(MsgPackFormats.MAP_32)
                Dim data = BitConverter.GetBytes(CUInt(props.Count))
                If BitConverter.IsLittleEndian Then Array.Reverse(data)
                writer.Write(data)
            End If

            For Each prop In props
                WriteMsgPack(writer, prop.name)
                prop.Serialize(o, writer, DefaultContext.SerializationMethod)
            Next
        Else
            If props.Count <= 15 Then
                Dim arrayVal As Byte = FixedArray.MIN + props.Count
                writer.Write(arrayVal)
            ElseIf props.Count <= UShort.MaxValue Then
                writer.Write(MsgPackFormats.ARRAY_16)
                Dim data = BitConverter.GetBytes(CUShort(props.Count))
                If BitConverter.IsLittleEndian Then Array.Reverse(data)
                writer.Write(data)
            Else
                writer.Write(MsgPackFormats.ARRAY_32)
                Dim data = BitConverter.GetBytes(CUInt(props.Count))
                If BitConverter.IsLittleEndian Then Array.Reverse(data)
                writer.Write(data)
            End If

            For Each prop As SerializableProperty In props
                prop.Serialize(o, writer, DefaultContext.SerializationMethod)
            Next
        End If
    End Sub

    Private Sub BuildMap()
        If Not serializedType.IsPrimitive AndAlso serializedType IsNot GetType(String) AndAlso Not IsSerializableGenericCollection(serializedType) Then
            props = New List(Of SerializableProperty)()
            propsByName = New Dictionary(Of String, SerializableProperty)()

            For Each prop In serializedType.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                If prop.CanRead = False OrElse prop.CanWrite = False Then
                    Continue For
                End If

                Dim serializableProp As SerializableProperty = Nothing

                If DefaultContext.SerializationMethod = SerializationMethod.Map Then
                    serializableProp = New SerializableProperty(prop)
                Else
                    Dim customAttributes = prop.GetCustomAttributes(GetType(MessagePackMemberAttribute), True)

                    If customAttributes.Length = 1 Then
                        Dim att = CType(customAttributes(0), MessagePackMemberAttribute)
                        serializableProp = New SerializableProperty(prop, att.Id, att.NilImplication)
                    End If
                End If

                If serializableProp IsNot Nothing Then
                    props.Add(serializableProp)
                    propsByName(serializableProp.name) = serializableProp
                End If
            Next

            If DefaultContext.SerializationMethod = SerializationMethod.Array Then
                props.Sort(Function(x, y) x.sequence.CompareTo(y.sequence))
            End If
        End If
    End Sub

    Private Sub BuildMap(propertyDefinitions As IList(Of MessagePackMemberDefinition))
        If Not serializedType.IsPrimitive AndAlso serializedType IsNot GetType(String) AndAlso Not IsSerializableGenericCollection(serializedType) Then
            Dim id As i32 = 1

            props = New List(Of SerializableProperty)()
            propsByName = New Dictionary(Of String, SerializableProperty)()

            For Each def In propertyDefinitions
                Dim prop = serializedType.GetProperty(def.PropertyName)

                If prop Is Nothing OrElse Not prop.CanRead OrElse Not prop.CanWrite Then
                    Continue For
                End If

                Dim serializableProp As SerializableProperty = Nothing

                If DefaultContext.SerializationMethod = SerializationMethod.Map Then
                    serializableProp = New SerializableProperty(prop)
                Else
                    serializableProp = New SerializableProperty(prop, ++id, def.NilImplication)
                End If

                If serializableProp IsNot Nothing Then
                    props.Add(serializableProp)
                    propsByName(serializableProp.name) = serializableProp
                End If
            Next

            If DefaultContext.SerializationMethod = SerializationMethod.Array Then
                props.Sort(Function(x, y) x.sequence.CompareTo(y.sequence))
            End If
        End If
    End Sub
End Class

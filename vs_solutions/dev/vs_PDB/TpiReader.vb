#Region "Microsoft.VisualBasic::00000000000000000000000000000000, sciBASIC#\vs_solutions\dev\vs_PDB\TpiReader.vb"

    ' Copyright (c) 2018 GPL3 Licensed
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

#End Region

Imports System.Text

Namespace sciBASIC.PDB

    ''' <summary>
    ''' Parses the TPI (type information) stream (stream #2) of a classic PDB and decodes the
    ''' CodeView type records (<c>LF_*</c> leaves) into <see cref="TypeRecord"/> objects.
    ''' Type ids start at <c>TypeIndexBegin</c> (usually 0x1000); each leaf record consumes one id.
    ''' </summary>
    Public Class TpiReader

        ''' <summary>Decoded type records, in stream order.</summary>
        Public ReadOnly Property TypeRecords As New List(Of TypeRecord)()

        ''' <summary>TPI stream version word.</summary>
        Public ReadOnly Property PdbStreamVersion As Integer

        ''' <summary>First type index carried by this stream.</summary>
        Public ReadOnly Property TypeIndexBegin As UInteger

        ''' <summary>One past the last type index carried by this stream.</summary>
        Public ReadOnly Property TypeIndexEnd As UInteger

        ''' <summary>Resolved display name per type index (primitives, named types).</summary>
        Private ReadOnly names As New Dictionary(Of UInteger, String)()

        ''' <summary>Member-name list per LF_FIELDLIST type index.</summary>
        Private ReadOnly fieldMembers As New Dictionary(Of UInteger, List(Of String))()

        Sub New(tpi As Stream)
            Dim data As Byte() = tpi.GetBytes()
            Parse(data)
        End Sub

        Private Sub Parse(data As Byte())
            If data.Length < 8 Then
                Return
            End If

            PdbStreamVersion = BitConverter.ToInt32(data, 0)
            Dim headerSize As Integer = BitConverter.ToInt32(data, 4)
            TypeIndexBegin = BitConverter.ToUInt32(data, 8)
            TypeIndexEnd = BitConverter.ToUInt32(data, 12)

            If TypeIndexBegin = 0 Then
                TypeIndexBegin = &H1000
            End If

            ' Records follow the header. HeaderSize is the size of the header (including the
            ' version word); fall back to 8 when the value looks implausible.
            Dim recStart As Integer

            If headerSize > 4 AndAlso headerSize < data.Length Then
                recStart = headerSize
            Else
                recStart = 8
            End If

            Dim current As UInteger = TypeIndexBegin

            For Each rec As CvRecord In CodeView.Enumerate(data, recStart, data.Length - recStart)
                Try
                    DecodeRecord(rec, current)
                Catch
                    ' Skip a malformed type record.
                End Try

                current += 1UI
            Next
        End Sub

        Private Sub DecodeRecord(rec As CvRecord, typeId As UInteger)
            Dim p As Byte() = rec.Payload
            Dim leaf As UShort = rec.Type
            Dim tr As New TypeRecord With {.TypeId = typeId}

            Select Case leaf
                Case CodeView.LF_MODIFIER
                    ' u32 modifiedType; u16 modifier
                    If p.Length < 6 Then Return
                    Dim underlying As UInteger = BitConverter.ToUInt32(p, 0)
                    tr.Kind = TypeKind.Typedef
                    tr.Name = ResolveTypeName(underlying)

                Case CodeView.LF_POINTER
                    ' u32 underlyingType; u32 attr (high 3 bits = pointer size in bytes: 1/2/4/8)
                    If p.Length < 8 Then Return
                    Dim underlying As UInteger = BitConverter.ToUInt32(p, 0)
                    Dim attr As UInteger = BitConverter.ToUInt32(p, 4)
                    Dim ptrSize As Integer = 1 << CInt((attr >> 29) And &H7)
                    tr.Kind = TypeKind.Pointer
                    tr.Size = CType(If(ptrSize <= 0, 0, ptrSize), UInteger)
                    tr.Name = ResolveTypeName(underlying) & "*"

                Case CodeView.LF_PROCEDURE
                    ' u32 retType; u8 callConv; u8 funcAttr; u16 parmCount; u32 argList
                    If p.Length < 4 Then Return
                    Dim ret As UInteger = BitConverter.ToUInt32(p, 0)
                    tr.Kind = TypeKind.FunctionType
                    tr.Name = ResolveTypeName(ret) & "()"

                Case CodeView.LF_ARRAY
                    ' u32 elementType; u32 indexType; numeric leaf (size); name
                    If p.Length < 8 Then Return
                    Dim elem As UInteger = BitConverter.ToUInt32(p, 0)
                    tr.Kind = TypeKind.Array
                    tr.Name = ResolveTypeName(elem) & "[]"

                Case CodeView.LF_CLASS, CodeView.LF_STRUCTURE, CodeView.LF_UNION, CodeView.LF_ENUM
                    DecodeNamedAggregate(p, leaf, tr)

                Case CodeView.LF_FIELDLIST
                    ' Member list only; keep the member names keyed by this type id.
                    Dim members As List(Of String) = DecodeFieldList(p)
                    fieldMembers(typeId) = members
                    Return

                Case Else
                    Return
            End Select

            If Not String.IsNullOrEmpty(tr.Name) Then
                names(typeId) = tr.Name
            End If

            TypeRecords.Add(tr)
        End Sub

        Private Sub DecodeNamedAggregate(p As Byte(), leaf As UShort, tr As TypeRecord)
            ' New (32-bit type index) layout:
            '   u16 property
            '   u32 fieldList        (LF_FIELDLIST type index)
            '   [u32 derived]        (class/struct only)
            '   [u32 vshape]         (class/struct only)
            '   u32 size
            '   name (null-terminated)
            If p.Length < 2 Then
                Return
            End If

            Dim [property] As UShort = BitConverter.ToUInt16(p, 0)
            Dim nameOff As Integer
            Dim size As UInteger = 0

            Select Case leaf
                Case CodeView.LF_UNION
                    If p.Length >= 10 Then
                        size = BitConverter.ToUInt32(p, 6)
                    End If
                    nameOff = 10

                Case CodeView.LF_ENUM
                    If p.Length >= 10 Then
                        size = 0
                    End If
                    nameOff = 10

                Case Else ' LF_CLASS / LF_STRUCTURE
                    If p.Length >= 18 Then
                        size = BitConverter.ToUInt32(p, 14)
                    End If
                    nameOff = 18
            End Select

            If nameOff > p.Length Then
                nameOff = p.Length
            End If

            Dim name As String = CodeView.ReadNullString(p, nameOff, Encoding.UTF8)

            Select Case leaf
                Case CodeView.LF_CLASS : tr.Kind = TypeKind.Class
                Case CodeView.LF_STRUCTURE : tr.Kind = TypeKind.Structure
                Case CodeView.LF_UNION : tr.Kind = TypeKind.Structure
                Case CodeView.LF_ENUM : tr.Kind = TypeKind.Enum
            End Select

            tr.Size = size

            If [property] And &H2 Then
                ' Forward reference: no real name yet.
                tr.Name = If(String.IsNullOrEmpty(name), $"fwd_{tr.TypeId:X}", name)
            Else
                tr.Name = If(String.IsNullOrEmpty(name), $"type_{tr.TypeId:X}", name)
            End If

            ' Attach member names resolved from the field list.
            Dim fieldList As UInteger

            If leaf = CodeView.LF_ENUM Then
                fieldList = If(p.Length >= 10, BitConverter.ToUInt32(p, 6), 0UI)
            Else
                fieldList = If(p.Length >= 6, BitConverter.ToUInt32(p, 2), 0UI)
            End If

            Dim members As List(Of String) = Nothing

            If fieldMembers.TryGetValue(fieldList, members) Then
                tr.Fields.AddRange(members)
            End If
        End Sub

        ''' <summary>
        ''' Decode the sub-records of an LF_FIELDLIST, collecting member / method / nested-type names.
        ''' </summary>
        Private Function DecodeFieldList(p As Byte()) As List(Of String)
            Dim members As New List(Of String)()
            Dim pos As Integer = 0

            While pos + 2 <= p.Length
                Dim subLeaf As UShort = BitConverter.ToUInt16(p, pos)
                pos += 2

                Select Case subLeaf
                    Case &H150D ' LF_MEMBER
                        If pos + 4 > p.Length Then Exit While
                        pos += 4 ' u32 member type
                        Dim skip As Integer = 0
                        ReadNumericLeaf(p, pos, skip)
                        pos += skip
                        AddName(members, ReadName(p, pos), pos)

                    Case &H150E ' LF_STMEMBER
                        If pos + 6 > p.Length Then Exit While
                        pos += 2 ' u16 attribute
                        pos += 4 ' u32 member type
                        Dim skip As Integer = 0
                        ReadNumericLeaf(p, pos, skip)
                        pos += skip
                        AddName(members, ReadName(p, pos), pos)

                    Case &H150F ' LF_METHOD
                        If pos + 8 > p.Length Then Exit While
                        pos += 4 ' u32 count
                        pos += 4 ' u32 method list
                        AddName(members, ReadName(p, pos), pos)

                    Case &H1510 ' LF_NESTTYPE
                        If pos + 2 > p.Length Then Exit While
                        pos += 2 ' u16 attribute
                        AddName(members, ReadName(p, pos), pos)

                    Case Else
                        ' Unknown sub-leaf; we cannot safely advance, stop here.
                        Exit While
                End Select
            End While

            Return members
        End Function

        Private Sub AddName(members As List(Of String), name As String, ByRef pos As Integer)
            If name.Length > 0 Then
                members.Add(name)
            End If

            pos += name.Length + 1
            ' Field-list sub-records are padded to a 2-byte boundary.
            pos = (pos + 1) And Not 1
        End Sub

        Private Function ReadName(p As Byte(), pos As Integer) As String
            If pos < 0 OrElse pos >= p.Length Then
                Return ""
            End If

            Return CodeView.ReadNullString(p, pos, Encoding.UTF8)
        End Function

        ''' <summary>
        ''' Resolve a display name for a type index (primitive, named user type, or raw id).
        ''' </summary>
        Private Function ResolveTypeName(typeIndex As UInteger) As String
            If typeIndex = 0 Then
                Return "void"
            End If

            If typeIndex < &H1000UI Then
                Return PrimitiveTypeName(CUShort(typeIndex))
            End If

            Dim nm As String = Nothing

            If names.TryGetValue(typeIndex, nm) AndAlso Not String.IsNullOrEmpty(nm) Then
                Return nm
            End If

            Return $"0x{typeIndex:X}"
        End Function

        ' ---- primitive type name table (low byte = base type) ----
        Private Shared ReadOnly primitiveNames As New Dictionary(Of Integer, String) From {
            {&H0, "void"}, {&H1, "void"}, {&H2, "signed char"}, {&H3, "unsigned char"},
            {&H4, "char"}, {&H5, "wchar_t"}, {&H6, "int"}, {&H7, "unsigned int"},
            {&H8, "short"}, {&H9, "unsigned short"}, {&HA, "signed char"},
            {&HB, "unsigned char"}, {&HC, "float"}, {&HD, "double"},
            {&HE, "long double"}, {&HF, "float128"},
            {&H10, "float complex"}, {&H11, "double complex"}, {&H12, "long double complex"},
            {&H13, "float128 complex"},
            {&H14, "bool"}, {&H15, "bool16"}, {&H16, "bool32"}, {&H17, "bool64"},
            {&H18, "long long"}, {&H19, "unsigned long long"}
        }

        Private Shared Function PrimitiveTypeName(t As UShort) As String
            Dim baseType As Integer = t And &HFF
            Dim mode As Integer = (t >> 8) And &HF

            Dim name As String = Nothing

            If Not primitiveNames.TryGetValue(baseType, name) Then
                name = $"bt{baseType:X2}"
            End If

            ' Non-zero mode means a (near/far/32/64-bit) pointer to the base type.
            If mode <> 0 Then
                Return name & "*"
            End If

            Return name
        End Function

        ''' <summary>
        ''' Read a CodeView numeric leaf (used for member offsets / constant values). Returns the
        ''' number of bytes consumed. Small values (&lt; 0x8000) are stored raw (2 bytes); otherwise
        ''' an LF_NUMERIC prefix (2 bytes) precedes the value.
        ''' </summary>
        Friend Shared Sub ReadNumericLeaf(p As Byte(), pos As Integer, ByRef bytesRead As Integer)
            If pos < 0 OrElse pos + 2 > p.Length Then
                bytesRead = 0
                Return
            End If

            Dim b0 As UShort = BitConverter.ToUInt16(p, pos)

            If b0 < &H8000US Then
                bytesRead = 2
                Return
            End If

            Dim valueSize As Integer

            Select Case b0
                Case &H8000US, &H8001US : valueSize = 2
                Case &H8002US, &H8003US, &H8004US : valueSize = 4
                Case &H8005US, &H8006US, &H8007US : valueSize = 8
                Case Else : valueSize = 0
            End Select

            bytesRead = 2 + valueSize
        End Sub
    End Class
End Namespace

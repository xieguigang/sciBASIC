#Region "Microsoft.VisualBasic::96ea4687767e7a21b081031eb0fe8c99, Data\BinaryData\Feather\Impl\Flatbuffers\Table.vb"

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

    '   Total Lines: 141
    '    Code Lines: 93 (65.96%)
    ' Comment Lines: 29 (20.57%)
    '    - Xml Docs: 10.34%
    ' 
    '   Blank Lines: 19 (13.48%)
    '     File Size: 6.05 KB


    '     Class Table
    ' 
    '         Properties: ByteBuffer
    ' 
    '         Function: __has_identifier, (+2 Overloads) __indirect, (+2 Overloads) __offset, __string, __union
    '                   __vector, __vector_as_arraysegment, __vector_len, (+2 Overloads) CompareStrings
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'* Copyright 2014 Google Inc. All rights reserved.
'*
'* Licensed under the Apache License, Version 2.0 (the "License");
'* you may not use this file except in compliance with the License.
'* You may obtain a copy of the License at
'*
'*     http://www.apache.org/licenses/LICENSE-2.0
'*
'* Unless required by applicable law or agreed to in writing, software
'* distributed under the License is distributed on an "AS IS" BASIS,
'* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'* See the License for the specific language governing permissions and
'* limitations under the License.


Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports std = System.Math

Namespace FlatBuffers
    ''' <summary>
    ''' All tables in the generated code derive from this struct, and add their own accessors.
    ''' </summary>
    Friend Class Table
        Public bb_pos As Integer
        Public bb As ByteBuffer

        Public ReadOnly Property ByteBuffer As ByteBuffer
            Get
                Return bb
            End Get
        End Property

        ' Look up a field in the vtable, return an offset into the object, or 0 if the field is not
        ' present.
        Public Function __offset(vtableOffset As Integer) As Integer
            Dim vtable = bb_pos - bb.GetInt(bb_pos)
            Return If(vtableOffset < bb.GetShort(vtable), bb.GetShort(vtable + vtableOffset), 0)
        End Function

        Public Shared Function __offset(vtableOffset As Integer, offset As Integer, bb As ByteBuffer) As Integer
            Dim vtable = bb.Length - offset
            Return bb.GetShort(vtable + vtableOffset - bb.GetInt(vtable)) + vtable
        End Function

        ' Retrieve the relative offset stored at "offset"
        Public Function __indirect(offset As Integer) As Integer
            Return offset + bb.GetInt(offset)
        End Function

        Public Shared Function __indirect(offset As Integer, bb As ByteBuffer) As Integer
            Return offset + bb.GetInt(offset)
        End Function

        ' Create a .NET String from UTF-8 data stored inside the flatbuffer.
        Public Function __string(offset As Integer) As String
            offset += bb.GetInt(offset)
            Dim len = bb.GetInt(offset)
            Dim startPos = offset + HeapSizeOf.int
            Return Encoding.UTF8.GetString(bb.Data, startPos, len)
        End Function

        ' Get the length of a vector whose offset is stored at "offset" in this object.
        Public Function __vector_len(offset As Integer) As Integer
            offset += bb_pos
            offset += bb.GetInt(offset)
            Return bb.GetInt(offset)
        End Function

        ' Get the start of data of a vector whose offset is stored at "offset" in this object.
        Public Function __vector(offset As Integer) As Integer
            offset += bb_pos
            Return offset + bb.GetInt(offset) + HeapSizeOf.int  ' data starts after the length
        End Function

        ' Get the data of a vector whoses offset is stored at "offset" in this object as an
        ' ArraySegment&lt;byte&gt;. If the vector is not present in the ByteBuffer,
        ' then a null value will be returned.
        Public Function __vector_as_arraysegment(offset As Integer) As ArraySegment(Of Byte)?
            Dim o = __offset(offset)
            If 0 = o Then
                Return Nothing
            End If

            Dim pos = __vector(o)
            Dim len = __vector_len(o)
            Return New ArraySegment(Of Byte)(bb.Data, pos, len)
        End Function

        ' Initialize any Table-derived type to point to the union at the given offset.
        Public Function __union(Of tT As {IFlatbufferObject, New})(offset As Integer) As tT
            offset += bb_pos
            Dim t As tT = New tT()
            t.__init(offset + bb.GetInt(offset), bb)
            Return t
        End Function

        Public Shared Function __has_identifier(bb As ByteBuffer, ident As String) As Boolean
            If ident.Length <> FileIdentifierLength Then
                Throw New ArgumentException("FlatBuffers: file identifier must be length " & FileIdentifierLength.ToString(), "ident")
            End If

            For i = 0 To FileIdentifierLength - 1
                If ident(i) <> Microsoft.VisualBasic.ChrW(bb.Get(bb.Position + HeapSizeOf.int + i)) Then Return False
            Next

            Return True
        End Function

        ' Compare strings in the ByteBuffer.
        Public Shared Function CompareStrings(offset_1 As Integer, offset_2 As Integer, bb As ByteBuffer) As Integer
            offset_1 += bb.GetInt(offset_1)
            offset_2 += bb.GetInt(offset_2)
            Dim len_1 = bb.GetInt(offset_1)
            Dim len_2 = bb.GetInt(offset_2)
            Dim startPos_1 = offset_1 + HeapSizeOf.int
            Dim startPos_2 = offset_2 + HeapSizeOf.int
            Dim len = std.Min(len_1, len_2)
            Dim bbArray = bb.Data
            For i = 0 To len - 1
                If bbArray(i + startPos_1) <> bbArray(i + startPos_2) Then Return bbArray(i + startPos_1) - bbArray(i + startPos_2)
            Next
            Return len_1 - len_2
        End Function

        ' Compare string from the ByteBuffer with the string object
        Public Shared Function CompareStrings(offset_1 As Integer, key As Byte(), bb As ByteBuffer) As Integer
            offset_1 += bb.GetInt(offset_1)
            Dim len_1 = bb.GetInt(offset_1)
            Dim len_2 = key.Length
            Dim startPos_1 = offset_1 + HeapSizeOf.int
            Dim len = std.Min(len_1, len_2)
            Dim bbArray = bb.Data
            For i = 0 To len - 1
                If bbArray(i + startPos_1) <> key(i) Then Return bbArray(i + startPos_1) - key(i)
            Next
            Return len_1 - len_2
        End Function
    End Class
End Namespace

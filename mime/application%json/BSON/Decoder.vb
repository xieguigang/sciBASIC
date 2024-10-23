#Region "Microsoft.VisualBasic::5e9b49816eae85084d43b530ba0b4825, mime\application%json\BSON\Decoder.vb"

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

    '   Total Lines: 168
    '    Code Lines: 105 (62.50%)
    ' Comment Lines: 29 (17.26%)
    '    - Xml Docs: 13.79%
    ' 
    '   Blank Lines: 34 (20.24%)
    '     File Size: 5.92 KB


    '     Class Decoder
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: decodeArray, decodeCString, decodeDocument, decodeElement, decodeString
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Text

Namespace BSON

    Public Class Decoder : Implements IDisposable

        ReadOnly reader As BinaryReader
        ReadOnly leaveOpen As Boolean = False

        ''' <summary>
        ''' create document decoder from a given in-memory stream data
        ''' </summary>
        ''' <param name="buf"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(buf As Byte(), Optional encoding As Encodings = Encodings.UTF8)
            Call Me.New(New MemoryStream(buf), encoding, leaveOpen:=False)
        End Sub

        Sub New(raw As Stream, Optional encoding As Encodings = Encodings.UTF8, Optional leaveOpen As Boolean = False)
            Me.reader = New BinaryReader(raw, encoding.CodePage, leaveOpen)
            Me.leaveOpen = leaveOpen
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getDocumentOffset() As Long
            Return reader.BaseStream.Position
        End Function

        Public Function decodeDocument() As JsonObject
            Dim length As Long = reader.ReadInt32() - 4
            Dim obj As New JsonObject()
            Dim i As Long = reader.BaseStream.Position

            While reader.BaseStream.Position < i + length - 1
                Dim name As String = Nothing
                Dim value As JsonElement = decodeElement(name)

                Call obj.Add(name, value)
            End While

            ' zero byte as terminator of current document object
            Call reader.ReadByte()

            Return obj
        End Function

        Private Function decodeArray() As JsonArray
            Dim obj As JsonObject = decodeDocument()
            Dim i As i32 = 0
            Dim array As New JsonArray()
            Dim key As Value(Of String) = ""

            While obj.HasObjectKey(key = Convert.ToString(++i))
                Call array.Add(obj(key))
            End While

            Return array
        End Function

        Private Function decodeString() As String
            Dim length As Integer = reader.ReadInt32()
            Dim buf As Byte() = reader.ReadBytes(length)

            Return Encoding.UTF8.GetString(buf)
        End Function

        ''' <summary>
        ''' Decode a ZERO terminated C-string.
        ''' </summary>
        ''' <returns></returns>
        Private Function decodeCString() As String
            Dim ms As New MemoryStream()

            While True
                Dim buf As Byte = reader.ReadByte()

                If buf = 0 Then
                    Exit While
                End If

                ms.WriteByte(buf)
            End While

            Return Encoding.UTF8.GetString(ms.GetBuffer(), 0, CInt(ms.Position))
        End Function

        Private Function decodeElement(ByRef name As String) As JsonElement
            Dim elementType As Byte = reader.ReadByte()

            If elementType = &H1 Then
                ' Double
                name = decodeCString()

                Return New JsonValue(New BSONValue(reader.ReadDouble()))
            ElseIf elementType = &H2 Then
                ' String
                name = decodeCString()

                Return New JsonValue(New BSONValue(decodeString()))
            ElseIf elementType = &H3 Then
                ' Document
                name = decodeCString()

                Return decodeDocument()
            ElseIf elementType = &H4 Then
                ' Array
                name = decodeCString()

                Return decodeArray()
            ElseIf elementType = &H5 Then
                ' Binary
                name = decodeCString()
                Dim length As Integer = reader.ReadInt32()
                Dim binaryType As Byte = reader.ReadByte()

                Return New JsonValue(New BSONValue(reader.ReadBytes(length)))
            ElseIf elementType = &H8 Then
                ' Boolean
                name = decodeCString()

                Return New JsonValue(New BSONValue(reader.ReadBoolean()))
            ElseIf elementType = &H9 Then
                ' DateTime
                name = decodeCString()
                Dim time As Int64 = reader.ReadInt64()
                Return New JsonValue(New BSONValue(New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + New TimeSpan(time * 10000)))
            ElseIf elementType = &HA Then
                ' None
                name = decodeCString()
                Return New JsonValue(New BSONValue())
            ElseIf elementType = &H10 Then
                ' Int32
                name = decodeCString()
                Return New JsonValue(New BSONValue(reader.ReadInt32()))
            ElseIf elementType = &H12 Then
                ' Int64
                name = decodeCString()
                Return New JsonValue(New BSONValue(reader.ReadInt64()))
            ElseIf elementType = &H7 Then
                name = decodeCString()
                Return ObjectId.ReadIdValue(reader)
            End If

            Throw New Exception(String.Format("Don't know elementType={0}", elementType))
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    If Not leaveOpen Then
                        Call reader.Dispose()
                    End If
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace

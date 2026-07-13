#Region "Microsoft.VisualBasic::PLY Point Cloud Reader, gr\Landscape\PLY\PlyReader.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
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

    '   Total Lines: 0
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 0 B


    '     Module PlyReader
    ' 
    '         Function: ReadFile, ReadStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports std = System.Math

Namespace Ply

    ''' <summary>
    ''' PLY point cloud file reader. Supports ASCII, binary_little_endian,
    ''' and binary_big_endian format variants. Parses vertex elements
    ''' into <see cref="PointCloud"/> objects with automatic property
    ''' mapping for x, y, z, intensity, and color channels.
    ''' </summary>
    Public Module PlyReader

#Region "Public API"

        ''' <summary>
        ''' Read a PLY file from the given path and return parsed point cloud data.
        ''' </summary>
        ''' <param name="path">File path to a .ply file.</param>
        ''' <returns>Array of <see cref="PointCloud"/> parsed from vertex elements.</returns>
        <Extension>
        Public Function ReadFile(path As String) As PointCloud()
            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return ReadStream(fs)
            End Using
        End Function

        ''' <summary>
        ''' Read PLY data from an open stream. The stream must be readable and seekable.
        ''' Auto-detects ASCII vs binary from the header and delegates to the
        ''' appropriate parser.
        ''' </summary>
        ''' <param name="stream">Readable, seekable stream positioned at the PLY header.</param>
        ''' <returns>Array of <see cref="PointCloud"/> parsed from vertex elements.</returns>
        Public Function ReadStream(stream As Stream) As PointCloud()
            ' Locate header boundary and determine data start position
            Dim headerText As String = Nothing
            Dim dataStartPosition As Long = -1

            Call LocateHeaderEnd(stream, headerText, dataStartPosition)

            If String.IsNullOrEmpty(headerText) Then
                Throw New InvalidDataException("Invalid PLY file: header not found or empty.")
            End If

            ' Parse the header to build a vertex format descriptor
            Dim vertexFmt As PlyVertexFormat = ParseHeaderText(headerText)

            If vertexFmt Is Nothing OrElse vertexFmt.Count <= 0 Then
                Return New PointCloud() {}
            End If

            ' Read vertex data based on format type
            Dim points As PointCloud()

            Select Case vertexFmt.format.ToLowerInvariant()
                Case "ascii"
                    points = ReadAsciiVertices(stream, dataStartPosition, vertexFmt)
                Case "binary_little_endian"
                    points = ReadBinaryVertices(stream, dataStartPosition, vertexFmt, isLittleEndian:=True)
                Case "binary_big_endian"
                    points = ReadBinaryVertices(stream, dataStartPosition, vertexFmt, isLittleEndian:=False)
                Case Else
                    Throw New NotSupportedException($"Unsupported PLY format: '{vertexFmt.format}'.")
            End Select

            Return points
        End Function

#End Region

#Region "Header Parsing"

        ''' <summary>
        ''' Locates the end of the ASCII header in the stream and reads header text.
        ''' After this call, dataStartPosition will point to the first byte after the
        ''' newline following "end_header".
        ''' </summary>
        Private Sub LocateHeaderEnd(stream As Stream, ByRef headerText As String, ByRef dataStartPos As Long)
            Const marker As String = "end_header"
            Dim sb As New StringBuilder()
            Dim buffer(4095) As Byte
            Dim found As Boolean = False

            Do
                Dim bytesRead As Integer = stream.Read(buffer, 0, buffer.Length)

                If bytesRead = 0 Then
                    Exit Do
                End If

                Dim chunk As String = Encoding.ASCII.GetString(buffer, 0, bytesRead)
                Dim markerIdx As Integer = chunk.IndexOf(marker, StringComparison.Ordinal)

                If markerIdx >= 0 Then
                    ' Header text is everything up to (and including) "end_header"
                    sb.Append(chunk.Substring(0, markerIdx + marker.Length))

                    ' Calculate where the actual data begins
                    Dim afterMarker As Integer = markerIdx + marker.Length
                    Dim remaining As Integer = bytesRead - afterMarker

                    ' Skip any newline characters after "end_header"
                    Dim skipBytes As Integer = 0
                    Dim i As Integer = afterMarker

                    While i < bytesRead AndAlso (buffer(i) = CByte(AscW(vbCr)) OrElse buffer(i) = CByte(AscW(vbLf)))
                        skipBytes += 1
                        i += 1
                    End While

                    ' dataStartPos = stream position - remaining bytes + skipped newline bytes
                    dataStartPos = stream.Position - remaining + skipBytes
                    found = True
                    Exit Do
                Else
                    sb.Append(chunk)
                End If
            Loop While True

            If Not found Then
                headerText = sb.ToString()
                dataStartPos = stream.Position
            Else
                headerText = sb.ToString()
            End If
        End Sub

        ''' <summary>
        ''' Parses the PLY header text to extract the vertex element format.
        ''' </summary>
        Private Function ParseHeaderText(headerText As String) As PlyVertexFormat
            Using reader As New System.IO.StringReader(headerText)
                Dim line As String = reader.ReadLine()

                ' First line must be "ply"
                If line Is Nothing OrElse Not line.Trim().Equals("ply", StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidDataException("Not a valid PLY file: missing 'ply' magic.")
                End If

                Dim format As String = "ascii"
                Dim elementName As String = Nothing
                Dim elementCount As Integer = 0
                Dim vertexProps As New List(Of PlyPropertyDef)()
                Dim foundVertex As Boolean = False
                Dim comment As String = Nothing

                Do
                    line = reader.ReadLine()
                    If line Is Nothing Then Exit Do

                    line = line.Trim()

                    If String.IsNullOrEmpty(line) Then Continue Do
                    If line.StartsWith("end_header", StringComparison.OrdinalIgnoreCase) Then Exit Do

                    Dim tokens As String() = line.Split(New Char() {" "c, vbTab}, StringSplitOptions.RemoveEmptyEntries)

                    If tokens.Length = 0 Then Continue Do

                    Select Case tokens(0).ToLowerInvariant()
                        Case "format"
                            If tokens.Length >= 2 Then
                                format = tokens(1).ToLowerInvariant()
                            End If

                        Case "comment"
                            comment = String.Join(" ", tokens, 1, tokens.Length - 1)

                        Case "element"
                            ' Save previous element if it was vertex
                            If foundVertex AndAlso elementName.Equals("vertex", StringComparison.OrdinalIgnoreCase) Then
                                ' Already captured vertex properties; keep the count
                            End If

                            If tokens.Length >= 3 Then
                                elementName = tokens(1).ToLowerInvariant()

                                If Integer.TryParse(tokens(2), elementCount) Then
                                    If elementName = "vertex" Then
                                        foundVertex = True
                                    Else
                                        ' Reset for non-vertex elements - we skip those
                                        foundVertex = False
                                    End If
                                End If
                            End If

                        Case "property"
                            If foundVertex Then
                                If tokens.Length >= 3 Then
                                    If tokens(1).Equals("list", StringComparison.OrdinalIgnoreCase) Then
                                        ' list property: "property list <count_type> <value_type> <name>"
                                        If tokens.Length >= 5 Then
                                            vertexProps.Add(New PlyPropertyDef With {
                                                .name = tokens(4),
                                                .type = tokens(3),
                                                .isList = True,
                                                .countType = tokens(2)
                                            })
                                        End If
                                    Else
                                        ' scalar property: "property <type> <name>"
                                        vertexProps.Add(New PlyPropertyDef With {
                                            .name = tokens(2),
                                            .type = tokens(1),
                                            .isList = False
                                        })
                                    End If
                                End If
                            End If
                    End Select
                Loop

                ' Build the vertex format
                If Not foundVertex Then
                    Return Nothing
                End If

                ' Compute byte offsets and stride for binary reading
                Dim offset As Integer = 0

                For Each prop As PlyPropertyDef In vertexProps
                    prop.byteOffset = offset

                    If prop.isList Then
                        ' List types are variable-length; for binary we handle specially
                        prop.byteSize = 0
                    Else
                        prop.byteSize = GetPropertyByteSize(prop.type)
                        offset += prop.byteSize
                    End If
                Next

                Dim result As New PlyVertexFormat With {
                    .format = format,
                    .count = elementCount,
                    .properties = vertexProps.ToArray(),
                    .stride = offset,
                    .comment = comment
                }

                Return result
            End Using
        End Function

        ''' <summary>
        ''' Returns the byte size for a PLY scalar property type.
        ''' </summary>
        Private Function GetPropertyByteSize(typeName As String) As Integer
            Select Case typeName.ToLowerInvariant()
                Case "char", "int8"
                    Return 1
                Case "uchar", "uint8"
                    Return 1
                Case "short", "int16"
                    Return 2
                Case "ushort", "uint16"
                    Return 2
                Case "int", "int32"
                    Return 4
                Case "uint", "uint32"
                    Return 4
                Case "float", "float32"
                    Return 4
                Case "double", "float64"
                    Return 8
                Case Else
                    ' Default to 4 bytes for unknown types
                    Return 4
            End Select
        End Function

#End Region

#Region "ASCII Data Reading"

        ''' <summary>
        ''' Reads ASCII-format vertex data from the stream starting at dataStartPos.
        ''' </summary>
        Private Function ReadAsciiVertices(stream As Stream,
                                            dataStartPos As Long,
                                            vertexFmt As PlyVertexFormat) As PointCloud()
            ' Seek to data start position
            Call stream.Seek(dataStartPos, SeekOrigin.Begin)

            Using reader As New StreamReader(stream, Encoding.ASCII,
                                             detectEncodingFromByteOrderMarks:=False,
                                             bufferSize:=4096,
                                             leaveOpen:=True)
                ' Build property index mapping
                Dim mapping As PropertyMapping = BuildPropertyMapping(vertexFmt.properties)
                Dim points As New List(Of PointCloud)(vertexFmt.count)

                Dim linesRead As Integer = 0

                Do While linesRead < vertexFmt.count
                    Dim line As String = reader.ReadLine()

                    If line Is Nothing Then Exit Do

                    line = line.Trim()

                    If String.IsNullOrEmpty(line) OrElse line.StartsWith("#"c) Then
                        Continue Do
                    End If

                    Dim tokens As String() = line.Split(New Char() {" "c, vbTab},
                                                         StringSplitOptions.RemoveEmptyEntries)

                    If tokens.Length = 0 Then Continue Do

                    Dim point As New PointCloud()
                    Dim colorR As Double = 0
                    Dim colorG As Double = 0
                    Dim colorB As Double = 0
                    Dim hasColorR As Boolean = False
                    Dim hasColorG As Boolean = False
                    Dim hasColorB As Boolean = False

                    ' Parse each property value based on mapping
                    If mapping.xIdx >= 0 AndAlso mapping.xIdx < tokens.Length Then
                        Double.TryParse(tokens(mapping.xIdx), point.x)
                    End If
                    If mapping.yIdx >= 0 AndAlso mapping.yIdx < tokens.Length Then
                        Double.TryParse(tokens(mapping.yIdx), point.y)
                    End If
                    If mapping.zIdx >= 0 AndAlso mapping.zIdx < tokens.Length Then
                        Double.TryParse(tokens(mapping.zIdx), point.z)
                    End If
                    If mapping.intensityIdx >= 0 AndAlso mapping.intensityIdx < tokens.Length Then
                        Double.TryParse(tokens(mapping.intensityIdx), point.intensity)
                    End If

                    ' Color channels
                    If mapping.rIdx >= 0 AndAlso mapping.rIdx < tokens.Length Then
                        Double.TryParse(tokens(mapping.rIdx), colorR)
                        hasColorR = True
                    End If
                    If mapping.gIdx >= 0 AndAlso mapping.gIdx < tokens.Length Then
                        Double.TryParse(tokens(mapping.gIdx), colorG)
                        hasColorG = True
                    End If
                    If mapping.bIdx >= 0 AndAlso mapping.bIdx < tokens.Length Then
                        Double.TryParse(tokens(mapping.bIdx), colorB)
                        hasColorB = True
                    End If

                    ' Assemble color string if any color channel was present
                    If hasColorR OrElse hasColorG OrElse hasColorB Then
                        ' Normalize: PLY colors are typically 0-255 (uchar) or 0-1 (float)
                        If colorR > 1 OrElse colorG > 1 OrElse colorB > 1 Then
                            ' Likely in 0-255 range, normalize to 0-1 for consistency
                            colorR = std.Min(colorR / 255.0, 1.0)
                            colorG = std.Min(colorG / 255.0, 1.0)
                            colorB = std.Min(colorB / 255.0, 1.0)
                        End If
                        Dim r As Integer = CInt(std.Max(0, std.Min(colorR * 255, 255)))
                        Dim g As Integer = CInt(std.Max(0, std.Min(colorG * 255, 255)))
                        Dim b As Integer = CInt(std.Max(0, std.Min(colorB * 255, 255)))
                        point.color = $"#{r:X2}{g:X2}{b:X2}"
                    End If

                    points.Add(point)
                    linesRead += 1
                Loop

                Return points.ToArray()
            End Using
        End Function

#End Region

#Region "Binary Data Reading"

        ''' <summary>
        ''' Reads binary-format vertex data from the stream starting at dataStartPos.
        ''' </summary>
        Private Function ReadBinaryVertices(stream As Stream,
                                             dataStartPos As Long,
                                             vertexFmt As PlyVertexFormat,
                                             isLittleEndian As Boolean) As PointCloud()
            Call stream.Seek(dataStartPos, SeekOrigin.Begin)

            Using reader As New BinaryReader(stream, Encoding.ASCII, leaveOpen:=True)
                Dim mapping As PropertyMapping = BuildPropertyMapping(vertexFmt.properties)
                Dim points As New List(Of PointCloud)(vertexFmt.count)

                For i As Integer = 0 To vertexFmt.count - 1
                    Dim point As New PointCloud()
                    Dim colorR As Double = 0
                    Dim colorG As Double = 0
                    Dim colorB As Double = 0
                    Dim hasColorR As Boolean = False
                    Dim hasColorG As Boolean = False
                    Dim hasColorB As Boolean = False

                    For Each prop As PlyPropertyDef In vertexFmt.properties
                        If prop.isList Then
                            ' Skip list properties (e.g., vertex_indices for faces)
                            ' Read count value first, then skip the list data
                            Dim listCount As Integer = ReadListCount(reader, prop.countType, isLittleEndian)
                            Dim itemBytes As Integer = GetPropertyByteSize(prop.type)

                            If itemBytes > 0 AndAlso listCount > 0 Then
                                reader.BaseStream.Seek(listCount * itemBytes, SeekOrigin.Current)
                            End If
                            Continue For
                        End If

                        Dim value As Double = ReadBinaryPropertyValue(reader, prop.type, isLittleEndian)

                        Select Case prop.name.ToLowerInvariant()
                            Case "x"
                                point.x = value
                            Case "y"
                                point.y = value
                            Case "z"
                                point.z = value
                            Case "intensity", "scalar_", "value", "scalar"
                                point.intensity = value
                            Case "red", "r", "diffuse_red"
                                colorR = value
                                hasColorR = True
                            Case "green", "g", "diffuse_green"
                                colorG = value
                                hasColorG = True
                            Case "blue", "b", "diffuse_blue"
                                colorB = value
                                hasColorB = True
                            Case "nx", "ny", "nz"
                                ' Normal vectors - skip for point cloud
                            Case "alpha", "a"
                                ' Alpha channel - could be used but we don't store it
                            Case Else
                                ' Unknown property - call ReadBinaryPropertyValue to advance the stream
                                ' value is already read, no need for further action
                        End Select
                    Next

                    ' Assemble color string
                    If hasColorR OrElse hasColorG OrElse hasColorB Then
                        If colorR > 1 OrElse colorG > 1 OrElse colorB > 1 Then
                            colorR = std.Min(colorR / 255.0, 1.0)
                            colorG = std.Min(colorG / 255.0, 1.0)
                            colorB = std.Min(colorB / 255.0, 1.0)
                        End If
                        Dim r As Integer = CInt(std.Max(0, std.Min(colorR * 255, 255)))
                        Dim g As Integer = CInt(std.Max(0, std.Min(colorG * 255, 255)))
                        Dim b As Integer = CInt(std.Max(0, std.Min(colorB * 255, 255)))
                        point.color = $"#{r:X2}{g:X2}{b:X2}"
                    End If

                    points.Add(point)
                Next

                Return points.ToArray()
            End Using
        End Function

        ''' <summary>
        ''' Reads a single binary property value and returns it as Double.
        ''' Advances the stream by the appropriate number of bytes.
        ''' </summary>
        Private Function ReadBinaryPropertyValue(reader As BinaryReader,
                                                  typeName As String,
                                                  isLittleEndian As Boolean) As Double
            Dim needsSwap As Boolean = (BitConverter.IsLittleEndian <> isLittleEndian)

            Select Case typeName.ToLowerInvariant()
                Case "char", "int8"
                    Return CDbl(reader.ReadSByte())

                Case "uchar", "uint8"
                    Return CDbl(reader.ReadByte())

                Case "short", "int16"
                    Dim val As Short = reader.ReadInt16()
                    If needsSwap Then val = SwapEndian(val)
                    Return CDbl(val)

                Case "ushort", "uint16"
                    Dim val As UInt16 = reader.ReadUInt16()
                    If needsSwap Then val = SwapEndian(val)
                    Return CDbl(val)

                Case "int", "int32"
                    Dim val As Integer = reader.ReadInt32()
                    If needsSwap Then val = SwapEndian(val)
                    Return CDbl(val)

                Case "uint", "uint32"
                    Dim val As UInt32 = reader.ReadUInt32()
                    If needsSwap Then val = SwapEndian(val)
                    Return CDbl(val)

                Case "float", "float32"
                    Dim val As Single = reader.ReadSingle()
                    If needsSwap Then val = SwapEndian(val)
                    Return CDbl(val)

                Case "double", "float64"
                    Dim val As Double = reader.ReadDouble()
                    If needsSwap Then val = SwapEndian(val)
                    Return val

                Case Else
                    ' Unknown type - read 4 bytes as default
                    Return CDbl(reader.ReadInt32())
            End Select
        End Function

        ''' <summary>
        ''' Reads the count value for a list-type PLY property.
        ''' </summary>
        Private Function ReadListCount(reader As BinaryReader,
                                        countType As String,
                                        isLittleEndian As Boolean) As Integer
            Dim needsSwap As Boolean = (BitConverter.IsLittleEndian <> isLittleEndian)

            Select Case countType.ToLowerInvariant()
                Case "char", "int8"
                    Return CInt(reader.ReadSByte())
                Case "uchar", "uint8"
                    Return CInt(reader.ReadByte())
                Case "short", "int16"
                    Dim val As Short = reader.ReadInt16()
                    If needsSwap Then val = SwapEndian(val)
                    Return CInt(val)
                Case "ushort", "uint16"
                    Dim val As UInt16 = reader.ReadUInt16()
                    If needsSwap Then val = SwapEndian(val)
                    Return CInt(val)
                Case "int", "int32"
                    Dim val As Integer = reader.ReadInt32()
                    If needsSwap Then val = SwapEndian(val)
                    Return val
                Case Else
                    Return reader.ReadInt32()
            End Select
        End Function

#End Region

#Region "Property Mapping"

        ''' <summary>
        ''' Builds a property-to-field index mapping from vertex property definitions.
        ''' </summary>
        Private Function BuildPropertyMapping(properties As PlyPropertyDef()) As PropertyMapping
            Dim mapping As New PropertyMapping() With {
                .xIdx = -1,
                .yIdx = -1,
                .zIdx = -1,
                .intensityIdx = -1,
                .rIdx = -1,
                .gIdx = -1,
                .bIdx = -1
            }

            For i As Integer = 0 To properties.Length - 1
                Dim name As String = properties(i).name.ToLowerInvariant()

                Select Case name
                    Case "x"
                        mapping.xIdx = i
                    Case "y"
                        mapping.yIdx = i
                    Case "z"
                        mapping.zIdx = i
                    Case "intensity", "scalar_", "value", "scalar"
                        mapping.intensityIdx = i
                    Case "red", "r", "diffuse_red"
                        mapping.rIdx = i
                    Case "green", "g", "diffuse_green"
                        mapping.gIdx = i
                    Case "blue", "b", "diffuse_blue"
                        mapping.bIdx = i
                End Select
            Next

            Return mapping
        End Function

        ''' <summary>
        ''' Holds property-to-PointCloud field index mappings.
        ''' </summary>
        Private Structure PropertyMapping
            Public xIdx As Integer
            Public yIdx As Integer
            Public zIdx As Integer
            Public intensityIdx As Integer
            Public rIdx As Integer
            Public gIdx As Integer
            Public bIdx As Integer
        End Structure

#End Region

#Region "Internal Types"

        ''' <summary>
        ''' Represents a single PLY property definition from the header.
        ''' </summary>
        Private Class PlyPropertyDef
            ''' <summary>Property name (e.g., "x", "y", "z", "intensity")</summary>
            Public name As String
            ''' <summary>Data type name (e.g., "float", "double", "uchar")</summary>
            Public type As String
            ''' <summary>True if this is a list-type property</summary>
            Public isList As Boolean
            ''' <summary>For list types, the type used for the count value</summary>
            Public countType As String
            ''' <summary>Byte offset within a vertex record (only used for binary)</summary>
            Public byteOffset As Integer
            ''' <summary>Byte size of this property (0 for list types)</summary>
            Public byteSize As Integer
        End Class

        ''' <summary>
        ''' Describes the vertex element format parsed from the PLY header.
        ''' </summary>
        Private Class PlyVertexFormat
            ''' <summary>PLY format: ascii, binary_little_endian, binary_big_endian</summary>
            Public format As String
            ''' <summary>Number of vertex entries</summary>
            Public count As Integer
            ''' <summary>Property definitions for each vertex</summary>
            Public properties As PlyPropertyDef()
            ''' <summary>Total byte stride per vertex record (binary only, 0 for ASCII)</summary>
            Public stride As Integer
            ''' <summary>Comment text from the PLY header</summary>
            Public comment As String
        End Class

#End Region

#Region "Endianness Helpers"

        ''' <summary>
        ''' Swaps endianness for a Short value.
        ''' </summary>
        Private Function SwapEndian(value As Short) As Short
            Return CShort(
                ((CUInt(value) >> 8) And &HFFUI) Or
                ((CUInt(value) << 8) And &HFF00UI)
            )
        End Function

        ''' <summary>
        ''' Swaps endianness for a UShort value.
        ''' </summary>
        Private Function SwapEndian(value As UShort) As UShort
            Return CUShort(
                ((CUInt(value) >> 8) And &HFFUI) Or
                ((CUInt(value) << 8) And &HFF00UI)
            )
        End Function

        ''' <summary>
        ''' Swaps endianness for an Integer value.
        ''' </summary>
        Private Function SwapEndian(value As Integer) As Integer
            Return CInt(
                (CUInt(value) >> 24) Or
                ((CUInt(value) >> 8) And &HFF00UI) Or
                ((CUInt(value) << 8) And &HFF0000UI) Or
                (CUInt(value) << 24)
            )
        End Function

        ''' <summary>
        ''' Swaps endianness for a UInteger value.
        ''' </summary>
        Private Function SwapEndian(value As UInteger) As UInteger
            Return (value >> 24) Or
                   ((value >> 8) And &HFF00UI) Or
                   ((value << 8) And &HFF0000UI) Or
                   (value << 24)
        End Function

        ''' <summary>
        ''' Swaps endianness for a Single value.
        ''' </summary>
        Private Function SwapEndian(value As Single) As Single
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            Array.Reverse(bytes)
            Return BitConverter.ToSingle(bytes, 0)
        End Function

        ''' <summary>
        ''' Swaps endianness for a Double value.
        ''' </summary>
        Private Function SwapEndian(value As Double) As Double
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            Array.Reverse(bytes)
            Return BitConverter.ToDouble(bytes, 0)
        End Function

#End Region

    End Module
End Namespace
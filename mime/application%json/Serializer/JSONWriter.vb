#Region "Microsoft.VisualBasic::2cf6d380c80abd9b8efbf5c466766cba, mime\application%json\Serializer\JSONWriter.vb"

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

    '   Total Lines: 410
    '    Code Lines: 292 (71.22%)
    ' Comment Lines: 69 (16.83%)
    '    - Xml Docs: 44.93%
    ' 
    '   Blank Lines: 49 (11.95%)
    '     File Size: 16.35 KB


    ' Class JSONWriter
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: encodeString, escapeString, IsJsonNumber, jsonValueString
    ' 
    '     Sub: (+2 Overloads) BuildJSONString, (+2 Overloads) Dispose, jsonArrayString, jsonObjectString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.BSON
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.ValueTypes
Imports any = Microsoft.VisualBasic.Scripting

Friend Class JSONWriter : Implements IDisposable

    ReadOnly json As TextWriter
    ReadOnly opts As JSONSerializerOptions
    ''' <summary>
    ''' when true, only flush the internal text writer on dispose and leave the
    ''' underlying stream open so that the caller can continue writing to it
    ''' afterwards. when false (default), the text writer is disposed which also
    ''' closes the underlying stream.
    ''' </summary>
    ReadOnly leaveOpen As Boolean

    Dim disposedValue As Boolean

    Sub New(opts As JSONSerializerOptions, file As Stream, Optional leaveOpen As Boolean = False)
        Me.opts = opts
        Me.leaveOpen = leaveOpen
        ' emit UTF-8 without BOM and propagate the leaveOpen flag to the underlying
        ' stream so the caller can keep writing when leaveOpen is true.
        Me.json = New StreamWriter(file, New UTF8Encoding(False), 1024, leaveOpen)
    End Sub

    Sub New(opts As JSONSerializerOptions, file As StringBuilder, Optional leaveOpen As Boolean = False)
        Me.opts = opts
        Me.leaveOpen = leaveOpen
        Me.json = New StringWriter(file)
    End Sub

    Public Sub BuildJSONString(json As JsonElement)
        Call BuildJSONString(json, 0)
    End Sub

    Private Sub BuildJSONString(json As JsonElement, indent As Integer, Optional trailingNewline As Boolean = True)
        If json Is Nothing OrElse (TypeOf json Is JsonValue AndAlso DirectCast(json, JsonValue).value Is Nothing) Then
            If opts.indent Then
                Call Me.json.WriteLine(opts.offsets(indent) & "null")
            Else
                Call Me.json.WriteLine("null")
            End If
        Else
            Select Case json.GetType
                Case GetType(JsonValue)
                    Dim val As String = jsonValueString(DirectCast(json, JsonValue))

                    If opts.indent Then
                        Call Me.json.Write(opts.offsets(indent) & val)
                    Else
                        Call Me.json.Write(val)
                    End If

                    If trailingNewline Then
                        Call Me.json.WriteLine()
                    End If
                Case GetType(JsonObject) : Call jsonObjectString(DirectCast(json, JsonObject), indent)
                Case GetType(JsonArray) : Call jsonArrayString(DirectCast(json, JsonArray), indent)
                Case Else
                    Throw New NotImplementedException(json.GetType.FullName)
            End Select
        End If
    End Sub

    ''' <summary>
    ''' "..."
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    Private Function jsonValueString(obj As JsonValue) As String
        Dim value As Object = obj.value

        If TypeOf value Is BSONValue Then
            value = DirectCast(value, BSONValue).GetObjectValue
        End If

        If value Is Nothing Then
            Return "null"
        End If

        If TypeOf value Is Date Then
            If opts.unixTimestamp Then
                ' UnixTimeStamp returns a Double; format it with the invariant
                ' culture so the timestamp is always a valid, locale-independent
                ' json number.
                Return DirectCast(value, Date).UnixTimeStamp.ToString("R", CultureInfo.InvariantCulture)
            Else
                ' a json date must be a quoted string; use the round-trip ISO-8601 format
                Return $"""{DirectCast(value, Date).ToString("o", CultureInfo.InvariantCulture)}"""
            End If
        ElseIf TypeOf value Is String Then
            Return encodeString(value)
        ElseIf TypeOf value Is Boolean Then
            Return If(DirectCast(value, Boolean), "true", "false")
        ElseIf TypeOf value Is ObjectId Then
            Return $"""{value.ToString}"""
        ElseIf TypeOf value Is Double Then
            Dim d As Double = CDbl(value)

            If Double.IsNaN(d) Then
                Return """NaN"""
            ElseIf d = Double.PositiveInfinity Then
                Return """Infinity"""
            ElseIf d = Double.NegativeInfinity Then
                Return """-Infinity"""
            Else
                Return d.ToString("R", CultureInfo.InvariantCulture)
            End If
        ElseIf TypeOf value Is Single Then
            Dim s As Single = CSng(value)

            If Single.IsNaN(s) Then
                Return """NaN"""
            ElseIf s = Single.PositiveInfinity Then
                Return """Infinity"""
            ElseIf s = Single.NegativeInfinity Then
                Return """-Infinity"""
            Else
                Return s.ToString("R", CultureInfo.InvariantCulture)
            End If
        ElseIf IsJsonNumber(value) Then
            ' all numeric clr primitive types (byte..ulong, decimal, ...) are emitted
            ' as unquoted json numbers using the invariant culture so the decimal
            ' separator is always "." and thousands separators never appear.
            If TypeOf value Is IFormattable Then
                Return DirectCast(value, IFormattable).ToString(Nothing, CultureInfo.InvariantCulture)
            Else
                Return any.ToString(value)
            End If
        Else
            ' anything that is not a json number / boolean / date / string (char,
            ' guid, timespan, enum, uri, version, opaque clr objects, ...) is emitted
            ' as a quoted json string so the serialized output is always valid json.
            Return encodeString(Convert.ToString(value, CultureInfo.InvariantCulture))
        End If
    End Function

    ''' <summary>
    ''' true for the clr numeric primitive types that may be emitted as an unquoted
    ''' json number. any other type (char, guid, timespan, enum, uri, version, ...)
    ''' must be serialized as a quoted string to stay valid json.
    ''' </summary>
    Private Function IsJsonNumber(value As Object) As Boolean
        Select Case value.GetType
            Case GetType(Byte), GetType(SByte),
                 GetType(Int16), GetType(UInt16),
                 GetType(Int32), GetType(UInt32),
                 GetType(Int64), GetType(UInt64),
                 GetType(Decimal), GetType(Double), GetType(Single)
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' {...}
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub jsonObjectString(obj As JsonObject, indent As Integer)
        Dim members As NamedValue(Of JsonElement)() = obj.ToArray

        If opts.indent Then
            Call json.WriteLine(opts.offsets(indent) & "{")
        Else
            Call json.Write("{")
        End If

        For i As Integer = 0 To members.Length - 1
            Dim member As NamedValue(Of JsonElement) = members(i)
            Dim name As String = encodeString(member.Name)
            Dim isLiteral As Boolean = TypeOf member.Value Is JsonValue
            Dim comment As String = obj.GetCommentText(member.Name)

            If Not comment.StringEmpty Then
                For Each line As String In comment.LineTokens
                    If opts.indent Then
                        Call json.WriteLine(opts.offsets(indent + 1) & $"// {line}")
                    Else
                        Call json.Write($"/* {line} */")
                    End If
                Next
            End If

            If Not isLiteral Then
                If TypeOf member.Value Is JsonArray Then
                    Dim type = DirectCast(member.Value, JsonArray).UnderlyingType
                    isLiteral = type IsNot GetType(Object) AndAlso type IsNot GetType(String)
                End If
            End If

            If opts.indent Then
                If Not isLiteral Then
                    Call json.WriteLine(opts.offsets(indent + 1) & name & ":")
                Else
                    Call json.Write(opts.offsets(indent + 1) & name & ": ")
                End If
            Else
                Call json.Write(name & ": ")
            End If

            If isLiteral Then
                If TypeOf member.Value Is JsonValue Then
                    Call json.Write(jsonValueString(DirectCast(member.Value, JsonValue)))
                Else
                    Call BuildJSONString(member.Value, 0)
                End If
            Else
                Call BuildJSONString(member.Value, indent + 1)
            End If

            If i < members.Length - 1 Then
                Call json.Write(",")

                If opts.indent Then
                    Call json.WriteLine()
                End If
            End If
        Next

        If opts.indent Then
            Call json.WriteLine()
            Call json.Write(opts.offsets(indent) & "}")
        Else
            Call json.Write("}")
        End If
    End Sub

    ''' <summary>
    ''' [...]
    ''' </summary>
    ''' <param name="arr"></param>
    Private Sub jsonArrayString(arr As JsonArray, indent As Integer)
        Dim elementType As Type = arr.UnderlyingType
        Dim literalVector As Boolean = elementType IsNot GetType(Object) AndAlso elementType IsNot GetType(String)

        If opts.indent Then
            If literalVector Then
                Call json.Write(opts.offsets(indent) & "[")
            Else
                Call json.WriteLine(opts.offsets(indent) & "[")
            End If
        Else
            Call json.Write("[")
        End If

        Select Case elementType
            Case GetType(Object)
                Dim objs As JsonElement() = arr.ToArray

                For i As Integer = 0 To objs.Length - 1
                    ' BuildJSONString writes the element (scalar or nested object/array)
                    ' with the proper indentation itself; we suppress its trailing newline
                    ' so the separator comma can be placed at the end of the same line.
                    Call BuildJSONString(objs(i), indent + 1, trailingNewline:=False)

                    If i < objs.Length - 1 Then
                        If opts.indent Then
                            Call json.Write(",")
                            Call json.WriteLine()
                        Else
                            Call json.Write(", ")
                        End If
                    End If
                Next

                If opts.indent Then
                    Call json.WriteLine()
                End If
            Case GetType(String)
                ' one line per string element
                Dim strs As New List(Of String)

                For Each ele As JsonElement In arr
                    Call strs.Add(jsonValueString(DirectCast(ele, JsonValue)))
                Next

                ' Dim check_chars = strs.All(Function(s) s = "null" OrElse s.Length < 5)

                If opts.indent Then
                    ' If check_chars Then
                    ' Call json.Write(strs.JoinBy(", "))
                    ' Else
                    Call json.WriteLine(strs.Select(Function(si) opts.offsets(indent + 1) & si).JoinBy(", " & Environment.NewLine))
                    ' End If
                Else
                    Call json.Write(strs.JoinBy(","))
                End If
            Case Else
                ' in one line vector style
                Dim strs As New List(Of String)

                For Each ele As JsonElement In arr
                    Call strs.Add(jsonValueString(DirectCast(ele, JsonValue)))
                Next

                If opts.indent Then
                    Call json.Write(strs.JoinBy(", "))
                Else
                    Call json.Write(strs.JoinBy(","))
                End If
        End Select

        If opts.indent Then
            If literalVector Then
                Call json.Write("]")
            Else
                Call json.Write(opts.offsets(indent) & "]")
            End If
        Else
            Call json.Write("]")
        End If
    End Sub

    Private Function encodeString(value As String) As String
        ' the non-unicode path still has to escape the json-grammar control characters
        ' (\, ", tab, cr, lf, ...) and only skips escaping the >0x7f non-ASCII range.
        Return """" & escapeString(value, opts.unicodeEscape) & """"
    End Function

    ''' <summary>
    ''' single-pass json string escaping. the json-grammar characters (\\, ", tab,
    ''' cr, lf, backspace, form-feed) and every other control character below 0x20
    ''' are always escaped. when <paramref name="escapeAllNonAscii"/> is true, each
    ''' non-ASCII character (>= 0x80, including every utf-16 code unit from the
    ''' supplementary planes) is escaped as \uXXXX as well.
    ''' </summary>
    Private Function escapeString(value As String, escapeAllNonAscii As Boolean) As String
        If value Is Nothing Then
            Return ""
        End If

        Dim sb As New StringBuilder(value.Length + 16)

        For Each c As Char In value
            Select Case c
                Case "\"c
                    sb.Append("\\")
                Case """"c
                    sb.Append("\""")
                Case vbCr
                    sb.Append("\r")
                Case vbLf
                    sb.Append("\n")
                Case vbTab
                    sb.Append("\t")
                Case vbBack
                    sb.Append("\b")
                Case vbFormFeed
                    sb.Append("\f")
                Case Else
                    Dim code As Integer = AscW(c)

                    If code < &H20 Then
                        ' any remaining control character (e.g. 0x00..0x1f)
                        sb.Append("\u")
                        sb.Append(code.ToString("x4", CultureInfo.InvariantCulture))
                    ElseIf escapeAllNonAscii AndAlso code > &H7F Then
                        sb.Append("\u")
                        sb.Append(code.ToString("x4", CultureInfo.InvariantCulture))
                    Else
                        sb.Append(c)
                    End If
            End Select
        Next

        Return sb.ToString()
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' always flush pending buffered text so the caller can read it
                Call json.Flush()

                ' only dispose the internal text writer (and its underlying stream)
                ' when the caller does not need to keep writing to the target stream.
                ' leaveOpen=True keeps the stream open for subsequent writes.
                If Not leaveOpen Then
                    Call json.Dispose()
                End If
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ' TODO: set large fields to null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
    ' Protected Overrides Sub Finalize()
    '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class

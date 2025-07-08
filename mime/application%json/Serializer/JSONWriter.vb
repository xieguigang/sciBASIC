Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.BSON
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.ValueTypes
Imports any = Microsoft.VisualBasic.Scripting

Friend Class JSONWriter : Implements IDisposable

    ReadOnly json As TextWriter
    ReadOnly opts As JSONSerializerOptions
    ''' <summary>
    ''' find two char
    ''' </summary>
    ReadOnly unescape As New Regex("[^\\]""", RegexOptions.Multiline)

    Dim disposedValue As Boolean

    Sub New(opts As JSONSerializerOptions, file As Stream)
        Me.opts = opts
        Me.json = New StreamWriter(file)
    End Sub

    Sub New(opts As JSONSerializerOptions, file As StringBuilder)
        Me.opts = opts
        Me.json = New StringWriter(file)
    End Sub

    Public Sub BuildJSONString(json As JsonElement)
        Call BuildJSONString(json, 0)
    End Sub

    Private Sub BuildJSONString(json As JsonElement, indent As Integer)
        If json Is Nothing OrElse (TypeOf json Is JsonValue AndAlso DirectCast(json, JsonValue).IsLiteralNull) Then
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
                        Call Me.json.WriteLine(opts.offsets(indent) & val)
                    Else
                        Call Me.json.WriteLine(val)
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

        If TypeOf value Is Date AndAlso opts.unixTimestamp Then
            Return DirectCast(value, Date).UnixTimeStamp
        ElseIf TypeOf value Is String Then
            Return encodeString(value)
        ElseIf TypeOf value Is Boolean Then
            Return value.ToString.ToLower
        ElseIf TypeOf value Is ObjectId Then
            Return $"""{value.ToString}"""
        ElseIf TypeOf value Is Double AndAlso CDbl(value).IsNaNImaginary Then
            Return """NaN"""
        Else
            ' number,integer,etc
            Return any.ToString(value)
        End If
    End Function

    ''' <summary>
    ''' {...}
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub jsonObjectString(obj As JsonObject, indent As Integer)
        If opts.indent Then
            Call json.WriteLine(opts.offsets(indent) & "{")
        Else
            Call json.Write("{")
        End If

        Dim members = obj.ToArray

        For i As Integer = 0 To members.Length - 1
            Dim member As NamedValue(Of JsonElement) = members(i)
            Dim name As String = encodeString(member.Name)
            Dim isLiteral As Boolean = TypeOf member.Value Is JsonValue

            If opts.indent AndAlso Not isLiteral Then
                Call json.WriteLine(opts.offsets(indent + 1) & name & ":")
            Else
                Call json.Write(name & ": ")
            End If

            If isLiteral Then
                Call json.Write(jsonValueString(DirectCast(member.Value, JsonValue)))
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
            Call json.WriteLine(opts.offsets(indent) & "}")
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

                For i As Integer = 0 To objs.Length - 2
                    Call BuildJSONString(objs(i), indent + 1)
                    Call json.Write(",")

                    If opts.indent Then
                        Call json.WriteLine()
                    End If
                Next

                Call BuildJSONString(objs.Last, indent + 1)
            Case GetType(String)
                ' one line per string element
                Dim strs As New List(Of String)

                For Each ele As JsonElement In arr
                    Call strs.Add(jsonValueString(DirectCast(ele, JsonValue)))
                Next

                If opts.indent Then
                    Call json.WriteLine(strs.Select(Function(si) opts.offsets(indent + 1) & si).JoinBy("," & vbCrLf))
                Else
                    Call json.Write(strs.JoinBy(","))
                End If
            Case Else
                ' in one line vector style
                Dim strs As New List(Of String)

                For Each ele As JsonElement In arr
                    Call strs.Add(jsonValueString(DirectCast(ele, JsonValue)))
                Next

                Call json.Write(strs.JoinBy(","))
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
        value = value.Replace(vbCr, vbLf)

        If opts.unicodeEscape Then
            Dim sb As New StringBuilder
            Dim code As Integer
            Dim bytes As Byte()
            Dim b1, b0 As String

            For Each c As Char In DirectCast(value, String).Replace("\", "\\")
                code = AscW(c)

                If code < 0 OrElse code > Byte.MaxValue Then
                    sb.Append("\u")
                    bytes = Encoding.Unicode.GetBytes(c)
                    b1 = bytes(1).ToString("x")
                    b0 = bytes(0).ToString("x")
                    sb.Append(If(b1.Length < 2, "0" & b1, b1))
                    sb.Append(If(b0.Length < 2, "0" & b0, b0))
                Else
                    sb.Append(c)
                End If
            Next

            value = sb.ToString.Replace(vbLf, "\n")

            If InStr(value, """") > 0 Then
                ' escape the quote symbol inside string,
                ' or json string will syntax error
                Dim unescape_quotes As String() = unescape.Matches(value).ToArray

                For Each unescape_char As String In unescape_quotes
                    value = value.Replace(
                    unescape_char,
                    unescape_char.First & "\" & unescape_char.Last
                )
                Next

                If value.First = """"c Then
                    value = "\" & value
                End If
            End If

            Return $"""{value}"""
        Else
            Return JsonContract.GetObjectJson(GetType(String), value).Replace(vbLf, "\n")
        End If
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects)
                Call json.Flush()
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

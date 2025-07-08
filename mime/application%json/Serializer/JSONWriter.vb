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
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

Friend Class JSONWriter : Implements IDisposable

    ReadOnly json As TextWriter
    ReadOnly opts As JSONSerializerOptions

    Sub New(opts As JSONSerializerOptions, file As Stream)
        Me.opts = opts
        Me.json = New StreamWriter(file)
    End Sub

    Sub New(opts As JSONSerializerOptions, file As StringBuilder)
        Me.opts = opts
        Me.json = New StringWriter(file)
    End Sub

    Public Sub BuildJSONString(json As JsonElement)
        If json Is Nothing OrElse (TypeOf json Is JsonValue AndAlso DirectCast(json, JsonValue).IsLiteralNull) Then
            Call Me.json.WriteLine("null")
        Else
            Select Case json.GetType
                Case GetType(JsonValue) : Return DirectCast(json, JsonValue).jsonValueString(opts)
                Case GetType(JsonObject) : Return DirectCast(json, JsonObject).jsonObjectString(opts)
                Case GetType(JsonArray) : Return DirectCast(json, JsonArray).jsonArrayString(opts)
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

        If value Is Nothing Then
            Return "null"
        ElseIf TypeOf value Is BSONValue Then
            value = DirectCast(value, BSONValue).GetObjectValue
        End If

        If TypeOf value Is Date AndAlso opts.unixTimestamp Then
            Return DirectCast(value, Date).UnixTimeStamp
        ElseIf TypeOf value Is String Then
            Return encodeString(value, opt)
        ElseIf TypeOf value Is Boolean Then
            Return value.ToString.ToLower
        ElseIf TypeOf value Is ObjectId Then
            Return $"""{value.ToString}"""
        ElseIf TypeOf value Is Double AndAlso CDbl(value).IsNaNImaginary Then
            Return """NaN"""
        Else
            ' number,integer,etc
            Return Any.ToString(value)
        End If
    End Function

    ''' <summary>
    ''' {...}
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    Private Function jsonObjectString(obj As JsonObject, opt As JSONSerializerOptions) As String
        Dim members As New List(Of String)

        For Each member As NamedValue(Of JsonElement) In obj
            Call members.Add($"{encodeString(member.Name, opt)}: {member.Value.BuildJsonString(opt)}")
        Next

        If opt.indent Then
            Return $"{{
            {members.JoinBy("," & Ascii.LF)}
        }}"
        Else
            Return $"{{{members.JoinBy(",")}}}"
        End If
    End Function

    ''' <summary>
    ''' [...]
    ''' </summary>
    ''' <param name="arr"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    Private Function jsonArrayString(arr As JsonArray, opt As JSONSerializerOptions) As String
        Dim a As New StringBuilder
        Dim array$() = arr _
            .Select(Function(item) item.BuildJsonString(opt)) _
            .ToArray

        If opt.indent Then
            Dim elementType As Type = arr.UnderlyingType

            Select Case elementType
                Case GetType(String)
                    ' one line per string element
                    Call a.AppendLine("[").AppendLine(array.JoinBy(", ")).AppendLine("]")
                Case GetType(Object)
                    Call a.AppendLine("[").AppendLine(array.JoinBy(", ")).AppendLine("]")
                Case Else
                    ' number, boolean in vector style
                    Call a.Append("[").Append(array.JoinBy(", ")).Append("]")
            End Select
        Else
            Call a.Append("[").Append(array.JoinBy(", ")).Append("]")
        End If

        Return a.ToString
    End Function

    ''' <summary>
    ''' find two char
    ''' </summary>
    ReadOnly unescape As New Regex("[^\\]""", RegexOptions.Multiline)
    Private disposedValue As Boolean

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

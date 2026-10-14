Imports System.Text

Namespace Runtime

    ''' <summary>
    ''' Node.js <c>Buffer</c> simulation: a byte container returned by binary fs reads
    ''' and produced by <c>Buffer.from/alloc/concat</c>. Instances expose <c>length</c>,
    ''' indexed byte access, and methods (<c>toString/slice/subarray/equals/fill/write/copy</c>).
    ''' </summary>
    Public NotInheritable Class BufferModule

        Private Sub New()
        End Sub

        ' ---------------- constructor object (Buffer.from / alloc / concat ...) ----------------

        Public Shared Function MakeModule() As JsModule
            Return New JsModule("Buffer")
        End Function

        Public Shared Function Dispatch(method As String, args As Object()) As Object
            Select Case method
                Case "from" : Return From(args)
                Case "alloc", "allocUnsafe", "allocUnsafeSlow" : Return Alloc(args)
                Case "concat" : Return Concat(args)
                Case "isBuffer" : Return TypeOf Arg(args, 0) Is BufferInstance
                Case "byteLength" : Return CDbl(ByteLength(args))
                Case "compare" : Return CDbl(Compare(args))
                Case Else : Throw JsRuntimeException.TypeError("Buffer." & method & " is not a function")
            End Select
        End Function

        Private Shared Function From(args As Object()) As BufferInstance
            Dim input = Arg(args, 0)
            Dim enc = If(args.Length > 1 AndAlso Arg(args, 1) IsNot Nothing AndAlso Arg(args, 1) IsNot JsRuntime.Undef,
                        JsRuntime.JsStr(Arg(args, 1)), "utf8")
            If TypeOf input Is BufferInstance Then
                Dim src = DirectCast(input, BufferInstance)
                Dim copy(src.Data.Length - 1) As Byte
                Array.Copy(src.Data, copy, src.Data.Length)
                Return New BufferInstance(copy)
            End If
            If TypeOf input Is String Then
                Return New BufferInstance(Encode(enc, CStr(input)))
            End If
            If JsRuntime.IsJsArray(input) Then
                Dim a = DirectCast(input, List(Of Object))
                Dim b(a.Count - 1) As Byte
                For i = 0 To a.Count - 1
                    b(i) = CByte(JsRuntime.JsNum(a(i)) And &HFF)
                Next
                Return New BufferInstance(b)
            End If
            Throw JsRuntimeException.TypeError("Buffer.from expects a string, array or Buffer")
        End Function

        Private Shared Function Alloc(args As Object()) As BufferInstance
            Dim size = CInt(System.Math.Floor(JsRuntime.JsNum(Arg(args, 0))))
            If size < 0 Then Throw JsRuntimeException.RangeError("Buffer allocation size must be a positive number")
            Dim b(size - 1) As Byte
            If args.Length > 1 AndAlso Arg(args, 1) IsNot Nothing AndAlso Arg(args, 1) IsNot JsRuntime.Undef Then
                Dim fill = Arg(args, 1)
                If TypeOf fill Is BufferInstance Then
                    Dim src = DirectCast(fill, BufferInstance)
                    If src.Data.Length > 0 Then
                        For i = 0 To size - 1 : b(i) = src.Data(i Mod src.Data.Length) : Next
                    End If
                    Return New BufferInstance(b)
                End If
                Dim value = CByte(JsRuntime.JsNum(fill) And &HFF)
                For i = 0 To size - 1 : b(i) = value : Next
            End If
            Return New BufferInstance(b)
        End Function

        Private Shared Function Concat(args As Object()) As BufferInstance
            Dim list = DirectCast(Arg(args, 0), List(Of Object))
            Dim total = 0
            For Each item In list
                Dim bi = TryCast(item, BufferInstance)
                If bi IsNot Nothing Then total += bi.Data.Length
            Next
            Dim out(total - 1) As Byte
            Dim pos = 0
            For Each item In list
                Dim bi = TryCast(item, BufferInstance)
                If bi IsNot Nothing Then
                    Array.Copy(bi.Data, 0, out, pos, bi.Data.Length)
                    pos += bi.Data.Length
                End If
            Next
            Return New BufferInstance(out)
        End Function

        Private Shared Function ByteLength(args As Object()) As Integer
            Dim input = Arg(args, 0)
            If TypeOf input Is BufferInstance Then Return DirectCast(input, BufferInstance).Data.Length
            Dim enc = If(args.Length > 1 AndAlso Arg(args, 1) IsNot Nothing, JsRuntime.JsStr(Arg(args, 1)), "utf8")
            Return Encode(enc, JsRuntime.JsStr(input)).Length
        End Function

        Private Shared Function Compare(args As Object()) As Integer
            Dim a = DirectCast(Arg(args, 0), BufferInstance).Data
            Dim b = DirectCast(Arg(args, 1), BufferInstance).Data
            Dim n = System.Math.Min(a.Length, b.Length)
            For i = 0 To n - 1
                If a(i) < b(i) Then Return -1
                If a(i) > b(i) Then Return 1
            Next
            Return a.Length.CompareTo(b.Length)
        End Function

        ' ---------------- instance methods ----------------

        Public Shared Function DispatchInstance(target As BufferInstance, method As String, args As Object()) As Object
            Select Case method
                Case "toString"
                    Return target.ToString(If(args.Length > 0, JsRuntime.JsStr(Arg(args, 0)), "utf8"),
                                           If(args.Length > 1, CInt(JsRuntime.JsNum(Arg(args, 1))), 0),
                                           If(args.Length > 2, CInt(JsRuntime.JsNum(Arg(args, 2))), target.Data.Length))
                Case "slice", "subarray"
                    Return target.Slice(If(args.Length > 0, CInt(JsRuntime.JsNum(Arg(args, 0))), 0),
                                        If(args.Length > 1, CInt(JsRuntime.JsNum(Arg(args, 1))), target.Data.Length))
                Case "equals"
                    Dim o = TryCast(Arg(args, 0), BufferInstance)
                    Return o IsNot Nothing AndAlso target.EqualsBuf(o)
                Case "fill"
                    Dim v = CByte(JsRuntime.JsNum(Arg(args, 0)) And &HFF)
                    Dim s = If(args.Length > 1, CInt(JsRuntime.JsNum(Arg(args, 1))), 0)
                    Dim e = If(args.Length > 2, CInt(JsRuntime.JsNum(Arg(args, 2))), target.Data.Length)
                    target.Fill(v, s, e) : Return target
                Case "write"
                    Dim enc = If(args.Length > 2 AndAlso Arg(args, 2) IsNot Nothing, JsRuntime.JsStr(Arg(args, 2)), "utf8")
                    Dim off = If(args.Length > 1, CInt(JsRuntime.JsNum(Arg(args, 1))), 0)
                    Dim bytes = Encode(enc, JsRuntime.JsStr(Arg(args, 0)))
                    Array.Copy(bytes, 0, target.Data, off, System.Math.Min(bytes.Length, target.Data.Length - off))
                    Return CDbl(System.Math.Min(bytes.Length, target.Data.Length - off))
                Case "copy"
                    Dim dst = DirectCast(Arg(args, 0), BufferInstance)
                    Dim tStart = If(args.Length > 1, CInt(JsRuntime.JsNum(Arg(args, 1))), 0)
                    Dim sStart = If(args.Length > 2, CInt(JsRuntime.JsNum(Arg(args, 2))), 0)
                    Dim sEnd = If(args.Length > 3, CInt(JsRuntime.JsNum(Arg(args, 3))), target.Data.Length)
                    Dim count = System.Math.Min(sEnd, target.Data.Length) - sStart
                    Array.Copy(target.Data, sStart, dst.Data, tStart, count)
                    Return CDbl(count)
                Case "includes", "indexOf"
                    Dim needle = Arg(args, 0)
                    Dim nb As Byte()
                    If TypeOf needle Is BufferInstance Then
                        nb = DirectCast(needle, BufferInstance).Data
                    Else
                        nb = Encode("utf8", JsRuntime.JsStr(needle))
                    End If
                    Dim found = target.IndexOf(nb)
                    If method = "includes" Then Return found >= 0
                    Return CDbl(found)
                Case "toJSON" : Return target.ToArrayObject()
                Case Else : Throw JsRuntimeException.TypeError("Buffer.prototype." & method & " is not a function")
            End Select
        End Function

        ' ---------------- encoding helpers ----------------

        Friend Shared Function Encode(enc As String, text As String) As Byte()
            Select Case LCase(If(enc, "utf8"))
                Case "ascii" : Return ASCIIEncoding.ASCII.GetBytes(text)
                Case "latin1", "binary" : Return Latin1.GetBytes(text)
                Case "base64" : Return Convert.FromBase64String(NormalizeBase64(text))
                Case "hex" : Return FromHex(text)
                Case Else : Return Encoding.UTF8.GetBytes(text)
            End Select
        End Function

        Friend Shared Function Decode(enc As String, bytes As Byte(), start As Integer, [end] As Integer) As String
            Dim s = System.Math.Max(0, start)
            Dim e = System.Math.Min(bytes.Length, [end])
            If s > e Then Return ""
            Dim slice(e - s - 1) As Byte
            Array.Copy(bytes, s, slice, 0, e - s)
            Select Case LCase(If(enc, "utf8"))
                Case "ascii" : Return ASCIIEncoding.ASCII.GetString(slice)
                Case "latin1", "binary" : Return Latin1.GetString(slice)
                Case "base64" : Return Convert.ToBase64String(slice)
                Case "hex" : Return ToHex(slice)
                Case Else : Return Encoding.UTF8.GetString(slice)
            End Select
        End Function

        Private Shared ReadOnly Latin1 As Encoding = Encoding.GetEncoding("ISO-8859-1")

        Private Shared Function NormalizeBase64(text As String) As String
            Dim t = text.Trim()
            Dim pad = t.Length Mod 4
            If pad = 2 Then t &= "=="
            If pad = 3 Then t &= "="
            Return t
        End Function

        Private Shared Function FromHex(text As String) As Byte()
            Dim sb As New StringBuilder(text.Trim())
            sb.Replace(vbCr, "").Replace(vbLf, "").Replace(" ", "")
            Dim t = sb.ToString()
            If t.Length Mod 2 <> 0 Then Throw JsRuntimeException.TypeError("Invalid hex string")
            Dim out(t.Length \ 2 - 1) As Byte
            For i = 0 To out.Length - 1
                out(i) = Convert.ToByte(t.Substring(i * 2, 2), 16)
            Next
            Return out
        End Function

        Private Shared Function ToHex(bytes As Byte()) As String
            Dim sb As New StringBuilder()
            For Each b In bytes : sb.Append(b.ToString("x2")) : Next
            Return sb.ToString()
        End Function

        Private Shared Function Arg(args As Object(), i As Integer) As Object
            If i < args.Length Then Return args(i)
            Return JsRuntime.Undef
        End Function

    End Class

    ''' <summary>Mutable byte buffer, the runtime value of a Node <c>Buffer</c>.</summary>
    Public Class BufferInstance

        Public Data As Byte()

        Public Sub New(data As Byte())
            Me.Data = data
        End Sub

        Public ReadOnly Property Length As Integer
            Get
                Return Data.Length
            End Get
        End Property

        Public Function Slice(start As Integer, [end] As Integer) As BufferInstance
            Dim s = If(start < 0, Data.Length + start, start)
            Dim e = If([end] < 0, Data.Length + [end], [end])
            s = System.Math.Max(0, s)
            e = System.Math.Min(Data.Length, e)
            If s > e Then e = s
            Dim out(e - s - 1) As Byte
            Array.Copy(Data, s, out, 0, e - s)
            Return New BufferInstance(out)
        End Function

        Public Sub Fill(value As Byte, start As Integer, [end] As Integer)
            Dim s = System.Math.Max(0, start)
            Dim e = System.Math.Min(Data.Length, [end])
            For i = s To e - 1 : Data(i) = value : Next
        End Sub

        Public Function EqualsBuf(other As BufferInstance) As Boolean
            If Data.Length <> other.Data.Length Then Return False
            For i = 0 To Data.Length - 1
                If Data(i) <> other.Data(i) Then Return False
            Next
            Return True
        End Function

        Public Function IndexOf(needle As Byte()) As Integer
            If needle.Length = 0 Then Return 0
            For i = 0 To Data.Length - needle.Length
                Dim ok = True
                For j = 0 To needle.Length - 1
                    If Data(i + j) <> needle(j) Then ok = False : Exit For
                Next
                If ok Then Return i
            Next
            Return -1
        End Function

        Public Function ToArrayObject() As List(Of Object)
            Dim a As New List(Of Object)(Data.Length)
            For Each b In Data : a.Add(CDbl(b)) : Next
            Return a
        End Function

        Public Overrides Function ToString() As String
            If Data.Length = 0 Then Return "<Buffer >"
            Dim limit = System.Math.Min(Data.Length, 50)
            Dim sb As New StringBuilder("<Buffer")
            For i = 0 To limit - 1
                sb.Append(" " & Data(i).ToString("x2"))
            Next
            If Data.Length > limit Then sb.Append(" ... " & Data.Length & " more bytes")
            sb.Append(">")
            Return sb.ToString()
        End Function

        Public Overloads Function ToString(enc As String, start As Integer, [end] As Integer) As String
            Return BufferModule.Decode(enc, Data, start, [end])
        End Function

    End Class

End Namespace

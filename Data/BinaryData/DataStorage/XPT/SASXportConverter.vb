Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Xpt.Types
Imports std = System.Math

Namespace Xpt

    Public Class SASXportConverter
        Implements IDisposable

        Public Shared LINE_LEN As Integer = 80

        Public Shared NAMESTR_LEN As Integer = 140

        Public Shared SAS_COLUMN_TYPE_CHR As Short = &H2


        Protected Friend processBlankRecords As Boolean = False
        Protected Friend doneField As Boolean = False
        Protected Friend convertDate9ToString As Boolean = True
        Protected Friend rawin As Stream

        Protected Friend ctx As XPTContext
        Protected Friend [in] As StreamReader

        Protected Friend DUMMY_BUFFER As Byte() = New Byte(LINE_LEN - 1) {}

        Protected Friend offsetField As Long = 0
        Protected Friend num_blank_rows As Integer = 0
        Protected Friend rowCountField As Integer = 0

        Protected Friend rowField As Byte() = Nothing
        Protected Friend blank_row As Byte() = Nothing

        Protected Friend recordField As IList(Of String) = Nothing
        Protected Friend primitiveRecordField As IList(Of ReadstatValue) = Nothing

        Public Sub New(fileName As String)
            Me.New(New FileStream(fileName, FileMode.Open, FileAccess.Read))
        End Sub

        Public Sub New([in] As Stream)
            rawin = [in]
            init()
        End Sub

        Private Sub init()
            [in] = New StreamReader(rawin)
            ctx = New XPTContext()
            PrimitiveUtils.memset(blank_row, AscW(" "c), ctx.row_length)
            readMeta()
            readNextRecord()
        End Sub

        Private Sub xport_read_record(record As Byte())
            read_bytes(record, LINE_LEN)
        End Sub

        Private Function xport_read_header_record() As XPTHeader

            Dim header As XPTHeader = New XPTHeader()
            Dim line As Byte() = createDefaultBuffer()

            xport_read_record(line)

            Dim offset = 20
            header.name = IO.readString(line, offset, 8)
            offset += 8 + 20
            header.num1 = Integer.Parse(IO.readString(line, offset, 5))
            offset += 5
            header.num2 = Integer.Parse(IO.readString(line, offset, 5))
            offset += 5
            header.num3 = Integer.Parse(IO.readString(line, offset, 5))
            offset += 5
            header.num4 = Integer.Parse(IO.readString(line, offset, 5))
            offset += 5
            header.num5 = Integer.Parse(IO.readString(line, offset, 5))
            offset += 5
            header.num6 = Integer.Parse(IO.readString(line, offset, 5))
            offset += 5
            Return header
        End Function

        Private Function xport_read_library_record() As XPTHeader

            Dim xrecord As XPTHeader = xport_read_header_record()

            If "LIBRARY".Equals(xrecord.name, StringComparison.OrdinalIgnoreCase) Then
                ctx.version = 5
            ElseIf "LIBV8".Equals(xrecord.name, StringComparison.OrdinalIgnoreCase) Then
                ctx.version = 8
            Else
                Throw New InvalidDataException("Unknows XPT File Version - " & ctx.version.ToString())
            End If
            Return xrecord
        End Function

        Private Sub xport_skip_record()
            xport_read_record(DUMMY_BUFFER)
        End Sub

        Private Sub xport_skip_rest_of_record(pos As Integer)
            Dim len = LINE_LEN - pos Mod LINE_LEN

            If len = LINE_LEN Then
                Return
            End If
            read_bytes(DUMMY_BUFFER, len)
        End Sub

        Private Function xport_read_timestamp_record() As TimeStamp

            Dim line As Byte() = createDefaultBuffer()

            Dim ts As TimeStamp = New TimeStamp()
            Dim month As String

            xport_read_record(line)

            Dim offset = 0
            ts.tm_mday = Short.Parse(IO.readString(line, offset, 2))
            offset += 2
            month = IO.readString(line, offset, 3)
            offset += 3
            ts.tm_year = Short.Parse(IO.readString(line, offset, 2))
            offset += 3
            ts.tm_hour = Short.Parse(IO.readString(line, offset, 2))
            offset += 3
            ts.tm_min = Short.Parse(IO.readString(line, offset, 2))
            offset += 3
            ts.tm_sec = Short.Parse(IO.readString(line, offset, 2))
            offset += 3
            For i As Short = 0 To XPTTypes.XPORT_MONTHS.Length - 1
                If XPTTypes.XPORT_MONTHS(i).Equals(month, StringComparison.OrdinalIgnoreCase) Then
                    ts.tm_mon = i
                    Exit For
                End If
            Next
            If ts.tm_year < 60 Then
                ts.tm_year += 2000
            ElseIf ts.tm_year < 100 Then
                ts.tm_year += 1900
            End If
            ctx.timestamp = createTS(ts)
            Return ts
        End Function

        Private Function createDefaultBuffer() As Byte()
            Return createBuffer(LINE_LEN)
        End Function

        Private Function createBuffer(len As Integer) As Byte()
            Dim line = New Byte(len - 1) {}
            Return line
        End Function

        Private Function createTS(ts As TimeStamp) As Long

            Dim cal As Date = New DateTime()
            cal = New DateTime(ts.tm_year, ts.tm_mon, ts.tm_mday, ts.tm_hour, ts.tm_min, ts.tm_sec)
            Return cal.Ticks
        End Function

        Private Function xport_expect_header_record(v5_name As String, v8_name As String) As XPTHeader

            Dim xrecord As XPTHeader = xport_read_header_record()

            If ctx.version = 5 AndAlso Not v5_name.Equals(xrecord.name, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidDataException("Wrong XPT Header Record - " & xrecord.name)
            ElseIf ctx.version = 8 AndAlso Not v8_name.Equals(xrecord.name, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidDataException("Wrong XPT Header Record - " & xrecord.name)
            End If

            Return xrecord
        End Function

        Private Sub xport_read_table_name_record()

            Dim line As Byte() = createDefaultBuffer()
            xport_read_record(line)

            Dim dst = createBuffer(129)
            Dim src_len = If(ctx.version = 5, 8, 32)
            ctx.table_name = IO.readString(dst, 8, src_len)
        End Sub

        Private Sub xport_read_file_label_record()

            Dim line As Byte() = createDefaultBuffer()
            xport_read_record(line)

            Dim dst = createBuffer(161)
            Dim src_len = 40
            ctx.file_label = IO.readString(dst, 32, src_len)
        End Sub

        Private Sub xport_read_namestr_header_record()

            Dim xrecord As XPTHeader = xport_read_header_record()

            If ctx.version = 5 AndAlso Not "NAMESTR".Equals(xrecord.name, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidDataException("Wrong XPT Header Record - " & xrecord.name)
            ElseIf ctx.version = 8 AndAlso Not "NAMSTV8".Equals(xrecord.name, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidDataException("Wrong XPT Header Record - " & xrecord.name)
            End If

            ctx.var_count = xrecord.num2
            ctx.variables = New ReadStatVariable(ctx.var_count - 1) {}
        End Sub

        Private Function xport_read_variables() As IList(Of XPTNameString)

            Dim nstr As IList(Of XPTNameString) = New List(Of XPTNameString)()
            Dim read = 0
            For i = 0 To ctx.var_count - 1

                Dim buffer = New Byte(NAMESTR_LEN - 1) {}
                Dim bytes = read_bytes(buffer, NAMESTR_LEN)
                read += bytes
                Dim namestr = xport_namestr_bswap(buffer)

                Dim variable As ReadStatVariable = New ReadStatVariable()

                variable.index = i
                variable.type = If(namestr.ntype = SAS_COLUMN_TYPE_CHR, ReadstatType.READSTAT_TYPE_STRING, ReadstatType.READSTAT_TYPE_DOUBLE)
                variable.storage_width = namestr.nlng
                variable.display_width = namestr.nfl
                variable.decimals = namestr.nfd
                variable.alignment = If(namestr.nfj > 0, ReadstatAlignment.READSTAT_ALIGNMENT_RIGHT, ReadstatAlignment.READSTAT_ALIGNMENT_LEFT)

                variable.name = namestr.nname
                variable.label = namestr.nlabel
                variable.format = xport_construct_format(namestr.nform, variable.display_width, variable.decimals)
                ' todo format;
                ctx.variables(i) = variable

                nstr.Add(namestr)
            Next

            xport_skip_rest_of_record(read)

            If ctx.version = 5 Then
                xport_read_obs_header_record()
            Else
                Dim xrecord As XPTHeader = xport_read_header_record()
                ' void 
                If "OBSV8".Equals(xrecord.name, StringComparison.OrdinalIgnoreCase) Then
                ElseIf "LABELV8".Equals(xrecord.name, StringComparison.OrdinalIgnoreCase) Then
                    xport_read_labels_v8(xrecord.num1)
                ElseIf "LABELV9".Equals(xrecord.name, StringComparison.OrdinalIgnoreCase) Then
                    xport_read_labels_v9(xrecord.num1)
                End If
            End If

            ctx.row_length = 0

            Dim index_after_skipping = 0

            For i = 0 To ctx.var_count - 1
                Dim variable = ctx.variables(i)
                variable.index_after_skipping = index_after_skipping
                ' todo deleted code for index after skipping
                ctx.row_length += variable.storage_width
            Next
            Return nstr
        End Function

        Private Sub xport_read_labels_v8(label_count As Integer)

            Dim labeldef = New Byte(5) {}
            Dim read = 0
            For i = 0 To label_count - 1
                Dim index, name_len, label_len As Integer
                Dim bytes = read_bytes(labeldef, 6)
                read += bytes
                Dim bb = ByteBuffer.wrap(labeldef).order(ByteOrder.BigEndian)
                index = bb.short()
                name_len = bb.short()
                label_len = bb.short()

                If index >= ctx.var_count Then
                    Throw New InvalidDataException("Invalid index")
                End If

                Dim name = New Byte(name_len - 1) {}
                Dim label = New Byte(label_len - 1) {}
                Dim variable = ctx.variables(index)

                read_bytes(name, name_len)
                read_bytes(label, label_len)
                variable.name = IO.readString(name, 0, name_len)
                variable.label = IO.readString(label, 0, label_len)
            Next
            xport_skip_rest_of_record(read)
            xport_read_obs_header_record()
        End Sub

        Private Sub xport_read_labels_v9(label_count As Integer)

            Dim labeldef = New Byte(9) {}
            Dim read = 0
            For i = 0 To label_count - 1
                Dim index, name_len, format_len, informat_len, label_len As Integer
                Dim bytes = read_bytes(labeldef, 10)
                read += bytes
                Dim bb = ByteBuffer.wrap(labeldef).order(ByteOrder.BigEndian)
                index = bb.short()
                name_len = bb.short()
                format_len = bb.short()
                informat_len = bb.short()
                label_len = bb.short()

                If index >= ctx.var_count Then
                    Throw New InvalidDataException("Invalid index")
                End If

                Dim name = New Byte(name_len - 1) {}
                Dim format = New Byte(format_len - 1) {}
                Dim informat = New Byte(informat_len - 1) {}
                Dim label = New Byte(label_len - 1) {}

                Dim variable = ctx.variables(index)

                read_bytes(name, name_len)
                read_bytes(format, format_len)
                read_bytes(informat, informat_len)
                read_bytes(label, label_len)

                variable.name = IO.readString(name, 0, name_len)
                variable.label = IO.readString(label, 0, label_len)
                variable.format = IO.readString(label, 0, label_len)
                variable.format = xport_construct_format(variable.format, variable.display_width, variable.decimals)
            Next
            xport_skip_rest_of_record(read)
            xport_read_obs_header_record()
        End Sub

        Private Function xport_read_obs_header_record() As XPTHeader
            Return xport_expect_header_record("OBS", "OBSV8")
        End Function

        Private Function xport_namestr_bswap(buffer As Byte()) As XPTNameString

            Dim namestr As XPTNameString = New XPTNameString()

            Dim offset = 0
            Dim bb = ByteBuffer.wrap(buffer).order(ByteOrder.BigEndian)

            namestr.ntype = bb.short()
            offset += 2
            namestr.nhfun = bb.short()
            offset += 2
            namestr.nlng = bb.short()
            offset += 2
            namestr.nvar0 = bb.short()
            offset += 2

            namestr.nname = IO.readString(buffer, offset, 8)
            offset += 8
            namestr.nlabel = IO.readString(buffer, offset, 40)
            offset += 40
            namestr.nform = IO.readString(buffer, offset, 8)
            offset += 8

            bb.position(offset)
            namestr.nfl = bb.short()
            offset += 2
            namestr.nfd = bb.short()
            offset += 2
            namestr.nfj = bb.short()
            offset += 2

            namestr.nfill = IO.readString(buffer, offset, 2)
            offset += 2
            namestr.niform = IO.readString(buffer, offset, 8)
            offset += 8

            bb.position(offset)
            namestr.nifl = bb.short()
            offset += 2
            namestr.nifd = bb.short()
            offset += 2
            namestr.npos = bb.int()
            offset += 4

            namestr.longname = IO.readString(buffer, offset, 32)
            offset += 32

            bb.position(offset)
            namestr.labeln = bb.short()
            offset += 2

            namestr.rest = IO.readString(buffer, offset, 18)
            offset += 18
            Return namestr
        End Function

        Private Function xport_construct_format(format As String, width As Integer, decimals As Integer) As String

            If decimals > 0 Then
                Return String.Format("{0}{1:D}.{2:D}", format, width, decimals)
            ElseIf width > 0 Then
                Return String.Format("{0}{1:D}", format, width)
            Else
                Return String.Format("{0}", format)
            End If
        End Function

        Protected Friend Overridable Sub readstat_convert(dst As Byte(), dst_len As Integer, src As Byte(), src_off As Integer, src_len As Integer)

            While src_len > 0 AndAlso src(src_off + src_len - 1) = Microsoft.VisualBasic.AscW(" "c)
                src_len -= 1
            End While

            If dst_len = 0 Then
                Throw New Exception("Destination lenght is 0.")
            ElseIf src_len + 1 > dst_len Then
                Throw New Exception("Source Lenght is greater than Destination lenght.")
            Else
                For i = 0 To src_len - 1
                    dst(i) = src(src_off + i)
                Next
            End If
        End Sub

        Private Function isBlankRow(row As Byte()) As Boolean
            Dim row_is_blank = True
            For pos = 0 To ctx.row_length - 1
                If row(pos) <> Microsoft.VisualBasic.AscW(" "c) Then
                    row_is_blank = False
                    Exit For
                End If
            Next
            Return row_is_blank
        End Function

        Private Function sas_validate_tag(tag As Byte) As Boolean
            If tag = Microsoft.VisualBasic.AscW("_"c) OrElse tag >= Microsoft.VisualBasic.AscW("A"c) AndAlso tag <= Microsoft.VisualBasic.AscW("Z"c) Then
                Return True
            End If

            Return False
        End Function

        Private Function read_bytes(buffer As Byte(), len As Integer) As Integer
            Dim off = len
            Try
                [in].BaseStream.Read(buffer, 0, len)
            Catch __unusedException1__ As Exception
                Console.WriteLine("!!WARN!! Reached EOF before read_fully, Offset: " & offsetField.ToString())
                Return -1
            End Try
            offsetField += off
            Return off
        End Function

        Public Overridable Property Debug As Boolean

        Public Overridable ReadOnly Property Done As Boolean
            Get
                Return doneField
            End Get
        End Property

        Public Overridable ReadOnly Property MetaData As XPTContext
            Get
                Return ctx
            End Get
        End Property

        Public Overridable ReadOnly Property Offset As Long
            Get
                Return offsetField
            End Get
        End Property

        Public Overridable ReadOnly Property Row As Byte()
            Get
                Return rowField
            End Get
        End Property

        Public Overridable ReadOnly Property RowCount As Integer
            Get
                Return rowCountField
            End Get
        End Property

        Public Overridable ReadOnly Property Record As IList(Of String)
            Get
                Return recordField
            End Get
        End Property

        Public Overridable ReadOnly Property PrimitiveRecord As IList(Of ReadstatValue)
            Get
                Return primitiveRecordField
            End Get
        End Property

        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            doneField = True
            If [in] Is Nothing Then
                Return
            End If

            [in].Close()
            [in] = Nothing
        End Sub

        Protected Friend Overridable Sub seek(offset As Integer)
            Dim len = 0
            Do
                len = std.Min(ctx.row_length, offset)
                Dim read = read_bytes(DUMMY_BUFFER, len)
                If read <= 0 Then
                    doneField = True
                    Dispose()
                    Exit Do
                End If
                offset -= read
            Loop While offset > 0
        End Sub

        Protected Friend Overridable Sub readNextRecord()

            If doneField Then
                Return
            End If

            While True
                rowCountField += 1
                Dim bytes_read = read_bytes(rowField, ctx.row_length)
                If bytes_read < ctx.row_length Then
                    doneField = True
                    Exit While
                End If
                If isBlankRow(rowField) Then
                    num_blank_rows += 1
                    Continue While
                Else
                    Exit While
                End If
            End While
            If doneField Then
                Dispose()
                Return
            End If
            If processBlankRecords Then
                While num_blank_rows > 0
                    processRecord(blank_row, ctx.row_length)
                    If Threading.Interlocked.Increment((ctx.parsed_row_count)) = ctx.row_limit Then
                        doneField = True
                        Throw New Exception("Invalid read situation.")
                    End If
                    num_blank_rows -= 1
                End While
            End If

            processRecord(rowField, ctx.row_length)

            If Threading.Interlocked.Increment((ctx.parsed_row_count)) = ctx.row_limit Then
                doneField = True
            End If
        End Sub

        Protected Friend Overridable Sub processRecord(row As Byte(), row_length As Integer)

            Dim pos = 0
            Dim [string] As String = Nothing
            recordField = New List(Of String)()
            primitiveRecordField = New List(Of ReadstatValue)()

            For i = 0 To ctx.var_count - 1
                Dim variable = ctx.variables(i)
                Dim value As ReadstatValue = New ReadstatValue()
                value.type = variable.type

                If variable.type = ReadstatType.READSTAT_TYPE_STRING Then
                    [string] = IO.readString(row, pos, variable.storage_width)
                    If Debug Then
                        Console.Write(" < " & [string] & " >, ")
                    End If
                    value.tvalue = [string]
                    recordField.Add([string])
                Else
                    Dim dval = 0.0R
                    If variable.storage_width <= XPTTypes.XPORT_MAX_DOUBLE_SIZE AndAlso variable.storage_width >= XPTTypes.XPORT_MIN_DOUBLE_SIZE Then
                        Dim full_value = New Byte(7) {}
                        If PrimitiveUtils.memcmp(full_value, 1, row, pos + 1, variable.storage_width - 1) AndAlso (row(pos) = Microsoft.VisualBasic.AscW("."c) OrElse sas_validate_tag(row(pos))) Then
                            If row(pos) = Microsoft.VisualBasic.AscW("."c) Then
                                value.is_system_missing = 1
                            Else
                                value.tag = row(pos)
                                value.is_tagged_missing = 1
                            End If
                        Else
                            PrimitiveUtils.memcpy(full_value, 0, row, pos, variable.storage_width)
                            dval = PrimitiveUtils.xpt2ieeeSimple(full_value)
                        End If
                    End If
                    value.value = dval
                    Dim val As String = "" & dval.ToString()
                    If convertDate9ToString AndAlso dval <> 0 AndAlso variable.format.ToLower().Contains("date") Then
                        val = XPTReaderUtils.convertSASDate9ToString(variable.format.ToLower(), dval)
                    End If
                    recordField.Add(val)
                    If Debug Then
                        Console.Write(value.value.ToString() & ", ")
                    End If
                End If
                primitiveRecordField.Add(value)
                pos += variable.storage_width
            Next
            If Debug Then
                Console.WriteLine()
            End If
        End Sub

        Public Overridable Sub readMeta()

            Dim header As XPTHeader = xport_read_library_record()
            '  Console.WriteLine((new Gson()).toJson(header));

            xport_skip_record()

            Dim ts As TimeStamp = xport_read_timestamp_record()
            '  Console.WriteLine((new Gson()).toJson(ts));

            Dim memberHeader = xport_expect_header_record("MEMBER", "MEMBV8")
            ' Console.WriteLine((new Gson()).toJson(memberHeader));

            Dim descHeader = xport_expect_header_record("DSCRPTR", "DSCPTV8")
            ' Console.WriteLine((new Gson()).toJson(descHeader));

            xport_read_table_name_record()

            xport_read_file_label_record()

            xport_read_namestr_header_record()

            Dim nstrs As IList(Of XPTNameString) = xport_read_variables()
            '  Console.WriteLine((new Gson()).toJson(nstrs));

            ' Console.WriteLine((new Gson()).toJson(ctx));

            If ctx.row_length = 0 Then
                doneField = True
                Dispose()
            Else
                rowField = New Byte(ctx.row_length - 1) {}
                blank_row = New Byte(ctx.row_length - 1) {}
            End If
        End Sub
    End Class
End Namespace

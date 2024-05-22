#Region "Microsoft.VisualBasic::409ed0e766c4de07998844781a1ddd15, Data\BinaryData\netCDF\CDFWriter.vb"

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

    '   Total Lines: 587
    '    Code Lines: 243 (41.40%)
    ' Comment Lines: 280 (47.70%)
    '    - Xml Docs: 34.29%
    ' 
    '   Blank Lines: 64 (10.90%)
    '     File Size: 25.68 KB


    ' Class CDFWriter
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: CalcOffsets, Dimensions, getDimension, getDimensionList, getVariableHeaderBuffer
    '               (+4 Overloads) GlobalAttributes
    ' 
    '     Sub: (+3 Overloads) AddVariable, (+5 Overloads) AddVector, (+2 Overloads) Dispose, Flush, Save
    '          writeAttributes
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

''' <summary>
''' 这个对象没有显式调用的文件写函数,必须要通过<see cref="IDisposable"/>接口来完成文件数据的写操作
''' </summary>
''' <remarks>
''' CDF文件之中，字符串仅允许ASCII字符，因为CDF文件之中的字符只有一个字节长度
''' 如果要写入中文之类的非ASCII编码的字符串的话，只能够首先转换为字节流，然后再写入到variable的数据之中
''' </remarks>
Public Class CDFWriter : Implements IDisposable

#Region ""

    ' netcdf_file  = header  data
    ' header       = magic  numrecs  dim_list  gatt_list  var_list
    ' magic        = 'C'  'D'  'F'  VERSION
    ' VERSION      = \x01 |                      // classic format
    ' \x02                        // 64-bit offset format
    ' numrecs      = NON_NEG | STREAMING         // length of record dimension
    ' dim_list     = ABSENT | NC_DIMENSION  nelems  [dim ...]
    ' gatt_list    = att_list                    // global attributes
    ' att_list     = ABSENT | NC_ATTRIBUTE  nelems  [attr ...]
    ' var_list     = ABSENT | NC_VARIABLE   nelems  [var ...]
    ' ABSENT       = ZERO  ZERO                  // Means list is not present
    ' ZERO         = \x00 \x00 \x00 \x00         // 32-bit zero
    ' NC_DIMENSION = \x00 \x00 \x00 \x0A         // tag for list of dimensions
    ' NC_VARIABLE  = \x00 \x00 \x00 \x0B         // tag for list of variables
    ' NC_ATTRIBUTE = \x00 \x00 \x00 \x0C         // tag for list of attributes
    ' nelems       = NON_NEG       // number of elements in following sequence
    ' dim          = name  dim_length
    ' name         = nelems  namestring
    ' // Names a dimension, variable, or attribute.
    ' // Names should match the regular expression
    ' // ([a-zA-Z0-9_]|{MUTF8})([^\x00-\x1F/\x7F-\xFF]|{MUTF8})*
    ' // For other constraints, see "Note on names", below.
    ' namestring   = ID1 [IDN ...] padding
    ' ID1          = alphanumeric | '_'
    ' IDN          = alphanumeric | special1 | special2
    ' alphanumeric = lowercase | uppercase | numeric | MUTF8
    ' lowercase    = 'a'|'b'|'c'|'d'|'e'|'f'|'g'|'h'|'i'|'j'|'k'|'l'|'m'|
    ' 'n'|'o'|'p'|'q'|'r'|'s'|'t'|'u'|'v'|'w'|'x'|'y'|'z'
    ' uppercase    = 'A'|'B'|'C'|'D'|'E'|'F'|'G'|'H'|'I'|'J'|'K'|'L'|'M'|
    ' 'N'|'O'|'P'|'Q'|'R'|'S'|'T'|'U'|'V'|'W'|'X'|'Y'|'Z'
    ' numeric      = '0'|'1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9'
    ' // special1 chars have traditionally been
    ' // permitted in netCDF names.
    ' special1     = '_'|'.'|'@'|'+'|'-'
    ' // special2 chars are recently permitted in
    ' // names (and require escaping in CDL).
    ' // Note: '/' is not permitted.
    ' special2     = ' ' | '!' | '"' | '#'  | '$' | '' | '&' | '\'' |
    ' '(' | ')' | '*' | ','  | ':' | ';' | '<' | '='  |
    ' '>' | '?' | '[' | '\' | ']' | '^' | '‘’ | '{'  |
    ' '|' | '}' | '~'
    ' MUTF8        = <multibyte UTF-8 encoded, NFC-normalized Unicode character>
    ' dim_length   = NON_NEG       // If zero, this is the record dimension.
    ' // There can be at most one record dimension.
    ' attr         = name  nc_type  nelems  [values ...]
    ' nc_type      = NC_BYTE   |
    ' NC_CHAR   |
    ' NC_SHORT  |
    ' NC_INT    |
    ' NC_FLOAT  |
    ' NC_DOUBLE
    ' var          = name  nelems  [dimid ...]  vatt_list  nc_type  vsize  begin
    ' // nelems is the dimensionality (rank) of the
    ' // variable: 0 for scalar, 1 for vector, 2
    ' // for matrix, ...
    ' dimid        = NON_NEG       // Dimension ID (index into dim_list) for
    ' // variable shape.  We say this is a "record
    ' // variable" if and only if the first
    ' // dimension is the record dimension.
    ' vatt_list    = att_list      // Variable-specific attributes
    ' vsize        = NON_NEG       // Variable size.  If not a record variable,
    ' // the amount of space in bytes allocated to
    ' // the variable's data.  If a record variable,
    ' // the amount of space per record.  See "Note
    ' // on vsize", below.
    ' begin        = OFFSET        // Variable start location.  The offset in
    ' // bytes (seek index) in the file of the
    ' // beginning of data for this variable.
    ' data         = non_recs  recs
    ' non_recs     = [vardata ...] // The data for all non-record variables,
    ' // stored contiguously for each variable, in
    ' // the same order the variables occur in the
    ' // header.
    ' vardata      = [values ...]  // All data for a non-record variable, as a
    ' // block of values of the same type as the
    ' // variable, in row-major order (last
    ' // dimension varying fastest).
    ' recs         = [record ...]  // The data for all record variables are
    ' // stored interleaved at the end of the
    ' // file.
    ' record       = [varslab ...] // Each record consists of the n-th slab
    ' // from each record variable, for example
    ' // x[n,...], y[n,...], z[n,...] where the
    ' // first index is the record number, which
    ' // is the unlimited dimension index.
    ' varslab      = [values ...]  // One record of data for a variable, a
    ' // block of values all of the same type as
    ' // the variable in row-major order (last
    ' // index varying fastest).
    ' values       = bytes | chars | shorts | ints | floats | doubles
    ' string       = nelems  [chars]
    ' bytes        = [BYTE ...]  padding
    ' chars        = [CHAR ...]  padding
    ' shorts       = [SHORT ...]  padding
    ' ints         = [INT ...]
    ' floats       = [FLOAT ...]
    ' doubles      = [DOUBLE ...]
    ' padding      = <0, 1, 2, or 3 bytes to next 4-byte boundary>
    ' // Header padding uses null (\x00) bytes.  In
    ' // data, padding uses variable's fill value.
    ' // See "Note on padding", below, for a special
    ' // case.
    ' NON_NEG      = <non-negative INT>
    ' STREAMING    = \xFF \xFF \xFF \xFF   // Indicates indeterminate record
    ' // count, allows streaming data
    ' OFFSET       = <non-negative INT> |  // For classic format or
    ' <non-negative INT64>  // for 64-bit offset format
    ' BYTE         = <8-bit byte>          // See "Note on byte data", below.
    ' CHAR         = <8-bit byte>          // See "Note on char data", below.
    ' SHORT        = <16-bit signed integer, Bigendian, two's complement>
    ' INT          = <32-bit signed integer, Bigendian, two's complement>
    ' INT64        = <64-bit signed integer, Bigendian, two's complement>
    ' FLOAT        = <32-bit IEEE single-precision float, Bigendian>
    ' DOUBLE       = <64-bit IEEE double-precision float, Bigendian>
    '                                          // following type tags are 32-bit integers
    ' NC_BYTE      = \x00 \x00 \x00 \x01       // 8-bit signed integers
    ' NC_CHAR      = \x00 \x00 \x00 \x02       // text characters
    ' NC_SHORT     = \x00 \x00 \x00 \x03       // 16-bit signed integers
    ' NC_INT       = \x00 \x00 \x00 \x04       // 32-bit signed integers
    ' NC_FLOAT     = \x00 \x00 \x00 \x05       // IEEE single precision floats
    ' NC_DOUBLE    = \x00 \x00 \x00 \x06       // IEEE double precision floats
    '                                          // Default fill values for each type, may be
    '                                          // overridden by variable attribute named
    '                                          // '_FillValue'. See "Note on fill values",
    '                                          // below.
    ' FILL_CHAR    = \x00                      // null byte
    ' FILL_BYTE    = \x81                      // (signed char) -127
    ' FILL_SHORT   = \x80 \x01                 // (short) -32767
    ' FILL_INT     = \x80 \x00 \x00 \x01       // (int) -2147483647
    ' FILL_FLOAT   = \x7C \xF0 \x00 \x00       // (float) 9.9692099683868690e+36
    ' FILL_DOUBLE  = \x47 \x9E \x00 \x00 \x00 \x00 \x00 \x00 //(double)9.9692099683868690e+36

#End Region

    ReadOnly output As BinaryDataWriter
    ReadOnly globalAttrs As New List(Of attribute)

    Dim variables As New List(Of variable)
    Dim dimensionList As New Dictionary(Of String, SeqValue(Of Dimension))
    Dim recordDimensionLength As UInteger
    Dim init0 As Long

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(path As String, Optional encoding As Encodings = Encodings.UTF8)
        Call Me.New(path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False), encoding)
    End Sub

    Sub New(file As Stream, Optional encoding As Encodings = Encodings.UTF8)
        output = New BinaryDataWriter(file, encoding) With {
            .ByteOrder = ByteOrder.BigEndian,
            .RerouteInt32ToUnsigned = True
        }

        ' magic and version
        Call output.Write(netCDFReader.Magic, BinaryStringFormat.NoPrefixOrTermination)
        ' classic format, version = 1
        Call output.Write(CByte(1))

        init0 = file.Position
    End Sub

    ''' <summary>
    ''' Add a collection of new global attributes to the cdf file
    ''' 
    ''' (在这里向文件中添加一些额外的标记信息, 用来解释数据集)
    ''' </summary>
    ''' <param name="attrs"></param>
    ''' <returns></returns>
    Public Function GlobalAttributes(ParamArray attrs As attribute()) As CDFWriter
        Call globalAttrs.AddRange(attrs)
        Return Me
    End Function

    Public Function GlobalAttributes(name As String, value As String) As CDFWriter
        Return GlobalAttributes(New attribute(name, value))
    End Function

    Public Function GlobalAttributes(name As String, value As Boolean) As CDFWriter
        Return GlobalAttributes(New attribute(name, If(value, 1, 0), CDFDataTypes.NC_INT))
    End Function

    Public Function GlobalAttributes(name As String, value As Integer) As CDFWriter
        Return GlobalAttributes(New attribute(name, value, CDFDataTypes.NC_INT))
    End Function

    ''' <summary>
    ''' 在这里定义在数据集中所使用到的基础数据类型信息
    ''' </summary>
    ''' <param name="[dim]"></param>
    ''' <returns></returns>
    Public Function Dimensions(ParamArray [dim] As Dimension()) As CDFWriter
        dimensionList = [dim] _
            .SeqIterator _
            .ToDictionary(Function(d) d.value.name,
                          Function(d)
                              Return d
                          End Function)
        Return Me
    End Function

    ''' <summary>
    ''' 会需要在这个函数之中进行offset的计算操作
    ''' </summary>
    Public Sub Save()
        Call output.Seek(init0, SeekOrigin.Begin)

        Call output.Write(recordDimensionLength)
        ' -------------------------dimensionsList----------------------------
        ' List of dimensions
        Call output.Write(Header.NC_DIMENSION)
        ' dimensionSize
        Call output.Write(dimensionList.Count)

        For Each [dim] In dimensionList
            Dim dimension As Dimension = [dim].Value

            Call output.writeName([dim].Key)
            Call output.Write(dimension.size)
        Next

        ' ------------------------attributesList-----------------------------
        ' global attributes
        Call writeAttributes(output, globalAttrs)

        ' -----------------------variablesList--------------------------
        ' List of variables
        Call output.Write(CUInt(Header.NC_VARIABLE))
        ' variableSize 
        Call output.Write(CUInt(variables.Count))

        ' 先生成每一个变量的header的buffer
        Dim variableBuffers As New List(Of Byte())

        For Each var As variable In variables
            variableBuffers.Add(getVariableHeaderBuffer(var, output))
        Next

        Dim tmp As String = CalcOffsets(variableBuffers)

        ' 在这个循环仅写入了变量的头部数据
        For i As Integer = 0 To variables.Count - 1
            ' offset已经在计算offset函数的调用过程之中被替换掉了
            ' 在这里直接写入buffer数据
            Call output.Write(variableBuffers(i))
        Next

        Using buffer As Stream = tmp.Open
            ' 接着就是写入数据块了
            Call output.Write(buffer)
        End Using
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Flush()
        Call output.Flush()
    End Sub

    ''' <summary>
    ''' 因为这个步骤是发生在计算offset之前的
    ''' 所以<see cref="variable.offset"/>，全部都是零
    ''' 可以在计算完成之后，将buffer的末尾4个字节替换掉再写入文件
    ''' </summary>
    ''' <param name="var"></param>
    ''' <returns></returns>
    Private Shared Function getVariableHeaderBuffer(var As variable, writerTemplate As BinaryDataWriter) As Byte()
        Dim buffer As New MemoryStream

        Using output = New BinaryDataWriter(buffer, writerTemplate.Encoding) With {
            .ByteOrder = writerTemplate.ByteOrder,
            .RerouteInt32ToUnsigned = writerTemplate.RerouteInt32ToUnsigned
        }
            Call output.writeName(var.name)

            ' dimensionality 
            Call output.Write(CUInt(var.dimensions.Length))
            ' dimensionsIds
            Call output.Write(var.dimensions)
            ' attributes of this variable
            Call writeAttributes(output, var.attributes)
            Call output.Write(var.type)
            ' varSize
            Call output.Write(CUInt(var.size))
            ' version = 1, write 4 bytes
            Call output.Write(var.offset)
            Call output.Flush()
        End Using

        Return buffer.ToArray
    End Function

    ''' <summary>
    ''' 这个函数在完成计算之后会直接修改<see cref="Variable"/> class之中的属性值
    ''' 完成函数调用之后可以直接读取属性值
    ''' </summary>
    ''' <param name="buffers">
    ''' 这个是和<see cref="variables"/>之中的元素一一对应的
    ''' </param>
    ''' <remarks>
    ''' 函数返回数据块的缓存
    ''' </remarks>
    Private Function CalcOffsets(buffers As List(Of Byte())) As String
        ' 这个位置是在所有的变量头部之后的
        ' 因为这个函数是发生在变量写入之前的，所以会需要加上自身的长度
        ' 才会将offset的位置移动到数据区域的起始位置
        Dim current As UInteger = output.Position + buffers.Sum(Function(v) v.Length)
        Dim chunk As Byte()
        Dim handle$ = TempFileSystem.GetAppSysTempFile(".dat", App.PID)

        ' 2019-1-21 当写入一个超大的CDF文件的时候
        ' 字节数量会超过Array的最大元素数量上限
        ' 所以在这里会需要使用Stream对象来避免这个可能的问题
        Using dataBuffer As FileStream = handle.Open
            For i As Integer = 0 To variables.Count - 1
                chunk = BitConverter.GetBytes(current)
                ' 因为CDF文件的byteorder是Big，所以在这里填充的时候会需要翻转一下顺序
                buffers(i).Fill(chunk, -4, reverse:=True)
                chunk = variables(i).value.GetBuffer(output.Encoding)
                current += chunk.Length
                dataBuffer.Write(chunk, Scan0, chunk.Length)
            Next

            Call dataBuffer.Flush()
        End Using

        Return handle
    End Function

    Private Shared Sub writeAttributes(output As BinaryDataWriter, attrs As attribute())
        If attrs Is Nothing Then
            attrs = {}
        End If

        ' List of global attributes
        Call output.Write(Header.NC_ATTRIBUTE)
        ' attributeSize
        Call output.Write(attrs.Length)

        For Each attr In attrs
            Call output.writeName(attr.name)
            ' type
            Call output.Write(attr.type)

            ' 在attributes里面，除了字符串，其他类型的数据都是只有一个元素
            Select Case attr.type
                Case CDFDataTypes.NC_BYTE
                    Call output.Write(1)
                    Call output.Write(Byte.Parse(attr.value))
                Case CDFDataTypes.NC_CHAR
                    Dim stringBuffer = output.Encoding.GetBytes(attr.value)
                    Call output.Write(attr.value.Length)
                    Call output.Write(stringBuffer)
                Case CDFDataTypes.NC_DOUBLE
                    Call output.Write(1)
                    Call output.Write(Double.Parse(attr.value))
                Case CDFDataTypes.NC_FLOAT
                    Call output.Write(1)
                    Call output.Write(Single.Parse(attr.value))
                Case CDFDataTypes.NC_INT
                    Call output.Write(1)
                    Call output.Write(UInteger.Parse(attr.value))
                Case CDFDataTypes.NC_SHORT
                    Call output.Write(1)
                    Call output.Write(Short.Parse(attr.value))
                Case CDFDataTypes.NC_INT64
                    Call output.Write(1)
                    Call output.Write(Long.Parse(attr.value))
                Case CDFDataTypes.BOOLEAN

                    ' 20210212 using byte flag for boolean?
                    Call output.Write(1)
                    Call output.Write(CByte(If(attr.value.ParseBoolean, 1, 0)))

                Case Else
                    Throw New NotImplementedException(attr.type.Description)
            End Select

            ' 都必须要做一次padding
            Call output.writePadding
        Next
    End Sub

    ''' <summary>
    ''' 添加一个变量数据
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="data">
    ''' 可以直接添加变量值，因为<see cref="CDFData"/>对象之中定义有转换操作符，
    ''' 所以可以在这里直接添加变量值对象，但是仅限于<see cref="CDFDataTypes"/>
    ''' 之中所限定的类型元素或者其数组
    ''' </param>
    ''' <param name="dims">
    ''' 这个列表必须要是<see cref="CDFWriter.Dimensions(Dimension())"/>之中的
    ''' </param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub AddVariable(name$, data As ICDFDataVector, dims As [Variant](Of String(), String), Optional attrs As [Variant](Of attribute, attribute()) = Nothing)
        variables += New variable With {
            .name = name,
            .type = data.cdfDataType,
            .size = data.length * sizeof(.type),
            .value = data,
            .attributes = attrs.TryCastArray,
            .dimensions = getDimensionList(dims)
        }
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function getDimension(name As String) As Dimension
        Return dimensionList.TryGetValue(name).value
    End Function

    Private Function getDimensionList(dims As [Variant](Of String(), String)) As Integer()
        If dims Like GetType(String) Then
            dims = {dims.TryCast(Of String)}
        End If

        Return dims _
            .TryCast(Of String()) _
            .Select(Function(d) dimensionList(d).i) _
            .ToArray
    End Function

    ''' <summary>
    ''' Add a numeric vector into target cdf file
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <param name="vec"></param>
    ''' <param name="[dim]"></param>
    ''' <param name="attrs"></param>
    ''' <remarks>
    ''' A wrapper of the <see cref="AddVariable(String, ICDFDataVector, Dimension(), attribute())"/> function
    ''' </remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Sub AddVector(name$, vec As IEnumerable(Of Double), [dim] As Dimension, Optional attrs As attribute() = Nothing)
        Call AddVariable(name, CType(vec.ToArray, doubles), [dim], attrs)
    End Sub

    ''' <summary>
    ''' Add a numeric vector into target cdf file
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <param name="vec"></param>
    ''' <param name="[dim]"></param>
    ''' <param name="attrs"></param>
    ''' <remarks>
    ''' A wrapper of the <see cref="AddVariable(String, ICDFDataVector, Dimension(), attribute())"/> function
    ''' </remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Sub AddVector(name$, vec As IEnumerable(Of Single), [dim] As Dimension, Optional attrs As attribute() = Nothing)
        Call AddVariable(name, CType(vec.ToArray, floats), [dim], attrs)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Sub AddVector(name$, vec As IEnumerable(Of Char), [dim] As Dimension, Optional attrs As attribute() = Nothing)
        Call AddVariable(name, CType(vec.ToArray, chars), [dim], attrs)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Sub AddVector(name$, vec As IEnumerable(Of Integer), [dim] As Dimension, Optional attrs As attribute() = Nothing)
        Call AddVariable(name, CType(vec.ToArray, integers), [dim], attrs)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="vec"></param>
    ''' <param name="[dim]"></param>
    ''' <param name="attrs"></param>
    ''' <remarks>
    ''' due to the reason of string value is in variable length, so we just pass the dimension name at here
    ''' </remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Sub AddVector(name As String, vec As IEnumerable(Of String),
                                   [dim] As String,
                                   Optional attrs As attribute() = Nothing)

        Dim chars As chars = CType(vec.ToArray, chars)
        Dim dimSize As New Dimension([dim], chars.Length)

        Call AddVariable(name, chars, dimSize, attrs)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="data"></param>
    ''' <param name="dim">the data dimension will be added into cdf header automatically if the dimension name is missing</param>
    ''' <param name="attrs"></param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub AddVariable(name$, data As ICDFDataVector, [dim] As Dimension, Optional attrs As attribute() = Nothing)
        Call AddVariable(name, data, {[dim]}, attrs)
    End Sub

    ''' <summary>
    ''' 如果<paramref name="dims"/>是不存在的，则会自动添加
    ''' 反之会使用旧的编号
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="data"></param>
    ''' <param name="dims"></param>
    ''' <param name="attrs"></param>
    Public Sub AddVariable(name$, data As ICDFDataVector, dims As Dimension(), Optional attrs As attribute() = Nothing)
        Dim dimNames As New List(Of String)

        For Each d As Dimension In dims
            If Not dimensionList.ContainsKey(d.name) Then
                dimensionList(d.name) = New SeqValue(Of Dimension) With {
                    .i = dimensionList.Count,
                    .value = New Dimension With {
                        .name = d.name,
                        .size = d.size
                    }
                }
            End If

            Call dimNames.Add(d.name)
        Next

        Call AddVariable(name, data, dimNames.ToArray, attrs)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call Save()
                Call output.Flush()
                Call output.Close()
                Call output.Dispose()
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

    ''' <summary>
    ''' Save and close the underlying file
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class

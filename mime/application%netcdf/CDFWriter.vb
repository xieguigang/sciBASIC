Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.MIME.application.netCDF.Components

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

    Dim output As BinaryDataWriter
    Dim globalAttrs As attribute()
    Dim dimensionList As Dimension()
    Dim variables As List(Of variable)

    Sub New(path As String)
        output = New BinaryDataWriter(path.Open) With {
            .ByteOrder = ByteOrder.BigEndian,
            .RerouteInt32ToUnsigned = True
        }

        ' magic and version
        Call output.Write(netCDFReader.Magic, BinaryStringFormat.NoPrefixOrTermination)
        ' classic format
        Call output.Write(CByte(1))
    End Sub

    Public Function GlobalAttributes(attrs As attribute()) As CDFWriter
        globalAttrs = attrs
        Return Me
    End Function

    Public Function Dimensions([dim] As Dimension()) As CDFWriter
        dimensionList = [dim]
        Return Me
    End Function

    Private Function FileWriter(recordDimensionLength As UInteger)
        ' >>>>>>> header
        Call output.Write(recordDimensionLength)
        ' -------------------------dimensionsList----------------------------
        ' List of dimensions
        Call output.Write(CUInt(Header.NC_DIMENSION))
        ' dimensionSize
        Call output.Write(CUInt(dimensionList.Length))

        For Each dimension In dimensionList
            Call output.Write(dimension.name, BinaryStringFormat.UInt32LengthPrefix)
            Call output.writePadding
            Call output.Write(CUInt(dimension.size))
        Next

        ' ------------------------attributesList-----------------------------
        ' global attributes
        Call writeAttributes(output, globalAttrs)

        ' -----------------------variablesList--------------------------
        ' List of variables
        Call output.Write(CUInt(Header.NC_VARIABLE))
        ' variableSize 
        Call output.Write(CUInt(variables.Count))

        For Each var As variable In variables
            Call output.Write(var.name, BinaryStringFormat.UInt32LengthPrefix)
            Call output.writePadding
            ' dimensionality 
            Call output.Write(CUInt(var.dimensions.Length))
            ' dimensionsIds
            Call output.Write(var.dimensions)
            ' attributes of this variable
            ' Call output.writeAttributes(var.attributes)
            Call output.Write(CUInt(str2num(var.type)))
            ' varSize
            Call output.Write(CUInt(var.size))
            ' version = 2, write 8 bytes
            Call output.Write(CUInt(var.offset))
        Next

        ' <<<<<<<< header
    End Function

    Private Sub writeAttributes(output As BinaryDataWriter, attrs As attribute())
        ' List of global attributes
        Call output.Write(CUInt(Header.NC_ATTRIBUTE))
        ' attributeSize
        Call output.Write(CUInt(attrs.Length))

        For Each attr In attrs
            Call output.Write(attr.name, BinaryStringFormat.UInt32LengthPrefix)
            Call output.writePadding
            Call output.Write(CUInt(str2num(attr.type)))
            ' one string, size = 1
            Call output.Write(CUInt(1))
            Call output.Write(attr.value, BinaryStringFormat.NoPrefixOrTermination)
            Call output.writePadding
        Next
    End Sub

    Public Sub AddVariable(name$, data As CDFData)

    End Sub

    Public Sub Save()

    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
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

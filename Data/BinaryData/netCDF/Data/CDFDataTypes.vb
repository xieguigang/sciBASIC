#Region "Microsoft.VisualBasic::d74e4c5752e5ab436aef7eaec4dfd724, Data\BinaryData\netCDF\Data\CDFDataTypes.vb"

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

    '   Total Lines: 104
    '    Code Lines: 23 (22.12%)
    ' Comment Lines: 69 (66.35%)
    '    - Xml Docs: 79.71%
    ' 
    '   Blank Lines: 12 (11.54%)
    '     File Size: 3.38 KB


    '     Enum CDFDataTypes
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace Data

    ''' <summary>
    ''' The enum values of the CDF data types.
    ''' 
    ''' nc_type
    ''' The netcdf external data types
    ''' 
    ''' > https://github.com/Unidata/netcdf-java/blob/a2bfa6b5e817d06ee317d223f9834ca9362b5da0/netcdf4/src/main/java/ucar/nc2/jni/netcdf/Nc4prototypes.java#L67
    ''' </summary>
    ''' <remarks>
    ''' 这个枚举值是直接可以和CDF文件之中读取出来的值之间相互转换的
    ''' </remarks>
    Public Enum CDFDataTypes As Integer

        ''' <summary>
        ''' int NC_NAT = 0; /* Not-A-Type */
        ''' </summary>
        <Description("undefined")> undefined = 0

        ' NC_BYTE      = \x00 \x00 \x00 \x01  // 8-bit signed integers
        ' NC_CHAR      = \x00 \x00 \x00 \x02  // text characters
        ' NC_SHORT     = \x00 \x00 \x00 \x03  // 16-bit signed integers
        ' NC_INT       = \x00 \x00 \x00 \x04  // 32-bit signed integers
        ' NC_FLOAT     = \x00 \x00 \x00 \x05  // IEEE single precision floats
        ' NC_DOUBLE    = \x00 \x00 \x00 \x06  // IEEE double precision floats

        ''' <summary>
        ''' 8-bit signed integers, signed 1 byte integer 
        ''' </summary>
        <Description("byte")> NC_BYTE = 1
        ''' <summary>
        ''' text characters, ISO/ASCII character
        ''' </summary>
        <Description("char")> NC_CHAR = 2
        ''' <summary>
        ''' 16-bit signed integers
        ''' </summary>
        <Description("short")> NC_SHORT = 3
        ''' <summary>
        ''' 32-bit signed integers
        ''' </summary>
        <Description("int")> NC_INT = 4
        ''' <summary>
        ''' IEEE single precision floats
        ''' </summary>
        <Description("float")> NC_FLOAT = 5
        ''' <summary>
        ''' IEEE double precision floats
        ''' </summary>
        <Description("double")> NC_DOUBLE = 6

        NC_UBYTE = 7
        NC_USHORT = 8
        NC_UINT = 9

        ' 下面是拓展类型

        ''' <summary>
        ''' #define NC_INT64   10
        ''' 
        ''' probably not supports by the standard netCDF4 library on linux and NASA Panoply software...
        ''' </summary>
        <Description("long")> NC_INT64 = 10
        ''' <summary>
        ''' unsigned 8-byte int 
        ''' </summary>
        NC_UINT64 = 11

        ''' <summary>
        ''' string
        ''' </summary>
        NC_STRING = 12

        ' /*
        '  * The following are use internally in support of user-defines
        '  * types. They are also the class returned by nc_inq_user_type.
        '  */

        ''' <summary>
        ''' used internally for vlen types
        ''' </summary>
        NC_VLEN = 13
        ''' <summary>
        ''' used internally for opaque types
        ''' </summary>
        NC_OPAQUE = 14
        ''' <summary>
        ''' used internally for enum types
        ''' </summary>
        NC_ENUM = 15
        ''' <summary>
        ''' used internally for compound types
        ''' </summary>
        NC_COMPOUND = 16

        ''' <summary>
        ''' probably not supports by the standard netCDF4 library on linux and NASA Panoply software...
        ''' </summary>
        <Description("boolean")> [BOOLEAN] = 25
    End Enum
End Namespace

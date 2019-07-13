#Region "Microsoft.VisualBasic::e99e80578576883b7c90fca68402a788, Data\BinaryData\DataStorage\netCDF\Data\CDFDataTypes.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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

Namespace netCDF

    ''' <summary>
    ''' The enum values of the CDF data types.
    ''' </summary>
    ''' <remarks>
    ''' 这个枚举值是直接可以和CDF文件之中读取出来的值之间相互转换的
    ''' </remarks>
    Public Enum CDFDataTypes As Integer
        <Description("undefined")> undefined = -1

        ' NC_BYTE      = \x00 \x00 \x00 \x01  // 8-bit signed integers
        ' NC_CHAR      = \x00 \x00 \x00 \x02  // text characters
        ' NC_SHORT     = \x00 \x00 \x00 \x03  // 16-bit signed integers
        ' NC_INT       = \x00 \x00 \x00 \x04  // 32-bit signed integers
        ' NC_FLOAT     = \x00 \x00 \x00 \x05  // IEEE single precision floats
        ' NC_DOUBLE    = \x00 \x00 \x00 \x06  // IEEE double precision floats

        ''' <summary>
        ''' 8-bit signed integers
        ''' </summary>
        <Description("byte")> [BYTE] = 1
        ''' <summary>
        ''' text characters
        ''' </summary>
        <Description("char")> [CHAR] = 2
        ''' <summary>
        ''' 16-bit signed integers
        ''' </summary>
        <Description("short")> [SHORT] = 3
        ''' <summary>
        ''' 32-bit signed integers
        ''' </summary>
        <Description("int")> [INT] = 4
        ''' <summary>
        ''' IEEE single precision floats
        ''' </summary>
        <Description("float")> [FLOAT] = 5
        ''' <summary>
        ''' IEEE double precision floats
        ''' </summary>
        <Description("double")> [DOUBLE] = 6
    End Enum
End Namespace

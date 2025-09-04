#Region "Microsoft.VisualBasic::1bde59906419f755d13d04629eab9ab4, Data\BinaryData\DataStorage\XPT\XPTTypes.vb"

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

    '   Total Lines: 207
    '    Code Lines: 146 (70.53%)
    ' Comment Lines: 15 (7.25%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 46 (22.22%)
    '     File Size: 5.57 KB


    '     Module XPTTypes
    ' 
    ' 
    ' 
    '     Class XPTHeader
    ' 
    '         Function: ToString
    ' 
    '     Class XPTNameString
    ' 
    '         Function: ToString
    ' 
    '     Class XPTContext
    ' 
    '         Function: ToString
    ' 
    '     Class TimeStamp
    ' 
    ' 
    ' 
    '     Class ReadStatVariable
    ' 
    '         Function: ToString
    ' 
    '     Class ReadstatMissingness
    ' 
    ' 
    ' 
    '     Class ReadstatValue
    ' 
    '         Function: ToString
    ' 
    '     Class ReadstatLabelSet
    ' 
    ' 
    ' 
    '     Class ReadstatValueLabel
    ' 
    ' 
    ' 
    '     Enum ReadstatType
    ' 
    '         READSTAT_TYPE_DOUBLE, READSTAT_TYPE_FLOAT, READSTAT_TYPE_INT16, READSTAT_TYPE_INT32, READSTAT_TYPE_INT8
    '         READSTAT_TYPE_STRING, READSTAT_TYPE_STRING_REF
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum ReadstatAlignment
    ' 
    '         READSTAT_ALIGNMENT_CENTER, READSTAT_ALIGNMENT_LEFT, READSTAT_ALIGNMENT_RIGHT, READSTAT_ALIGNMENT_UNKNOWN
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum ReadstatMeasure
    ' 
    '         READSTAT_MEASURE_NOMINAL, READSTAT_MEASURE_ORDINAL, READSTAT_MEASURE_SCALE, READSTAT_MEASURE_UNKNOWN
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Xpt.Types

    Public Module XPTTypes

        Public Const XPORT_MIN_DOUBLE_SIZE As Integer = 3
        Public Const XPORT_MAX_DOUBLE_SIZE As Integer = 8

        Public Const CN_TYPE_NATIVE As Integer = 0
        Public Const CN_TYPE_XPORT As Integer = 1
        Public Const CN_TYPE_IEEEB As Integer = 2
        Public Const CN_TYPE_IEEEL As Integer = 3

        Public ReadOnly XPORT_MONTHS As String() = New String() {"JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"}

    End Module

    Public Class XPTHeader
        ' size 9 bytes
        Public name As String

        Public num1 As Integer
        Public num2 As Integer
        Public num3 As Integer
        Public num4 As Integer
        Public num5 As Integer
        Public num6 As Integer

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class

    Public Class XPTNameString
        Public ntype As Short
        Public nhfun As Short
        Public nlng As Short
        Public nvar0 As Short

        ' size 8 bytes
        Public nname As String
        ' size 40 bytes
        Public nlabel As String
        ' size 8 bytes
        Public nform As String

        Public nfl As Short
        Public nfd As Short
        Public nfj As Short

        ' size 2 bytes
        Public nfill As String
        ' size 8 bytes
        Public niform As String

        Public nifl As Short
        Public nifd As Short
        Public npos As Integer

        ' size 32 bytes
        Public longname As String

        Public labeln As Short

        ' size 18 bytes
        Public rest As String

        Public Overrides Function ToString() As String
            Return $"{nname} ({nlabel})"
        End Function
    End Class

    Public Class XPTContext

        Public file_size As Long
        Public timestamp As Long

        Public obs_count As Integer
        Public var_count As Integer
        Public row_limit As Integer
        Public row_length As Integer
        Public parsed_row_count As Integer

        ' size 40*4 +1
        Public file_label As String
        ' size 32*4 +1
        Public table_name As String

        Public version As Integer

        Public variables As ReadStatVariable()

        Public Overrides Function ToString() As String
            Return $"version {version}"
        End Function
    End Class

    Public Class TimeStamp

        Public tm_isdst As Integer = -1
        Public tm_mday As Short
        Public tm_mon As Short
        Public tm_year As Short
        Public tm_hour As Short
        Public tm_min As Short
        Public tm_sec As Short

    End Class

    Public Class ReadStatVariable
        Public type As ReadstatType
        Public index As Integer

        ' size 300 bytes
        Public name As String
        ' size 256 bytes
        Public format As String
        ' size 1024 bytes
        Public label As String

        Public offset As Integer
        Public storage_width As Integer
        Public user_width As Integer

        Public label_set As ReadstatLabelSet

        Public missingness As ReadstatMissingness
        Public measure As ReadstatMeasure
        Public alignment As ReadstatAlignment

        Public display_width As Integer
        Public decimals As Integer
        Public skip As Integer
        Public index_after_skipping As Integer

        Public Overrides Function ToString() As String
            Return $"{name}({label}, {type.ToString})"
        End Function
    End Class

    Public Class ReadstatMissingness
        ' size 32s
        Friend missing_ranges As ReadstatValue()
        Friend missing_ranges_count As Long
    End Class

    Public Class ReadstatValue
        Friend tvalue As String
        Friend value As Double
        Public type As ReadstatType
        Friend tag As Byte
        Friend is_system_missing As Integer
        Friend is_tagged_missing As Integer

        Public Overrides Function ToString() As String
            Return type.ToString
        End Function
    End Class

    Public Class ReadstatLabelSet
        Public type As ReadstatType
        ' size 256 bytes
        Friend name As String

        Friend value_labels As ReadstatValueLabel
        Friend value_labels_count As Long
        Friend value_labels_capacity As Long

        Friend variables_count As Long
        Friend variables_capacity As Long
    End Class

    Public Class ReadstatValueLabel
        Friend double_key As Double
        Friend int32_key As Integer
        Friend tag As Char

        Friend string_key As String
        Friend string_key_len As Integer

        Friend label As String
        Friend label_len As Integer
    End Class

    Public Enum ReadstatType
        READSTAT_TYPE_STRING
        READSTAT_TYPE_INT8
        READSTAT_TYPE_INT16
        READSTAT_TYPE_INT32
        READSTAT_TYPE_FLOAT
        READSTAT_TYPE_DOUBLE
        READSTAT_TYPE_STRING_REF
    End Enum

    Public Enum ReadstatAlignment
        READSTAT_ALIGNMENT_UNKNOWN
        READSTAT_ALIGNMENT_LEFT
        READSTAT_ALIGNMENT_CENTER
        READSTAT_ALIGNMENT_RIGHT
    End Enum

    Public Enum ReadstatMeasure
        READSTAT_MEASURE_UNKNOWN
        READSTAT_MEASURE_NOMINAL
        READSTAT_MEASURE_ORDINAL
        READSTAT_MEASURE_SCALE
    End Enum
End Namespace

#Region "Microsoft.VisualBasic::ae9880e075b21179503f2e3311df10dd, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\FormatNumber.vb"

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

    '   Total Lines: 78
    '    Code Lines: 37 (47.44%)
    ' Comment Lines: 37 (47.44%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (5.13%)
    '     File Size: 2.94 KB


    '     Enum FormatNumber
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace XLSX.Writer.Styling


    ''' <summary>
    ''' Enum for predefined number formats
    ''' </summary>
    ''' <remarks>There are other predefined formats (e.g. 43 and 44) that are not listed. The declaration of such formats is done in the number formats section of the style document, whereas the officially listed ones are implicitly used and not declared in the style document</remarks>
    Public Enum FormatNumber
        ''' <summary>No format / Default</summary>
        none = 0
        ''' <summary>Format: 0</summary>
        format_1 = 1
        ''' <summary>Format: 0.00</summary>
        format_2 = 2
        ''' <summary>Format: #,##0</summary>
        format_3 = 3
        ''' <summary>Format: #,##0.00</summary>
        format_4 = 4
        ''' <summary>Format: $#,##0_);($#,##0)</summary>
        format_5 = 5
        ''' <summary>Format: $#,##0_);[Red]($#,##0)</summary>
        format_6 = 6
        ''' <summary>Format: $#,##0.00_);($#,##0.00)</summary>
        format_7 = 7
        ''' <summary>Format: $#,##0.00_);[Red]($#,##0.00)</summary>
        format_8 = 8
        ''' <summary>Format: 0%</summary>
        format_9 = 9
        ''' <summary>Format: 0.00%</summary>
        format_10 = 10
        ''' <summary>Format: 0.00E+00</summary>
        format_11 = 11
        ''' <summary>Format: # ?/?</summary>
        format_12 = 12
        ''' <summary>Format: # ??/??</summary>
        format_13 = 13
        ''' <summary>Format: m/d/yyyy</summary>
        format_14 = 14
        ''' <summary>Format: d-mmm-yy</summary>
        format_15 = 15
        ''' <summary>Format: d-mmm</summary>
        format_16 = 16
        ''' <summary>Format: mmm-yy</summary>
        format_17 = 17
        ''' <summary>Format: mm AM/PM</summary>
        format_18 = 18
        ''' <summary>Format: h:mm:ss AM/PM</summary>
        format_19 = 19
        ''' <summary>Format: h:mm</summary>
        format_20 = 20
        ''' <summary>Format: h:mm:ss</summary>
        format_21 = 21
        ''' <summary>Format: m/d/yyyy h:mm</summary>
        format_22 = 22
        ''' <summary>Format: #,##0_);(#,##0)</summary>
        format_37 = 37
        ''' <summary>Format: #,##0_);[Red](#,##0)</summary>
        format_38 = 38
        ''' <summary>Format: #,##0.00_);(#,##0.00)</summary>
        format_39 = 39
        ''' <summary>Format: #,##0.00_);[Red](#,##0.00)</summary>
        format_40 = 40
        ''' <summary>Format: mm:ss</summary>
        format_45 = 45
        ''' <summary>Format: [h]:mm:ss</summary>
        format_46 = 46
        ''' <summary>Format: mm:ss.0</summary>
        format_47 = 47
        ''' <summary>Format: ##0.0E+0</summary>
        format_48 = 48
        ''' <summary>Format: #</summary>
        format_49 = 49
        ''' <summary>Custom Format (ID 164 and higher)</summary>
        custom = 164
    End Enum


End Namespace

#Region "Microsoft.VisualBasic::a99ea40b320ac408093681642138dede, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLS\BIFF\EnumConstants.vb"

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

    '   Total Lines: 73
    '    Code Lines: 43 (58.90%)
    ' Comment Lines: 21 (28.77%)
    '    - Xml Docs: 61.90%
    ' 
    '   Blank Lines: 9 (12.33%)
    '     File Size: 1.81 KB


    '     Enum ValueTypes
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum CellAlignment
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum CellFont
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum CellHiddenLocked
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum MarginTypes
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum FontFormatting
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace XLS.BIFF

    ''' <summary>
    ''' enum to handle the various types of values that can be written
    ''' to the excel file.
    ''' </summary>
    Public Enum ValueTypes
        xlsinteger = 0
        xlsnumber = 1
        xlsText = 2
    End Enum

    ''' <summary>
    ''' enum to hold cell alignment
    ''' </summary>
    Public Enum CellAlignment
        xlsGeneralAlign = 0
        xlsLeftAlign = 1
        xlsCentreAlign = 2
        xlsrightAlign = 3
        xlsFillCell = 4
        xlsLeftBorder = 8
        xlsRightBorder = 16
        xlsTopBorder = 32
        xlsBottomBorder = 64
        xlsShaded = 128
    End Enum

    ''' <summary>
    ''' enum to handle selecting the font for the cell
    ''' </summary>
    Public Enum CellFont
        'used by rgbAttr2
        'bits 0-5 handle the *picture* formatting, not bold/underline etc...
        'bits 6-7 handle the font number
        xlsFont0 = 0
        xlsFont1 = 64
        xlsFont2 = 128
        xlsFont3 = 192
    End Enum

    Public Enum CellHiddenLocked
        'used by rgbAttr1
        'bits 0-5 must be zero
        'bit 6 locked/unlocked
        'bit 7 hidden/not hidden
        xlsNormal = 0
        xlsLocked = 64
        xlsHidden = 128
    End Enum


    ''' <summary>
    ''' set up variables to hold the spreadsheet's layout
    ''' </summary>
    Public Enum MarginTypes
        xlsLeftMargin = 38
        xlsRightMargin = 39
        xlsTopMargin = 40
        xlsBottomMargin = 41
    End Enum


    Public Enum FontFormatting
        'add these enums together. For example: xlsBold + xlsUnderline
        xlsNoFormat = 0
        xlsBold = 1
        xlsItalic = 2
        xlsUnderline = 4
        xlsStrikeout = 8
    End Enum

End Namespace

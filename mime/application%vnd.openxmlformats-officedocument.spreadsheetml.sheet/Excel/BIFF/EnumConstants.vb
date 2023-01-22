Namespace BIFF
    'enum to handle the various types of values that can be written
    'to the excel file.
    Public Enum ValueTypes
        xlsinteger = 0
        xlsnumber = 1
        xlsText = 2
    End Enum

    'enum to hold cell alignment
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

    'enum to handle selecting the font for the cell
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


    'set up variables to hold the spreadsheet's layout
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
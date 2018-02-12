Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl
Imports xlsxFont = Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.font

Public Module Styling

    Public Function StyleFont(fontface$, size!, Optional fillColor$ = "black", Optional bold As Boolean = False, Optional italic As Boolean = False, Optional underline As Boolean = False) As xlsxFont
        Dim style As New xlsxFont With {
            .charset = "134",
            .scheme = "minor",
            .color = fillColor.TranslateColor,
            .family = 3,
            .name = fontface,
            .sz = size,
            .b = If(bold, New Flag, Nothing),
            .i = If(italic, New Flag, Nothing),
            .u = If(underline, New Flag, Nothing)
        }

        Return style
    End Function
End Module

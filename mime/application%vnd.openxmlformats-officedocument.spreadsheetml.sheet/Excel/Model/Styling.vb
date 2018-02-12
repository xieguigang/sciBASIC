Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.worksheets
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

    <Extension>
    Public Sub SetColorScaleStyles(xlsx As File, sheetName$, range$, style As colorScale)
        Dim worksheet As worksheet = xlsx.GetWorksheet(sheetName)
        Dim format As New conditionalFormatting With {
            .sqref = range,
            .cfRule = New cfRule With {
                .colorScale = style,
                .type = "colorScale",
                .priority = 1
            }
        }

        worksheet.conditionalFormattings.Add(format)

        With "worksheet.update"
            If xlsx.modify(.ByRef) = -1 Then
                xlsx.modify.Add(.ByRef)
            End If
        End With
    End Sub

    Public Function ColorScale(min$, p50$, max$) As colorScale
        Dim minType As New StringValue With {.type = "min"}
        Dim maxType As New StringValue With {.type = "max"}
        Dim p50Type As New StringValue With {.type = "percentile", .val = 50}

        Return New colorScale With {
            .cfvo = {minType, p50Type, maxType},
            .colors = {min, p50, max}
        }
    End Function

    Public Function ColorScale(min$, p25$, p50$, p75$, max$) As colorScale
        Dim minType As New StringValue With {.type = "min"}
        Dim maxType As New StringValue With {.type = "max"}
        Dim p25Type As New StringValue With {.type = "percentile", .val = 25}
        Dim p50Type As New StringValue With {.type = "percentile", .val = 50}
        Dim p75Type As New StringValue With {.type = "percentile", .val = 75}

        Return New colorScale With {
            .cfvo = {minType, p25Type, p50Type, p75Type, maxType},
            .colors = {min, p25, p50, p75, max}
        }
    End Function
End Module

#Region "Microsoft.VisualBasic::e9d7fcd90449b46e87569ba2e553b8fb, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Model\Styling.vb"

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
    '    Code Lines: 66
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 3.11 KB


    '     Module Styling
    ' 
    '         Function: (+2 Overloads) ColorScale, StyleFont
    ' 
    '         Sub: SetColorScaleStyles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.XML.xl
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.XML.xl.worksheets
Imports xlsxFont = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.XML.xl.font

Namespace XLSX.Model

    Public Module Styling

        Public Function StyleFont(fontface$, size!,
                                  Optional fillColor$ = "black",
                                  Optional bold As Boolean = False,
                                  Optional italic As Boolean = False,
                                  Optional underline As Boolean = False) As xlsxFont

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
                    .ColorScale = style,
                    .Type = "colorScale",
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
End Namespace

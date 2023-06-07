Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer

Module Module1

    Sub testWriter()
        Dim workbook As New Workbook("basic.xlsx", "SXheet124234") ' Create New workbook
        workbook.CurrentWorksheet.AddNextCell("Test") ' Add cell A1
        workbook.CurrentWorksheet.AddNextCell(55.2) ' Add cell B1
        workbook.CurrentWorksheet.AddNextCell(DateTime.Now) ' Add cell C1

        workbook.AddWorksheet("page_nooote")
        workbook.CurrentWorksheet.AddNextCell("Test22222") ' Add cell A1
        workbook.CurrentWorksheet.AddNextCell(4323355.2) ' Add cell B1
        workbook.CurrentWorksheet.AddNextCell(DateTime.Now) ' Add cell C1

        workbook.Save()

        Pause()
    End Sub
End Module

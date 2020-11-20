Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.DATA
Imports Table = Microsoft.VisualBasic.Data.csv.IO.File

Public Module MsHtml

    Public Function ToExcel(Of T As Class)(seq As IEnumerable(Of T), Optional sheetName$ = "Sheet1") As String
        Return seq.ToCsvDoc.ToExcel(sheetName)
    End Function

    <Extension>
    Public Function ToExcel(file As Table, sheetName As String) As String
        Dim html As New StringBuilder()

        html.AppendLine("<html xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:x=""urn:schemas-microsoft-com:office:excel"" xmlns=""http://www.w3.org/TR/REC-html40"">")
        html.AppendLine("<head>")
        html.AppendLine("<meta name=""ProgId"" content=""Excel.Sheet"">")
        html.AppendLine("<meta name=""Generator"" content=""Microsoft Excel 11"">")
        html.AppendLine("<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">")

        html.AppendLine($"    
<!--[if gte mso 9]>
	<xml>
		<x:ExcelWorkbook>
            <x:ExcelWorksheets>
                <x:ExcelWorksheet>
                    <x:Name>{sheetName}</x:Name>
                    <x:WorksheetOptions>
                        <x:DisplayGridlines/>
                    </x:WorksheetOptions>
                </x:ExcelWorksheet>
            </x:ExcelWorksheets>
		</x:ExcelWorkbook>
    </xml>
<![endif]-->")

        html.AppendLine("</head>")
        html.AppendLine("<body>")
        html.AppendLine(file.html)
        html.AppendLine("</body>")
        html.AppendLine("</html>")

        Return html.ToString()
    End Function
End Module

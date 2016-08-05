Imports System.Runtime.CompilerServices

Namespace Excel

    Public Module Extensions

        <Extension>
        Public Function ReadXlsx(file As String, Optional sheetName As String = "Sheet1") As DocumentStream.DataFrame
            Dim reader As New ExcelReader(file.GetFullPath, True, True)
            Dim data As DataTable = reader.GetWorksheet(sheetName)
            Dim df As DocumentStream.DataFrame = data.CreateDataReader.DataFrame
            Return df
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="file"></param>
        ''' <param name="sheetName"></param>
        ''' <param name="maps">Field(Csv) -> Class.Property Name</param>
        ''' <returns></returns>
        <Extension>
        Public Function ReadXlsx(Of T As Class)(file As String,
                                                Optional sheetName As String = "Sheet1",
                                                Optional maps As Dictionary(Of String, String) = Nothing) As T()
            Dim df As DocumentStream.DataFrame = file.ReadXlsx(sheetName)
            Return df.AsDataSource(Of T)(False, maps)
        End Function
    End Module
End Namespace
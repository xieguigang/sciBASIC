Imports System.IO
Imports System.Text

Namespace DecisionTree

    Public Module CsvFileHandler

        Public Function ImportFromCsvFile(filePath As String) As DataTable
            Dim rows = 0
            Dim data = New DataTable()

            Using reader = New StreamReader(File.OpenRead(filePath))
                While Not reader.EndOfStream
                    Dim line = reader.ReadLine()
                    Dim values = line.Substring(0, line.Length - 1).Split(";"c)

                    For Each item As String In values
                        If String.IsNullOrEmpty(item) OrElse String.IsNullOrWhiteSpace(item) Then
                            Throw New Exception("Value can't be empty")
                        End If

                        If rows = 0 Then
                            data.Columns.Add(item)
                        End If
                    Next

                    If rows > 0 Then
                        data.Rows.Add(values)
                    End If

                    rows += 1

                    If values.Length <> data.Columns.Count Then
                        Throw New Exception("Row is shorter or longer than title row")
                    End If
                End While
            End Using

            Dim differentValuesOfLastColumn = MyAttribute.GetDifferentAttributeNamesOfColumn(data, data.Columns.Count - 1)

            If differentValuesOfLastColumn.Count > 2 Then
                Throw New Exception("The last column is the result column and can contain only 2 different values")
            End If

            ' if no rows are entered or data == null, return null
            Return If(data.Rows.Count > 0, data, Nothing)
        End Function

        Public Sub ExportToCsvFile(data As DataTable, filePath As String)
            If data.Columns.Count = 0 Then
                Throw New Exception("Nothing to export")
            End If

            Dim sb = New StringBuilder()

            ' add titles to the string builder
            For Each item In data.Columns
                ' seperate values with a ;
                sb.AppendFormat($"{item};")
            Next

            sb.AppendLine()

            ' add every row to the string builder
            For i As Integer = 0 To data.Rows.Count - 1
                For j As Integer = 0 To data.Columns.Count - 1
                    ' seperate values with a ;
                    sb.AppendFormat($"{data.Rows(i)(j)};")
                Next

                sb.AppendLine()
            Next

            File.WriteAllText(filePath, sb.ToString())

            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine("Data sucessfully exported")
            Console.ResetColor()
        End Sub
    End Module
End Namespace
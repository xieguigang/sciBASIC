#Region "Microsoft.VisualBasic::744c8a4af52e4c31fc7aa8eb01956e12, G:/GCModeller/src/runtime/sciBASIC#/Data_science/DataMining/DataMining//test/DecisionTreeProgram.vb"

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

'   Total Lines: 474
'    Code Lines: 57
' Comment Lines: 322
'   Blank Lines: 95
'     File Size: 22.07 KB


'     Module DecisionTreeProgram
' 
'         Sub: Main, treeTest2
' 
'     Class CsvFileHandler
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: ImportFromCsvFile
' 
'         Sub: DisplayErrorMessage
' 
' 
' /********************************************************************************/

#End Region

'Imports System.Collections.Generic
'Imports System.Data
'Imports System.IO
Imports Microsoft.VisualBasic.DataMining.DecisionTree
Imports Microsoft.VisualBasic.DataMining.DecisionTree.Data
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace DecisionTree

    Module DecisionTreeProgram

        Sub Main()

            '  Call treeTest2()

            Dim data As DataTable = CsvFileHandler.ImportFromCsvFile("E:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\DecisionTree\trainingdata.csv")
            Dim decisionTree As New Tree(data)
            Dim valueForQuery As New Dictionary(Of String, String) From {{"Outlook", "Sunny"}, {"Temperatur", "Hot"}, {"Humidity", "High"}, {"Wind", "Weak"}}
            ' OUTLOOK -- sunny --> HUMIDITY -- high --> YES
            Dim result = decisionTree.CalculateResult(valueForQuery)

            valueForQuery = New Dictionary(Of String, String) From {{"Outlook", "Overcast"}, {"Temperatur", "Hot"}, {"Humidity", "High"}, {"Wind", "Weak"}}

            Dim query3 As New Dictionary(Of String, String)(valueForQuery)

            ' OUTLOOK -- overcast --> NO
            Dim result2 = decisionTree.CalculateResult(valueForQuery)

            Call decisionTree.root.GetJson.SaveTo("./trainingdata.json")

            Dim tree2 As New Tree("./trainingdata.json".LoadJsonFile(Of TreeNode))

            Dim result3 = decisionTree.CalculateResult(query3)

            Pause()
        End Sub


        'Sub treeTest2()
        '    Dim data As DataTable = csv.Load("E:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\DecisionTree\RestaurantTrainData.csv").AsMatrix.Imports
        '    Dim tree As New Tree(data)

        '    '   Call tree.root.GetJson.SaveTo("E:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\DecisionTree\RestaurantTrainData.json")

        '    Dim validations = csv.Load("E:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\DecisionTree\RestaurantTestData.csv").AsMatrix.Imports.AsValidateSet.ToArray
        '    Dim runTest = Function(test As NamedValue(Of Dictionary(Of String, String)))
        '                      Dim result = tree.CalculateResult(test.Value)
        '                      Dim validates = result.result.ToLower = test.Name.ToLower

        '                      Return New NamedValue(Of ClassifyResult) With {.Name = test.Name, .Value = result, .Description = validates}
        '                  End Function

        '    Dim validaTest = validations.Select(Function(test) test.DoCall(runTest)).GroupBy(Function(result) result.Description).Select(Function(g)
        '                                                                                                                                     Return New NamedCollection(Of ClassifyResult)(g.Key, g.Select(Function(d) d.Value)) With {.Description = .Length}
        '                                                                                                                                 End Function).ToArray


        '    Pause()
        'End Sub
    End Module

    Public NotInheritable Class CsvFileHandler
        Private Sub New()
        End Sub
        Public Shared Function ImportFromCsvFile(filePath As String) As DataTable
            Dim lines = filePath.ReadAllLines
            Dim headers = lines(Scan0).Trim(";"c).Split(";"c)
            Dim rows = lines.Skip(1).Select(Function(line)
                                                Dim t = line.Trim(";"c).Split(";"c)
                                                Dim obj As New Entity With {.entityVector = t}

                                                Return obj
                                            End Function).ToArray

            Return New DataTable With {.headers = headers, .rows = rows}

            'Dim rows = 0
            'Dim data = New DataTable()

            'Try
            '    Using reader = New StreamReader(File.OpenRead(filePath))
            '        While Not reader.EndOfStream
            '            Dim line = reader.ReadLine()
            '            Dim values = line.Substring(0, line.Length - 1).Split(";"c)

            '            For Each item As String In values
            '                If String.IsNullOrEmpty(item) OrElse String.IsNullOrWhiteSpace(item) Then
            '                    Throw New Exception("Value can't be empty")
            '                End If

            '                If rows = 0 Then
            '                    data.Columns.Add(item)
            '                End If
            '            Next

            '            If rows > 0 Then
            '                data.Rows.Add(values)
            '            End If

            '            rows += 1

            '            If values.Length <> data.Columns.Count Then
            '                Throw New Exception("Row is shorter or longer than title row")
            '            End If
            '        End While
            '    End Using

            '    Dim differentValuesOfLastColumn = Attributes.GetDifferentAttributeNamesOfColumn(data, data.Columns.Count - 1)

            '    If differentValuesOfLastColumn.Count > 2 Then
            '        Throw New Exception("The last column is the result column and can contain only 2 different values")
            '    End If
            'Catch ex As Exception
            '    DisplayErrorMessage(ex.Message)
            '    data = Nothing
            'End Try

            '' if no rows are entered or data == null, return null
            'Return If(data.Rows.Count > 0, data, Nothing)
        End Function

        'Public Shared Sub ExportToCsvFile(data As DataTable, filePath As String)
        '    If data.Columns.Count = 0 Then
        '        Throw New Exception("Nothing to export")
        '    End If

        '    Dim sb = New StringBuilder()

        '    ' add titles to the string builder
        '    For Each item In data.Columns
        '        ' seperate values with a ;
        '        sb.AppendFormat("{item};")
        '    Next

        '    sb.AppendLine()

        '    ' add every row to the string builder
        '    For i As Integer = 0 To data.Rows.Count - 1
        '        For j As Integer = 0 To data.Columns.Count - 1
        '            ' seperate values with a ;
        '            sb.AppendFormat("{data.Rows[i][j]};")
        '        Next

        '        sb.AppendLine()
        '    Next

        '    File.WriteAllText(filePath, sb.ToString())

        '    Console.ForegroundColor = ConsoleColor.Green
        '    Console.WriteLine("Data sucessfully exported")
        '    Console.ResetColor()
        'End Sub

        Private Shared Sub DisplayErrorMessage(errorMessage As String)
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine(vbLf & "{errorMessage}" & vbLf)
            Console.ResetColor()
        End Sub
    End Class

End Namespace

'    Public NotInheritable Class DecisionTreeProgram
'        Private Sub New()
'        End Sub
'        Friend Shared Sub Main(args As String())
'            Console.WindowWidth = Console.LargestWindowWidth - 10
'            Console.ForegroundColor = ConsoleColor.Yellow
'            Console.WriteLine("Welcome to the decison tree calculator")
'            Console.WriteLine("---------------------------------------")
'            Console.ResetColor()

'            Do
'                Dim data = New DataTable()

'                Console.ForegroundColor = ConsoleColor.Cyan
'                Console.WriteLine("1 - Import data from csv file")
'                Console.ForegroundColor = ConsoleColor.Yellow
'                Console.WriteLine("2 - Enter data manually")
'                Console.ForegroundColor = ConsoleColor.Cyan
'                Console.WriteLine("3 - End program")
'                Console.ResetColor()
'                Dim input = ReadLineTrimmed()

'                Select Case input
'                ' data will be imported from csv file
'                    Case "1"
'                        Console.ForegroundColor = ConsoleColor.Cyan
'                        Console.WriteLine(vbLf & "Enter the path to the .csv file which you want to import")
'                        Console.ResetColor()
'                        input = ReadLineTrimmed()

'                        data = CsvFileHandler.ImportFromCsvFile(input)

'                        If data Is Nothing Then
'                            DisplayErrorMessage("An error occured while importing the data from the .csv file. Press any key to close the program.")
'                            Console.ReadKey()
'                            EndProgram()
'                        Else
'                            CreateTreeAndHandleUserOperation(data)
'                        End If

'                        Exit Select

'                ' user enters data by hand
'                    Case "2"
'                        Do
'                            Console.ForegroundColor = ConsoleColor.Yellow
'                            Console.WriteLine(vbLf & "1 - Enter data")
'                            Console.ForegroundColor = ConsoleColor.Cyan
'                            Console.WriteLine("2 - Create decision tree")
'                            Console.ForegroundColor = ConsoleColor.Cyan
'                            Console.WriteLine("3 - Export to csv file")
'                            Console.ForegroundColor = ConsoleColor.Yellow
'                            Console.WriteLine("4 - End program")
'                            Console.ResetColor()
'                            input = ReadLineTrimmed()

'                            Select Case input
'                            ' user enters data by hand
'                                Case "1"
'                                    Console.ForegroundColor = ConsoleColor.Cyan
'                                    Console.WriteLine(vbLf & "How many columns do you want to enter?")
'                                    Console.ResetColor()
'                                    input = ReadLineTrimmed()

'                                    Dim amountOfColumns As Integer
'                                    Dim rightInput = Integer.TryParse(input, amountOfColumns)

'                                    If rightInput AndAlso amountOfColumns > 1 Then
'                                        data = EnterColumnTitle(amountOfColumns)
'                                        data = EnterRowValues(data)

'                                        CreateTreeAndHandleUserOperation(data)
'                                    Else
'                                        DisplayErrorMessage("Wrong input. Amount of columns must be an integer greater than 1")
'                                    End If

'                                    Exit Select

'                            ' after data input decision tree can be created
'                                Case "2"
'                                    If data.Rows.Count > 0 Then
'                                        CreateTreeAndHandleUserOperation(data)
'                                    Else
'                                        DisplayErrorMessage("You don't have data entered yet")
'                                    End If
'                                    Exit Select

'                            ' after data input the data can be exported into a csv file
'                                Case "3"
'                                    Console.ForegroundColor = ConsoleColor.Cyan
'                                    Console.WriteLine(vbLf & "Enter path for your csv file")
'                                    Console.ResetColor()
'                                    input = ReadLineTrimmed()

'                                    Dim endOfDirectoryPath = input.LastIndexOf("\")

'                                    If endOfDirectoryPath > 0 AndAlso Directory.Exists(input.Substring(0, endOfDirectoryPath)) Then
'                                        Try
'                                            CsvFileHandler.ExportToCsvFile(data, input)
'                                        Catch ex As Exception
'                                            DisplayErrorMessage("An error during the export occured. Error message: {ex.Message}")
'                                        End Try
'                                    Else
'                                        DisplayErrorMessage("Directory not found")
'                                    End If
'                                    Exit Select

'                                Case "4"
'                                    EndProgram()
'                                    Exit Select
'                                Case Else

'                                    DisplayErrorMessage("Wrong input")
'                                    Exit Select
'                            End Select
'                        Loop While True

'                    Case "3"
'                        EndProgram()
'                        Exit Select
'                    Case Else

'                        DisplayErrorMessage("Wrong input")
'                        Exit Select
'                End Select
'            Loop While True
'        End Sub

'        Private Shared Function ReadLineTrimmed() As String
'            Return Console.ReadLine().TrimStart().TrimEnd()
'        End Function

'        Private Shared Sub CreateTreeAndHandleUserOperation(data As DataTable)
'            Dim decisionTree = New Tree()
'            decisionTree.root = Tree.Learn(data, "")
'            Dim returnToMainMenu = False

'            Console.ForegroundColor = ConsoleColor.Green
'            Console.WriteLine(vbLf & "Decision tree created")
'            Console.ResetColor()

'            Do
'                Dim valuesForQuery = New Dictionary(Of String, String)()

'                ' loop for data input for the query and some special commands
'                For i As Integer = 0 To data.Columns.Count - 2
'                    Console.ForegroundColor = ConsoleColor.Cyan
'                    Console.WriteLine(vbLf & "Enter your value for {data.Columns[i]} or help for a list of the additional instructions")
'                    Console.ResetColor()
'                    Dim input = ReadLineTrimmed()

'                    If input.ToUpper().Equals("ENDPROGRAM") Then
'                        EndProgram()
'                    ElseIf input.ToUpper().Equals("PRINT") Then
'                        Console.WriteLine()
'                        Tree.Print(decisionTree.root, decisionTree.root.name.ToUpper())
'                        Tree.PrintLegend("Due to the limitation of the console the tree is displayed as a list of every possible route. The colors indicate the following values:")

'                        i -= 1
'                    ElseIf input.ToUpper().Equals("MAINMENU") Then
'                        returnToMainMenu = True
'                        Console.WriteLine()

'                        Exit For
'                    ElseIf input.ToUpper().Equals("HELP") Then
'                        Console.ForegroundColor = ConsoleColor.Yellow
'                        Console.WriteLine(vbLf & "ExportData to export your tree")
'                        Console.ForegroundColor = ConsoleColor.Magenta
'                        Console.WriteLine("Print to print the tree")
'                        Console.ForegroundColor = ConsoleColor.Yellow
'                        Console.WriteLine("EndProgram to end the program")
'                        Console.ForegroundColor = ConsoleColor.Magenta
'                        Console.WriteLine("MainMenu to return to the main menu")
'                        Console.ForegroundColor = ConsoleColor.Gray

'                        i -= 1
'                    ElseIf input.ToUpper().Equals("EXPORTDATA") Then
'                        Console.ForegroundColor = ConsoleColor.Cyan
'                        Console.WriteLine(vbLf & "Enter path for your csv file")
'                        Console.ResetColor()
'                        input = ReadLineTrimmed()

'                        Dim endOfDirectoryPath = input.LastIndexOf("\")

'                        If endOfDirectoryPath > 0 AndAlso Directory.Exists(input.Substring(0, endOfDirectoryPath)) Then
'                            Try
'                                CsvFileHandler.ExportToCsvFile(data, input)
'                            Catch ex As Exception
'                                DisplayErrorMessage("An error during the export occured. " & vbLf & "Error message: {ex.Message}")
'                            End Try
'                        Else
'                            DisplayErrorMessage("Directory not found")
'                        End If

'                        i -= 1
'                    ElseIf String.IsNullOrEmpty(input) OrElse String.IsNullOrWhiteSpace(input) Then
'                        DisplayErrorMessage("The attribute can't be empty or a white space")
'                        i -= 1
'                    Else
'                        valuesForQuery.Add(data.Columns(i).ToString(), input)
'                    End If
'                Next

'                ' if input was not to return to the main menu, the query will be processed
'                If Not returnToMainMenu Then
'                    Dim result = Tree.CalculateResult(decisionTree.root, valuesForQuery, "")

'                    Console.WriteLine()

'                    If result.Contains("Attribute not found") Then
'                        DisplayErrorMessage("Can't caluclate outcome. Na valid route through the tree was found")
'                    Else
'                        Tree.Print(Nothing, result)
'                        Tree.PrintLegend("The colors indicate the following values:")
'                    End If
'                End If
'            Loop While Not returnToMainMenu
'        End Sub

'        Private Shared Function EnterRowValues(data As DataTable) As DataTable
'            Dim userEnteringMoreData = True

'            Do
'                Dim input As String
'                Dim enteredValues = New String(data.Columns.Count - 1) {}
'                Dim userEnteringMoreDataOrStopInput = True

'                For i As Integer = 0 To data.Columns.Count - 1
'                    Console.ForegroundColor = ConsoleColor.Cyan
'                    Console.WriteLine(vbLf & "Enter value for column {data.Columns[i]}")
'                    Console.ResetColor()
'                    input = ReadLineTrimmed()

'                    ' user can't enter an empty value
'                    If String.IsNullOrEmpty(input) OrElse String.IsNullOrWhiteSpace(input) Then
'                        DisplayErrorMessage("You must enter a value")
'                        i -= 1
'                    Else
'                        enteredValues(i) = input
'                    End If
'                Next

'                data.Rows.Add(enteredValues)

'                Do
'                    Console.ForegroundColor = ConsoleColor.Yellow
'                    Console.WriteLine(vbLf & "1 - Enter more data")
'                    Console.ForegroundColor = ConsoleColor.Cyan
'                    Console.WriteLine("2 - Stop data input")
'                    Console.ResetColor()
'                    input = ReadLineTrimmed()

'                    Select Case input
'                        Case "1"
'                            userEnteringMoreDataOrStopInput = False
'                            Exit Select

'                        Case "2"
'                            userEnteringMoreData = False
'                            userEnteringMoreDataOrStopInput = False
'                            Exit Select
'                        Case Else

'                            DisplayErrorMessage("Wrong input")
'                            Exit Select
'                    End Select
'                Loop While userEnteringMoreDataOrStopInput
'            Loop While userEnteringMoreData

'            Return data
'        End Function

'        Private Shared Function EnterColumnTitle(amountOfColumns As Integer) As DataTable
'            Dim data = New DataTable()

'            For i As Integer = 0 To amountOfColumns - 1
'                Console.ForegroundColor = ConsoleColor.Yellow
'                Console.WriteLine(vbLf & "Enter title for column {i}")
'                Console.ResetColor()
'                Dim input = ReadLineTrimmed()

'                ' user can't enter an empty title
'                If String.IsNullOrEmpty(input) OrElse String.IsNullOrWhiteSpace(input) Then
'                    DisplayErrorMessage("You must enter a title")
'                    i -= 1
'                Else
'                    Try
'                        data.Columns.Add(input)
'                    Catch ex As Exception
'                        DisplayErrorMessage("An error occured. Error message: {ex.Message}")
'                        i -= 1
'                    End Try
'                End If
'            Next

'            Return data
'        End Function

'        Private Shared Sub DisplayErrorMessage(errorMessage As String)
'            Console.ForegroundColor = ConsoleColor.Red
'            Console.WriteLine(vbLf & "{errorMessage}" & vbLf)
'            Console.ResetColor()
'        End Sub

'        Private Shared Sub EndProgram()
'            Environment.[Exit](0)
'        End Sub
'    End Class
'End Namespace

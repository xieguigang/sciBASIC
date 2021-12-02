Imports System.ComponentModel
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags

Namespace ConsoleTableApp
    Friend Class Program
        Public Shared Sub Main(ByVal args As String())
            ConsoleTableBuilder.From(Function() New ConsoleTableBaseData With {
                            .Rows = New List(Of List(Of Object)) From {
                                New List(Of Object) From {
                                    "a1",
                                    "b1",
                                    "c1"
                                },
                                New List(Of Object) From {
                                    "a2",
                                    "b2",
                                    "c2"
                                }
                            },
                            .Column = New List(Of Object) From {
                                "-A-",
                                "-B-",
                                "-C-"
                            }
                        }).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine()
            ConsoleTableBuilder.From(New List(Of Integer) From {
                            1,
                            2,
                            3,
                            4,
                            5,
                            6
                        }).WithFormat(ConsoleTableBuilderFormat.Alternative).ExportAndWriteLine()
            ConsoleTableBuilder.From(New List(Of String) From {
                            "1",
                            "2",
                            "3",
                            "4",
                            "5",
                            "6"
                        }).WithFormat(ConsoleTableBuilderFormat.Alternative).WithColumn("I'm a custom name").ExportAndWriteLine()
            ConsoleTableBuilder.From(SampleTableData()).WithFormat(ConsoleTableBuilderFormat.Alternative).WithColumnFormatter(1, Function(text) $"[ {text.ToUpper()} ]").WithFormatter(1, Function(text) $"<{text}>").WithMinLength(New Dictionary(Of Integer, Integer) From {
                            {1, 30}
                        }).WithTextAlignment(New Dictionary(Of Integer, TextAligntment) From {
                            {1, TextAligntment.Right}
                        }).WithHeaderTextAlignment(New Dictionary(Of Integer, TextAligntment) From {
                            {1, TextAligntment.Center}
                        }).WithTitle("MY TABLE", ConsoleColor.DarkRed, ConsoleColor.Gray, TextAligntment.Right).ExportAndWriteLine(TableAligntment.Center)
            ConsoleTableBuilder.From(New List(Of Object()) From {
                            New Object() {"s"}
                        }).WithTitle("abcdefghlm").ExportAndWriteLine()
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("From [DataTable] type and Minimal format:")
            ConsoleTableBuilder.From(SampleTableData()).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine()
            Call _____________________________PrintDemoDivider()
            Dim strBuilder01 = ConsoleTableBuilder.From(SampleTableData()).WithPaddingLeft(String.Empty).WithCharMapDefinition().Export()
            Console.WriteLine(strBuilder01)
            Call _____________________________PrintDemoDivider()
            Dim strBuilder02 = ConsoleTableBuilder.From(SampleTableData()).WithTitle("MARKDOWN WITH TITLE ???").WithPaddingLeft(String.Empty).WithFormat(ConsoleTableBuilderFormat.MarkDown).Export()
            Console.WriteLine(strBuilder02)
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("Text alignment with table title")
            ConsoleTableBuilder.From(SampleListData).WithTextAlignment(New Dictionary(Of Integer, TextAligntment) From {
                            {0, TextAligntment.Center},
                            {1, TextAligntment.Right},
                            {3, TextAligntment.Right},
                            {100, TextAligntment.Right}
                        }).WithMinLength(New Dictionary(Of Integer, Integer) From {
                            {1, 30}
                        }).WithCharMapDefinition(CharMapDefinition.FramePipDefinition).WithTitle("HELLO I AM TITLE", ConsoleColor.Green, ConsoleColor.DarkGray, TextAligntment.Right).WithFormatter(1, Function(text) text.ToUpper().Replace(" ", "-") & " «").ExportAndWriteLine(TableAligntment.Center)
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("Text alignment and column min length")
            ConsoleTableBuilder.From(SampleTableData()).WithTextAlignment(New Dictionary(Of Integer, TextAligntment) From {
                            {0, TextAligntment.Center},
                            {1, TextAligntment.Right},
                            {3, TextAligntment.Center},
                            {100, TextAligntment.Right}
                        }).WithMinLength(New Dictionary(Of Integer, Integer) From {
                            {1, 35},
                            {3, 10}
                        }).WithFormatter(2, Function(text)
                                                Dim chars As Char() = text.ToCharArray()
                                                Array.Reverse(chars)
                                                Return New [String](chars)
                                            End Function).WithTitle("Hello, everyone! This is the LONGEST TEXT EVER! I was inspired by the various other 'longest texts ever' on the internet, and I wanted to make my own. So here it is!".ToUpper(), ConsoleColor.Yellow, ConsoleColor.DarkMagenta).WithCharMapDefinition(CharMapDefinition.FrameDoublePipDefinition).WithFormatter(3, Function(text)
                                                                                                                                                                                                                                                                                                                                                                                             If String.IsNullOrEmpty(text) OrElse text.Trim().Length = 0 Then
                                                                                                                                                                                                                                                                                                                                                                                                 Return "0 $"
                                                                                                                                                                                                                                                                                                                                                                                             Else
                                                                                                                                                                                                                                                                                                                                                                                                 Return text & " $"
                                                                                                                                                                                                                                                                                                                                                                                             End If
                                                                                                                                                                                                                                                                                                                                                                                         End Function).WithColumnFormatter(3, Function(text) "#").ExportAndWriteLine()
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("Custom format using CharMap")
            ConsoleTableBuilder.From(SampleTableData()).WithCharMapDefinition(CharMapDefinition.FramePipDefinition, New Dictionary(Of HeaderCharMapPositions, Char) From {
                            {HeaderCharMapPositions.TopLeft, "╒"c},
                            {HeaderCharMapPositions.TopCenter, "╤"c},
                            {HeaderCharMapPositions.TopRight, "╕"c},
                            {HeaderCharMapPositions.BottomLeft, "╞"c},
                            {HeaderCharMapPositions.BottomCenter, "╪"c},
                            {HeaderCharMapPositions.BottomRight, "╡"c},
                            {HeaderCharMapPositions.BorderTop, "═"c},
                            {HeaderCharMapPositions.BorderRight, "│"c},
                            {HeaderCharMapPositions.BorderBottom, "═"c},
                            {HeaderCharMapPositions.BorderLeft, "│"c},
                            {HeaderCharMapPositions.Divider, "│"c}
                        }).ExportAndWriteLine(TableAligntment.Right)
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("Custom format using CharMap: Header has no divider")
            ConsoleTableBuilder.From(SampleTableData()).WithCharMapDefinition(CharMapDefinition.FramePipDefinition).WithCharMapDefinition(CharMapDefinition.FramePipDefinition, New Dictionary(Of HeaderCharMapPositions, Char) From {
                            {HeaderCharMapPositions.TopLeft, "╒"c},
                            {HeaderCharMapPositions.TopCenter, "═"c},
                            {HeaderCharMapPositions.TopRight, "╕"c},
                            {HeaderCharMapPositions.BottomLeft, "╞"c},
                            {HeaderCharMapPositions.BottomCenter, "╤"c},
                            {HeaderCharMapPositions.BottomRight, "╡"c},
                            {HeaderCharMapPositions.BorderTop, "═"c},
                            {HeaderCharMapPositions.BorderRight, "│"c},
                            {HeaderCharMapPositions.BorderBottom, "═"c},
                            {HeaderCharMapPositions.BorderLeft, "│"c},
                            {HeaderCharMapPositions.Divider, " "c}
                        }).ExportAndWriteLine()
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("No header FramePipDefinition")
            ConsoleTableBuilder.From(SampleListData).WithCharMapDefinition(CharMapDefinition.FramePipDefinition).ExportAndWriteLine()
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("No header FrameDoublePipDefinition:")
            ConsoleTableBuilder.From(SampleListData).WithCharMapDefinition(CharMapDefinition.FrameDoublePipDefinition).ExportAndWriteLine()
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("From [DataTable] type and Default format:")
            ConsoleTableBuilder.From(SampleTableData()).ExportAndWriteLine()
            Console.WriteLine("From [DataTable] type and Minimal format:")
            ConsoleTableBuilder.From(SampleTableData()).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine()
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("From [List] type and Alternative format:")
            ConsoleTableBuilder.From(SampleListData).WithFormat(ConsoleTableBuilderFormat.Alternative).ExportAndWriteLine()
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("From [List] type and MarkDown format w/ custom column name:")
            ConsoleTableBuilder.From(SampleListData).WithFormat(ConsoleTableBuilderFormat.MarkDown).WithColumn(New List(Of String) From {
                            "N A M E",
                            "[Position]",
                            "Office",
                            "<Age>",
                            "Something else I don't care"
                        }).ExportAndWriteLine()
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("From [List<T>] (where T:class) type and Minimal format:")
            ConsoleTableBuilder.From(SampleEmployeesList).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine()
            Call _____________________________PrintDemoDivider()
            Console.WriteLine("From [List<T>] (where T: !class) type and Alternative format:")
            ConsoleTableBuilder.From(New List(Of Integer) From {
                            1,
                            2,
                            3,
                            4,
                            5,
                            6
                        }).WithFormat(ConsoleTableBuilderFormat.Alternative).WithColumn("I'm a custom name").ExportAndWrite()
            Call _____________________________PrintDemoDivider()
            ConsoleTableBuilder.From(New List(Of Object()) From {
                            New Object() {"luong", "son", "ba", Nothing, "phim", Nothing, Nothing, Nothing, 2, Nothing},
                            New Object() {"chuc", "anh", "dai", "nhac", Nothing, Nothing, Nothing}
                        }).TrimColumn(True).AddRow(New List(Of Object) From {
                            1,
                            "this",
                            "is",
                            "new",
                            "row",
                            "use",
                            "<List>",
                            Nothing,
                            Nothing,
                            Nothing
                        }).AddRow((New Object() {"2", "new row", "use", "array[] values", Nothing, Nothing})).WithMetadataRow(MetaRowPositions.Top, Function(b) String.Format("=> First top line <{0}>", "FIRST")).WithMetadataRow(MetaRowPositions.Top, Function(b) String.Format("=> Second top line <{0}>", "SECOND")).WithMetadataRow(MetaRowPositions.Bottom, Function(b) String.Format("=> This table has {3} rows and {2} columns=> [{0}] - [test value {1}]", "test value 1", 2, b.NumberOfColumns, b.NumberOfRows)).WithMetadataRow(MetaRowPositions.Bottom, Function(b) String.Format("=> Bottom line <{0}>", "HELLO WORLD")).WithColumn(New List(Of String) From {
                            "THIS",
                            "IS",
                            "ADVANCED",
                            "OPTIONS"
                        }).WithCharMapDefinition(New Dictionary(Of CharMapPositions, Char) From {
                            {CharMapPositions.BorderLeft, "¡"c},
                            {CharMapPositions.BorderRight, "¡"c},
                            {CharMapPositions.DividerY, "¡"c}
                        }).WithHeaderCharMapDefinition(New Dictionary(Of HeaderCharMapPositions, Char) From {
                            {HeaderCharMapPositions.BottomLeft, "»"c},
                            {HeaderCharMapPositions.BottomCenter, "»"c},
                            {HeaderCharMapPositions.BottomRight, "»"c},
                            {HeaderCharMapPositions.Divider, "¡"c},
                            {HeaderCharMapPositions.BorderBottom, "»"c},
                            {HeaderCharMapPositions.BorderLeft, "¡"c},
                            {HeaderCharMapPositions.BorderRight, "¡"c}
                        }).ExportAndWriteLine()
            Console.ReadKey()
        End Sub

        Private Shared Function SampleTableData() As DataTable
            Dim table As DataTable = New DataTable()
            table.Columns.Add("Name", GetType(String))
            table.Columns.Add("Position", GetType(String))
            table.Columns.Add("Office", GetType(String))
            table.Columns.Add("Age", GetType(Integer))
            table.Columns.Add("Start Date", GetType(Date))
            table.Rows.Add("Airi Satou", "Accountant", "Tokyo", 33, New DateTime(2017, 05, 09))
            table.Rows.Add("Angelica Ramos", "Chief Executive Officer (CEO)", "New York", 47, New DateTime(2017, 01, 12))
            table.Rows.Add("Ashton Cox", "Junior Technical Author", "London", 46, New DateTime(2017, 04, 02))
            table.Rows.Add("Bradley Greer", "Software Engineer", "San Francisco", 28, New DateTime(2017, 11, 15))
            Return table
        End Function

        Private Shared SampleShortListData As List(Of List(Of Object)) = New List(Of List(Of Object)) From {
            New List(Of Object) From {
                ""
            },
            New List(Of Object) From {
                ""
            },
            New List(Of Object) From {
                ""
            },
            New List(Of Object) From {
                ""
            }
        }
        Private Shared SampleListData As List(Of List(Of Object)) = New List(Of List(Of Object)) From {
            New List(Of Object) From {
                "Sakura Yamamoto",
                "Support Engineer",
                "London",
                46
            },
            New List(Of Object) From {
                "Serge Baldwin",
                "Data Coordinator",
                "San Francisco",
                28,
                "something else"
            },
            New List(Of Object) From {
                "Shad Decker",
                "Regional Director",
                "Edinburgh"
            }
        }
        Private Shared SampleEmployeesList As List(Of Employee) = New List(Of Employee) From {
            New Employee("Airi Satou", "Accountant", "Tokyo", 33, New DateTime(2017, 05, 09)),
            New Employee("Angelica Ramos", "Chief Executive Officer (CEO)", "New York", 47, New DateTime(2017, 01, 12)),
            New Employee("Ashton Cox", "Junior Technical Author", "London", 46, New DateTime(2017, 04, 02)),
            New Employee("Bradley Greer", "Software Engineer", "San Francisco", 28, New DateTime(2017, 11, 15))
        }

        Private Class Employee
            Public Sub New(ByVal name As String, ByVal position As String, ByVal office As String, ByVal age As Integer, ByVal startDate As Date)
                Me.Name = name
                Me.Position = position
                Me.Office = office
                Me.Age = age
                Me.StartDate = startDate
            End Sub

            <Description("N - A - M - E")>
            Public Property Name As String
            Public Property Position As String
            Public Property Office As String
            Public Property Age As Integer
            Public Property StartDate As Date
        End Class

        Private Shared Sub _____________________________PrintDemoDivider()
            Console.WriteLine()
            Console.WriteLine()
            ' Console.Write(string.Format("\n\n{0}\n", Enumerable.Range(0, Console.WindowWidth).Select(x => "=").Aggregate((s, a) => s + a)));
        End Sub
    End Class
End Namespace

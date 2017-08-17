Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace BarPlot

    Public Module BarPlotDataExtensions

        Public Function LoadDataSet(path$, Optional schema$ = "scibasic.category31()", Optional groupByColumn! = yes, Optional tsv! = no) As BarDataGroup
            Dim csv As File = File.Load(path)
            Dim samples As New List(Of BarDataSample)
            Dim serialsName$()
            Dim matrix As IEnumerable(Of String())

            If groupByColumn Then
                matrix = csv.Columns
            Else
                matrix = csv.Select(Function(row) row.ToArray)
            End If

            With matrix
                serialsName = .First _
                    .Skip(1) _
                    .ToArray

                For Each column As String() In .Skip(1)
                    Dim groupName$ = column(Scan0)
                    Dim data#() = column.Skip(1).AsDouble()
                    Dim sample As New BarDataSample With {
                        .Tag = groupName,
                        .data = data
                    }

                    samples += sample
                Next
            End With

            Dim colors As Color() = Designer.GetColors(schema, serialsName.Length)
            Dim out As New BarDataGroup With {
                .Samples = samples,
                .Serials = serialsName _
                    .SeqIterator _
                    .Select(Function(i)
                                Return New NamedValue(Of Color) With {
                                    .Name = i.value,
                                    .Value = colors(i)
                                }
                            End Function) _
                    .ToArray
            }

            Return out
        End Function
    End Module
End Namespace
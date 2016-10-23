Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq

Namespace csv

    ''' <summary>
    ''' 
    ''' ###### csv format
    ''' 
    ''' ```
    ''' "    ",serial1,serial2,...
    ''' group1,
    ''' group2,
    ''' group3,
    ''' ...
    ''' ```
    ''' </summary>
    Public Module BarData

        <Extension>
        Public Function LoadBarData(csv$, Optional colors$() = Nothing) As BarDataGroup
            Dim file As New File(csv)
            Dim header As RowObject = file.First
            Dim names$() = header.Skip(1).ToArray
            Dim clData As Color() = If(
                colors.IsNullOrEmpty,
                ChartColors.Shuffles,
                colors.ToArray(AddressOf ToColor))

            Return New BarDataGroup With {
                .Serials = names _
                    .SeqIterator _
                    .ToArray(Function(x) New NamedValue(Of Color) With {
                        .Name = x.obj,
                        .x = clData(x.i)
                    }),
                .Samples = file _
                    .Skip(1) _
                    .ToArray(Function(x) New BarDataSample With {
                        .Tag = x.First,
                        .data = x _
                            .Skip(1) _
                            .ToArray(AddressOf Val)
                    })
            }
        End Function
    End Module
End Namespace
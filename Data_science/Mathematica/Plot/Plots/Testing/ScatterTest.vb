Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

Module ScatterTest

    Sub Main()
        For Each file As String In (ls - l - r - "*.csv" <= "D:\OneDrive\2017-9-25")
            Dim csv As csv = csv.Load(file)
            Dim X = csv.Columns(0).Skip(1).AsNumeric
            Dim Y = csv.Columns(csv.Headers.IndexOf("mean")).Skip(1).AsNumeric
            Dim err = csv.Columns(csv.Headers.IndexOf("sem")).Skip(1).AsNumeric
            Dim points = X.SeqIterator.Select(Function(i) New PointData With {.errMinus = err(i), .errPlus = err(i), .pt = New PointF With {.X = i.value, .Y = Y(i)}}).ToArray
            Dim s As New SerialData With {.color = Color.Black, .lineType = DashStyle.Dash, .width = 2, .title = file.BaseName, .PointSize = 5, .pts = points}

            Call Scatter.Plot({s}, showLegend:=False).Save($"D:\OneDrive\2017-9-25\{file.BaseName}.png")
        Next
    End Sub
End Module

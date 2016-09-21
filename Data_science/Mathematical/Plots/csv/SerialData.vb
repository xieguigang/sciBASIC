Imports System.Drawing
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace csv

    Public Class SerialData

        Public Property serial As String
        Public Property X As Single
        Public Property Y As Single
        Public Property value As Double
        Public Property tag As String
        Public Property errPlus As Double
        Public Property errMinus As Double

        Public Shared Function GetData(csv As String, Optional colors As Color() = Nothing) As IEnumerable(Of Plots.SerialData)
            Return GetData(csv.LoadCsv(Of SerialData), colors)
        End Function

        Public Shared Iterator Function GetData(data As IEnumerable(Of SerialData), Optional colors As Color() = Nothing) As IEnumerable(Of Plots.SerialData)
            Dim gs = From x As SerialData
                     In data
                     Select x
                     Group x By x.serial Into Group

            colors = If(
                colors.IsNullOrEmpty,
                Imaging.ChartColors.Shuffles,
                colors)

            For Each g In gs.SeqIterator

                Yield New Plots.SerialData With {
                    .title = g.obj.serial,
                    .color = colors(g),
                    .pts = LinqAPI.Exec(Of PointData) <=
                        From x As SerialData
                        In g.obj.Group
                        Select New PointData With {
                            .errMinus = x.errMinus,
                            .errPlus = x.errPlus,
                            .pt = New PointF(x.X, x.Y),
                            .Tag = x.tag,
                            .value = x.value
                        }
                    }
            Next
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
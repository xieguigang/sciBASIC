Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module Histogram

    ''' <summary>
    ''' {x, y}
    ''' </summary>
    Public Structure HistogramData
        Public x#, y#

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    <Extension>
    Public Function Plot(data As IEnumerable(Of HistogramData),
                         Optional color$ = "blue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing) As Bitmap

        Return New HistogramGroup With {
            .Serials = {
                New NamedValue(Of Color) With {
                    .Name = NameOf(data),
                    .x = color.ToColor(Drawing.Color.Blue)
                }
            }
        }.Plot(bg, size, margin)
    End Function

    <Extension>
    Public Function Plot(groups As HistogramGroup,
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing) As Bitmap

    End Function

    Public Class HistogramGroup : Inherits ProfileGroup

        Public Property Samples As NamedValue(Of HistogramData())()
    End Class
End Module

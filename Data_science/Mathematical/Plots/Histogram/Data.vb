Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class HistogramSample

    Public Property Tag As String
    Public Property data As Double()

    Public ReadOnly Property StackedSum As Double
        Get
            Return data.Sum
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class HistogramGroup

    ''' <summary>
    ''' 与<see cref="HistogramSample.data"/>里面的数据顺序是一致的
    ''' </summary>
    ''' <returns></returns>
    Public Property Serials As NamedValue(Of Color)()
    Public Property Samples As HistogramSample()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Shared Function FromDistributes(data As IEnumerable(Of Double), Optional base! = 10.0F, Optional color$ = "darkblue") As HistogramGroup
        Dim source = data.Distributes(base!)
        Dim bg As Color = color.ToColor(onFailure:=Drawing.Color.DarkBlue)
        Dim values As New List(Of Double)
        Dim serials = LinqAPI.Exec(Of NamedValue(Of Color)) <=
            From lv As Integer
            In source.Keys
            Select New NamedValue(Of Color) With {
                .Name = lv.ToString,
                .x = bg
            }

        For Each x In serials
            values += source(CInt(x.Name)).value
        Next

        Return New HistogramGroup With {
            .Serials = serials,
            .Samples = {
                New HistogramSample With {
                    .Tag = "Distribution",
                    .data = values
                }
            }
        }
    End Function
End Class

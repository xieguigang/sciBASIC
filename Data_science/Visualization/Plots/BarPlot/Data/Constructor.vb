Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Distributions

Namespace BarPlot.Data

    Public Module Constructor

        ''' <summary>
        ''' 这个应该是生成直方图的数据
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="base!"></param>
        ''' <param name="serialColor$"></param>
        ''' <returns></returns>
        Public Function FromDistributes(data As IEnumerable(Of Double), Optional base! = 10.0F, Optional serialColor$ = "darkblue") As BarDataGroup
            Dim source = data.Distributes(base!)
            Dim bg As Color = serialColor.ToColor(onFailure:=Color.DarkBlue)
            Dim values As New List(Of Double)
            Dim serials = LinqAPI.Exec(Of NamedValue(Of Color)) _
 _
                () <= From lv As Integer
                      In source.Keys
                      Let tag As String = lv.ToString
                      Select New NamedValue(Of Color) With {
                          .Name = tag,
                          .Value = bg
                      }

            For Each x As NamedValue(Of Color) In serials
                values += source(CInt(x.Name)).Value
            Next

            Return New BarDataGroup With {
                .Serials = serials,
                .Samples = {
                    New BarDataSample With {
                        .Tag = "Distribution",
                        .data = values
                    }
                }
            }
        End Function

        <Extension>
        Public Function SimpleSerials(data As IEnumerable(Of NamedValue(Of Double)), Optional posColor$ = "red", Optional ngColor$ = "darkblue") As BarDataGroup
            Dim dataVector As NamedValue(Of Double)() = data.ToArray
            Dim colors As NamedValue(Of Color)() = dataVector _
                .Select(Function(d)
                            Dim color As Color

                            If d.Value > 0 Then
                                color = posColor.ToColor(Color.Red)
                            Else
                                color = ngColor.ToColor(Color.DarkBlue)
                            End If

                            Return New NamedValue(Of Color) With {
                                .Name = d.Name,
                                .Value = color
                            }
                        End Function) _
                .ToArray

            Return New BarDataGroup With {
                .Serials = colors,
                .Samples = {
                    New BarDataSample With {
                        .data = dataVector.Values,
                        .Tag = "N/A"
                    }
                }
            }
        End Function
    End Module
End Namespace
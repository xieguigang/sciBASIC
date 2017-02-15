Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace BarPlot.Histogram

    Module Extensions

        ''' <summary>
        ''' Syntax helper
        ''' </summary>
        ''' <param name="hist"></param>
        ''' <param name="step!"></param>
        ''' <param name="legend"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NewModel(hist As Dictionary(Of Double, IntegerTagged(Of Double)), step!, legend As Legend) As HistProfile
            Return New HistProfile(hist, [step]) With {
                .legend = legend
            }
        End Function
    End Module
End Namespace
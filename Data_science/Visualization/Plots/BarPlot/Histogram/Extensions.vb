#Region "Microsoft.VisualBasic::852aa3effee3f1da81d005e1efbf7494, Data_science\Visualization\Plots\BarPlot\Histogram\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module Extensions
    ' 
    '         Function: NewModel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Math.Distributions.BinBox

Namespace BarPlot.Histogram

    Module Extensions

        ''' <summary>
        ''' Syntax helper
        ''' </summary>
        ''' <param name="hist"></param>
        ''' <param name="step!"></param>
        ''' <param name="legend"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function NewModel(hist As IEnumerable(Of DataBinBox(Of Double)), step!, legend As Legend) As HistProfile
            Return New HistProfile(hist, [step]) With {
                .legend = legend
            }
        End Function
    End Module
End Namespace

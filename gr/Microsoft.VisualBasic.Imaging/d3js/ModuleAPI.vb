Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace d3js

    Public Module ModuleAPI

        ''' <summary>
        ''' A D3 plug-in for automatic label placement using simulated annealing that easily 
        ''' incorporates into existing D3 code, with syntax mirroring other D3 layouts.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function labeler(Optional maxMove# = 5,
                                Optional maxAngle# = 0.5,
                                Optional w_len# = 0.2,      ' leader line length 
                                Optional w_inter# = 1.0,    ' leader line intersection
                                Optional w_lab2# = 30.0,    ' label-label overlap
                                Optional w_lab_anc# = 30.0, ' label-anchor overlap
                                Optional w_orient# = 3.0    ' orientation bias
                                ) As Labeler

            Return New Labeler With {
                .maxAngle = maxAngle,
                .maxMove = maxMove,
                .w_inter = w_inter,
                .w_lab2 = w_lab2,
                .w_lab_anc = w_lab_anc,
                .w_len = w_len,
                .w_orient = w_orient
            }
        End Function

        <Extension>
        Public Function Label(g As Graphics2D, labels As IEnumerable(Of String), Optional fontCSS$ = CSSFont.Win7Normal) As IEnumerable(Of Label)
            Dim font As Font = CSSFont _
                .TryParse(fontCSS) _
                .GDIObject

            Return labels _
                .SafeQuery _
                .Select(Function(s$)
                            Dim size As SizeF = g.MeasureString(s, font)

                            Return New Label With {
                                .text = s,
                                .width = size.Width,
                                .height = size.Height
                            }
                        End Function)
        End Function
    End Module
End Namespace
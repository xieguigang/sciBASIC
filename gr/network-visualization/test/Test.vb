#Region "Microsoft.VisualBasic::4345d30a5cdf32fc5b787ecdb1353bce, gr\network-visualization\test\Test.vb"

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

    ' Module Test
    ' 
    '     Sub: Main, TestStyling
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Styling
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Module Test

    Sub Main()
        Call TestStyling()
        ' Call TestPageRank()

        Pause()
    End Sub



    Sub TestStyling()
        Dim json As New StyleJSON With {
            .nodes = New Dictionary(Of String, NodeStyle) From {
            {
                "*", New NodeStyle With {
                    .fill = "black",
                    .size = "size",
                    .stroke = Stroke.AxisStroke
                }
            },
            {
                "type = example", New NodeStyle With {
                    .fill = "red",
                    .size = "scale(size, 5, 30)"
                }
            }
            },
            .labels = New Dictionary(Of String, LabelStyle) From {
            {
                "degree > 2", New LabelStyle With {
                    .fill = "brown"
                }
            }
            }
        }
        Dim styles As StyleMapper = StyleMapper.FromJSON(json)
    End Sub
End Module

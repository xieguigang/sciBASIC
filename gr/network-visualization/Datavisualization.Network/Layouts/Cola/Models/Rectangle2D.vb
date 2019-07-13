#Region "Microsoft.VisualBasic::ce98a48a91e19346aa68066565f7cd1b, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Models\Rectangle2D.vb"

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

    '     Class Rectangle2D
    ' 
    '         Properties: space_left
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports RectangleF = Microsoft.VisualBasic.Imaging.LayoutModel.Rectangle2D

Namespace Layouts.Cola

    Public Class Rectangle2D : Inherits RectangleF

        Public Property space_left As Double

        Sub New(x1#, x2#, y1#, y2#)
            Call MyBase.New(x1, x2, y1, y2)
        End Sub

        Sub New()
            Call MyBase.New
        End Sub
    End Class
End Namespace

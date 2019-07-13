#Region "Microsoft.VisualBasic::84894a374131f725dfba9d39b573f747, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Models\Accessor\RectAccessors.vb"

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

    '     Class RectAccessors
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Layouts.Cola

    Public Class RectAccessors
        Public getCentre As Func(Of Rectangle2D, Double)
        Public getOpen As Func(Of Rectangle2D, Double)
        Public getClose As Func(Of Rectangle2D, Double)
        Public getSize As Func(Of Rectangle2D, Double)
        Public makeRect As Func(Of Double, Double, Double, Double, Rectangle2D)
        Public findNeighbours As Action(Of Node, RBTree(Of Integer, Node))
    End Class
End Namespace

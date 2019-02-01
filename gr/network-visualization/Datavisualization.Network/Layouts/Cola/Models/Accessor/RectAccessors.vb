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
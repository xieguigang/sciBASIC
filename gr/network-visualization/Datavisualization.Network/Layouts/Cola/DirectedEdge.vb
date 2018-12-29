Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Layouts.Cola

    ''' <summary>
    ''' An object with three point properties, the intersection with the
    ''' source rectangle (sourceIntersection), the intersection with then
    ''' target rectangle (targetIntersection), And the point an arrow
    ''' head of the specified size would need to start (arrowStart).
    ''' </summary>
    Public Structure DirectedEdge

        Public sourceIntersection As Point2D
        Public targetIntersection As Point2D
        Public arrowStart As Point2D

    End Structure
End Namespace
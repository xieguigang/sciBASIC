Imports Microsoft.VisualBasic.Language.JavaScript

Namespace Layouts.Cola

    ''' <summary>
    ''' The common type interface that will be used in <see cref="Projection"/>
    ''' </summary>
    Public Interface IGraphNode : Inherits IJavaScriptObjectAccessor
        Property bounds As Rectangle2D
        Property variable As Variable
        Property width As Double
        Property height As Double
        Property px As Double?
        Property py As Double?
        Property x As Double
        Property y As Double

        Property fixed As Boolean
        Property fixedWeight As Double?

    End Interface

    'Public Class GraphNode : Inherits Leaf
    '    Implements IGraphNode

    '    Public Property width As Double Implements IGraphNode.width
    '    Public Property height As Double Implements IGraphNode.height

    '    Public Overrides Property variable As Variable Implements IGraphNode.variable

    '    Public Overrides Property bounds As Rectangle2D Implements IGraphNode.bounds

    '    Public Property px As Double? Implements IGraphNode.px

    '    Public Property py As Double? Implements IGraphNode.py

    '    Public Property x As Double Implements IGraphNode.x

    '    Public Property y As Double Implements IGraphNode.y

    '    Public Property fixed As Boolean Implements IGraphNode.fixed

    '    Public Property fixedWeight As Double? Implements IGraphNode.fixedWeight
    'End Class
End Namespace
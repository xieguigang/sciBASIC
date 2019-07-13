#Region "Microsoft.VisualBasic::c304147761157bff532cd096bb5df7e7, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Models\Accessor\LinkAccessor.vb"

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

    '     Class LinkAccessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class LinkTypeAccessor
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Properties: GetLinkType
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter

Namespace Layouts.Cola

    Public Class LinkAccessor : Inherits LinkAccessor(Of Link(Of Integer))

        Public getLength As Func(Of Link(Of Integer), Double)

        Sub New()
            Me.getSourceIndex = Function(e) e.source
            Me.getTargetIndex = Function(e) e.target
            Me.getLength = Function(e) e.length
            Me.setLength = Sub(e, l) e.length = l
        End Sub
    End Class

    Public Class LinkTypeAccessor(Of Link) : Inherits LinkAccessor(Of Link)

        Public Delegate Function IGetLinkType(link As Link) As Integer

        ''' <summary>
        ''' return a unique identifier for the type of the link
        ''' </summary>
        ''' <returns></returns>
        Public Property GetLinkType As IGetLinkType
    End Class
End Namespace

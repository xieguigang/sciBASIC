#Region "Microsoft.VisualBasic::6f80cab578cdaaf40a82d831cf378d70, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Layout3D\Module1.vb"

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

    '     Class IConstraint
    ' 
    '         Properties: axis, gap, left, right
    ' 
    '     Class LinkSepAccessor
    ' 
    '         Properties: axis
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Layouts.Cola

    Public Class IConstraint
        Public Property axis As String
        Public Property left() As Double
        Public Property right() As Double
        Public Property gap() As Double
    End Class

    Public Class LinkSepAccessor(Of Link)
        Inherits LinkAccessor

        Public Property axis As String

    End Class
End Namespace

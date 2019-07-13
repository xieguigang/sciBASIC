#Region "Microsoft.VisualBasic::53a0ca48bd36732716a0fbce6455a3f5, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Layout3D\Node3D.vb"

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

    '     Class Node3D
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Layouts.Cola

    Public Class Node3D : Inherits Node

        Public z As Double

        Public Sub New(Optional x As Double = 0, Optional y As Double = 0, Optional z As Double = 0)
            Me.x = x
            Me.y = y
            Me.z = z
        End Sub
    End Class
End Namespace

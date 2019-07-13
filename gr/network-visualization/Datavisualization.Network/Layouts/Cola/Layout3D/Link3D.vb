#Region "Microsoft.VisualBasic::c6acdb69e27f1d97a1966e22b96e34c4, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Layout3D\Link3D.vb"

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

    '     Class Link3D
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: actualLength
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript

Namespace Layouts.Cola

    Public Class Link3D

        Public length As Double
        Public source As Integer
        Public target As Integer

        Sub New()
        End Sub

        Public Sub New(source As Integer, target As Integer)
            Me.source = source
            Me.target = target
        End Sub

        Public Function actualLength(x As Double()()) As Double
            Return Math.Sqrt(x.Reduce(Function(c As Double, v As Double())
                                          Dim dx = v(Me.target) - v(Me.source)
                                          Return c + dx * dx
                                      End Function, 0))
        End Function
    End Class
End Namespace

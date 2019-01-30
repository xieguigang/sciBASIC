Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter

Namespace Layouts.Cola

    Public Class LinkAccessor : Inherits LinkLengthAccessor(Of Link3D)

        Public Function getSourceIndex(e As any) As Double
            Return e.source
        End Function
        Public Function getTargetIndex(e As any) As Double
            Return e.target
        End Function
        Public Function getLength(e As any) As Double
            Return e.length
        End Function
        Public Sub setLength(e As any, l As Double)
            e.length = l
        End Sub
    End Class
End Namespace
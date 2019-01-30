Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter

Namespace Layouts.Cola

    Public Class LinkAccessor(Of T) : Inherits LinkLengthAccessor(Of T)

        Public Function getSourceIndex(e As T) As Double
            Return e.source
        End Function
        Public Function getTargetIndex(e As T) As Double
            Return e.target
        End Function
        Public Function getLength(e As T) As Double
            Return e.length
        End Function
        Public Sub setLength(e As T, l As Double)
            e.length = l
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
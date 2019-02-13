Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter
Imports Microsoft.VisualBasic.Language.JavaScript

Namespace Layouts.Cola

    Public Class Link(Of NodeRefType)

        Public Property source() As NodeRefType
        Public Property target() As NodeRefType

        ''' <summary>
        ''' ideal length the layout should try to achieve for this link 
        ''' </summary>
        ''' <returns></returns>
        Public Property length() As Double

        ''' <summary>
        ''' how hard we should try to satisfy this link's ideal length
        ''' must be in the range: ``0 &lt; weight &lt;= 1``
        ''' if unspecified 1 is the default
        ''' </summary>
        ''' <returns></returns>
        Public Property weight() As Double
    End Class

    Public Delegate Function LinkNumericPropertyAccessor(t As Link(Of Node)) As Double

    Public Class LinkLengthTypeAccessor
        Inherits LinkLengthAccessor(Of Link(Of Node))

        Public Shadows [getType] As LinkNumericPropertyAccessor
        Public getMinSeparation As UnionType(Of Double)
    End Class
End Namespace
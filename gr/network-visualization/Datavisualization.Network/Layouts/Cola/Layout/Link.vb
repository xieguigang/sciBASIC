Namespace Layouts.Cola

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="NodeRefType">可以是节点对象类型的实例或者节点数组的下标数字值</typeparam>
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
End Namespace
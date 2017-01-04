Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language

Namespace Darwinism.GAF

    Public Structure TimeValue

        ''' <summary>
        ''' X
        ''' </summary>
        Dim Time#
        ''' <summary>
        ''' ``(y) = f(x)``
        ''' </summary>
        Dim Y#

        Public Overrides Function ToString() As String
            Return $"[{Time}] {Y}"
        End Function

        ''' <summary>
        ''' 从<paramref name="X"/>之中找出离<paramref name="y"/>之中的<see cref="TimeValue.Time"/>最近的元素然后生成index
        ''' </summary>
        ''' <param name="X#">假设X是从小到大排序的</param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Function BuildIndex(X#(), y As TimeValue()) As IndexOf(Of Double)
            Dim xTime As New List(Of Double)

            ' 在这个函数里面不需要任何排序操作，否则会打乱原有的一一对应关系

            For Each time As TimeValue In y
                Dim minD# = Integer.MaxValue
                Dim yx#

                For Each xi As Double In X
                    Dim d = Math.Abs(xi - time.Time)

                    If d <= minD Then
                        minD = d
                        yx = xi
                    End If
                Next

                xTime += yx#
            Next

            Return New IndexOf(Of Double)(xTime)
        End Function
    End Structure
End Namespace
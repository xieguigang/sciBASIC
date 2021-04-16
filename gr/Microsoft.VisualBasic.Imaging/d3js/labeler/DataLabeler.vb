Imports System.Drawing
Imports Microsoft.VisualBasic.Linq

Namespace d3js.Layout

    Public MustInherit Class DataLabeler : Implements IEnumerable(Of Label)

        Protected m_labels As Label()
        Protected m_anchors As Anchor()
        Protected unpinnedLabels As Integer()

        ''' <summary>
        ''' box width/height
        ''' </summary>
        Protected CANVAS_WIDTH As Double = 1
        Protected CANVAS_HEIGHT As Double = 1
        Protected offset As PointF

        ''' <summary>
        ''' main simulated annealing function.(这个函数运行完成之后，可以直接使用<see cref="Label.X"/>和<see cref="Label.Y"/>位置数据进行作图)
        ''' </summary>
        ''' <param name="nsweeps"></param>
        ''' <returns></returns>
        Public MustOverride Function Start(Optional nsweeps% = 2000, Optional showProgress As Boolean = True) As DataLabeler

        Public Function WithOffset(offset As PointF) As DataLabeler
            Me.offset = offset
            Return Me
        End Function

        ''' <summary>
        ''' users insert graph width
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overridable Function Width(x#) As DataLabeler
            CANVAS_WIDTH = x
            Return Me
        End Function

        ''' <summary>
        ''' users insert graph height
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overridable Function Height(x#) As DataLabeler
            CANVAS_HEIGHT = x
            Return Me
        End Function

        Public Function Size(x As SizeF) As DataLabeler
            With x
                CANVAS_WIDTH = .Width
                CANVAS_HEIGHT = .Height
            End With

            Return Me
        End Function

        ''' <summary>
        ''' users insert label positions
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function Labels(x As IEnumerable(Of Label)) As DataLabeler
            m_labels = x.ToArray
            unpinnedLabels = m_labels _
                .SeqIterator _
                .Where(Function(l) Not l.value.pinned) _
                .Select(Function(lb) lb.i) _
                .ToArray

            Return Me
        End Function

        ''' <summary>
        ''' users insert anchor positions
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function Anchors(x As IEnumerable(Of Anchor)) As DataLabeler
            m_anchors = x.ToArray
            Return Me
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Label) Implements IEnumerable(Of Label).GetEnumerator
            For Each label As Label In m_labels
                Yield label
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
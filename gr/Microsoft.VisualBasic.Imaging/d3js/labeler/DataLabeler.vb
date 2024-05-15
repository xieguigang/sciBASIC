#Region "Microsoft.VisualBasic::b8efab0111e9deb4c260437e4fd6adb1, gr\Microsoft.VisualBasic.Imaging\d3js\labeler\DataLabeler.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 106
    '    Code Lines: 53
    ' Comment Lines: 37
    '   Blank Lines: 16
    '     File Size: 3.52 KB


    '     Class DataLabeler
    ' 
    '         Function: Anchors, GetEnumerator, Height, IEnumerable_GetEnumerator, Labels
    '                   Size, Width, WithOffset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Linq

Namespace d3js.Layout

    Public MustInherit Class DataLabeler : Implements IEnumerable(Of Label)

        Protected m_labels As Label()
        Protected m_anchors As Anchor()

        ''' <summary>
        ''' the index of the labels which is unpinned
        ''' (can move on the canvas) in the array of 
        ''' <see cref="m_labels"/>, this index value 
        ''' can also used for read anchor object from 
        ''' the <see cref="m_anchors"/>.
        ''' </summary>
        Protected unpinnedLabels As Integer()

        ''' <summary>
        ''' box width/height
        ''' </summary>
        Protected CANVAS_WIDTH As Double = 1
        Protected CANVAS_HEIGHT As Double = 1
        Protected offset As PointF

        ''' <summary>
        ''' main simulated annealing function.
        ''' (这个函数运行完成之后，可以直接使用<see cref="Label.X"/>和
        ''' <see cref="Label.Y"/>位置数据进行作图)
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

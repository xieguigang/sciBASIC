#Region "Microsoft.VisualBasic::c08a0caf0eaacc4c4b8c528500bbe8e1, gr\network-visualization\Datavisualization.Network\Graph\Model\Handle\Handle.vb"

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

    '   Total Lines: 150
    '    Code Lines: 83 (55.33%)
    ' Comment Lines: 45 (30.00%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 22 (14.67%)
    '     File Size: 5.20 KB


    '     Class Handle
    ' 
    '         Properties: isDirectPoint, originalLocation
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: convert, getSerializableString, ParseHandle, ParseHandles, pointAuto
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graph.EdgeBundling

    ' 20200610
    ' 在计算三角函数的时候似乎误差非常大

    ''' <summary>
    ''' 进行网络之中的边连接的布局走向的``拐点``的矢量化描述
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/cytoscape/cytoscape-impl/blob/93530ef3b35511d9b1fe0d0eb913ecdcd3b456a8/ding-impl/ding-presentation-impl/src/main/java/org/cytoscape/ding/impl/HandleImpl.java#L247
    ''' </remarks>
    Public Class Handle

        Friend cosTheta# = Double.NaN
        Friend sinTheta# = Double.NaN
        Friend ratio# = Double.NaN

        ' Original handle location
        Friend x# = 0
        Friend y# = 0

        Const DELIMITER As Char = ","c

        Public ReadOnly Property isDirectPoint As Boolean
            Get
                Return cosTheta.IsNaNImaginary AndAlso sinTheta.IsNaNImaginary AndAlso ratio.IsNaNImaginary
            End Get
        End Property

        ''' <summary>
        ''' 当前的这个位置是一个绝对位置，并非矢量描述位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property originalLocation As PointF
            Get
                Return New PointF(x, y)
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(location As PointF)
            x = location.X
            y = location.Y
        End Sub

        ''' <summary>
        ''' Rotate And scale the vector to the handle position
        ''' </summary>
        ''' <param name="sX">x of source node</param>
        ''' <param name="sY">y of source node</param>
        ''' <param name="tX">x of target node</param>
        ''' <param name="tY">y of target node</param>
        ''' <returns></returns>
        Public Function pointAuto(sX As Double, sY As Double, tX As Double, tY As Double) As PointF
            If isDirectPoint Then
                Return originalLocation
            Else
                Return convert(tX, tY, sX, sY)
            End If
        End Function

        ''' <summary>
        ''' Rotate And scale the vector to the handle position
        ''' </summary>
        ''' <param name="sX">x of source node</param>
        ''' <param name="sY">y of source node</param>
        ''' <param name="tX">x of target node</param>
        ''' <param name="tY">y of target node</param>
        ''' <returns></returns>
        Public Function convert(sX As Double, sY As Double, tX As Double, tY As Double) As PointF
            ' Original edge vector v = (vx, vy). (from source to target)
            Dim vx = tX - sX
            Dim vy = tY - sY

            ' rotate
            Dim newX = vx * cosTheta - vy * sinTheta
            Dim newY = vx * sinTheta + vy * cosTheta

            ' New rotated vector v' = (newX, newY).
            ' Resize vector
            newX = newX * ratio
            newY = newY * ratio

            ' ... And this Is the New handle position.
            Dim handleX = newX + sX
            Dim handleY = newY + sY

            Dim newPoint As New PointF(handleX, handleY)

            Return newPoint
        End Function

        ''' <summary>
        ''' Serialized string Is "cos,sin,ratio".
        ''' </summary>
        ''' <returns></returns>
        Public Function getSerializableString() As String
            Return cosTheta & DELIMITER & sinTheta & DELIMITER & ratio
        End Function

        Public Overrides Function ToString() As String
            Return New Dictionary(Of String, Double) From {
                {NameOf(cosTheta), cosTheta},
                {NameOf(sinTheta), sinTheta},
                {NameOf(ratio), ratio}
            }.GetJson
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="strRepresentation">
        ''' String join of <see cref="getSerializableString()"/> between handles with delimiter ``|``.
        ''' </param>
        ''' <returns></returns>
        Public Shared Function ParseHandles(strRepresentation As String) As IEnumerable(Of Handle)
            If strRepresentation.StringEmpty Then
                Return {}
            Else
                Return strRepresentation _
                    .Split("|"c) _
                    .Select(AddressOf ParseHandle)
            End If
        End Function

        Private Shared Function ParseHandle(str As String) As Handle
            Dim parts As Double() = str _
                .Split(DELIMITER) _
                .Select(AddressOf Double.Parse) _
                .ToArray

            If parts.Length = 2 Then
                Return New Handle With {.x = parts(0), .y = parts(1)}
            ElseIf parts.Length = 3 Then
                Return New Handle With {
                    .cosTheta = parts(0),
                    .sinTheta = parts(1),
                    .ratio = parts(2)
                }
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace

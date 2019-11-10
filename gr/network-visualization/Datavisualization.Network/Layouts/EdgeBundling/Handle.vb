Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Layouts.EdgeBundling

    ''' <summary>
    ''' 进行网络之中的边连接的布局走向的拐点的矢量化描述
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/cytoscape/cytoscape-impl/blob/93530ef3b35511d9b1fe0d0eb913ecdcd3b456a8/ding-impl/ding-presentation-impl/src/main/java/org/cytoscape/ding/impl/HandleImpl.java#L247
    ''' </remarks>
    Public Class Handle

        Dim cosTheta# = Double.NaN
        Dim sinTheta# = Double.NaN
        Dim ratio# = Double.NaN

        ' Original handle location
        Dim x# = 0
        Dim y# = 0

        Const DELIMITER As Char = ","c

        Public ReadOnly Property isDirectPoint As Boolean
            Get
                Return cosTheta.IsNaNImaginary AndAlso sinTheta.IsNaNImaginary AndAlso ratio.IsNaNImaginary
            End Get
        End Property

        Public ReadOnly Property originalLocation As PointF
            Get
                Return New PointF(x, y)
            End Get
        End Property

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
                    .Select(AddressOf parseHandle)
            End If
        End Function

        Private Shared Function parseHandle(str As String) As Handle
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
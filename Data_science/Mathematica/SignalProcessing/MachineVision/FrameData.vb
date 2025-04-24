Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' A frame data of the <see cref="Detection"/>.
''' </summary>
Public Class FrameData : Inherits FrameData(Of Detection)
End Class

Public Class FrameData(Of T As Detection) : Implements Enumeration(Of T)

    <XmlAttribute>
    Public Property FrameID As Integer

    <XmlElement("Objects")>
    Public Property Detections As T()

    Sub New()
    End Sub

    Sub New(id As Integer, detections As IEnumerable(Of Detection))
        _FrameID = id
        _Detections = detections.SafeQuery.ToArray
    End Sub

    Public Function SetIndex(id As Integer) As FrameData(Of T)
        FrameID = id
        Return Me
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"#{FrameID} " & Detections _
            .SafeQuery _
            .Select(Function(a) a.ObjectID) _
            .GetJson
    End Function

    Public Iterator Function GenericEnumerator() As IEnumerator(Of T) Implements Enumeration(Of T).GenericEnumerator
        For Each obj As T In Detections.SafeQuery
            Yield obj
        Next
    End Function
End Class

Public Class Detection

    <XmlAttribute>
    Public Property ObjectID As String
    Public Property Position As PointF

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"{ObjectID} [x:{Position.X}, y:{Position.Y}]"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(obj As Detection) As PointF
        Return obj.Position
    End Operator
End Class


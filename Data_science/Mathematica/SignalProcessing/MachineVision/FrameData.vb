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

    ''' <summary>
    ''' get detection object by its index in current frame
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Default Public ReadOnly Property Item(index As Integer) As T
        Get
            Return Detections(index)
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(id As Integer, detections As IEnumerable(Of Detection))
        _FrameID = id
        _Detections = detections.SafeQuery.ToArray
    End Sub

    ''' <summary>
    ''' Just set the given index value to the <see cref="FrameID"/>
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    <DebuggerStepThrough>
    Public Function SetIndex(id As Integer) As FrameData(Of T)
        FrameID = id

        For Each obj As T In Detections.SafeQuery
            obj.FrameID = id
        Next

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
    Public Property FrameID As Integer

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"{ObjectID} [x:{Position.X}, y:{Position.Y}]"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(obj As Detection) As PointF
        Return obj.Position
    End Operator
End Class


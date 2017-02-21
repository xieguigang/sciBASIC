
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Data

    Public Structure Vector

        ''' <summary>
        ''' 2017-1-30
        ''' 因为在进行XML存储的时候，可能会由于需要调整格式对齐数字，所以在这里使用字符串来存储数据，
        ''' 否则直接使用数组会因为格式的变化而无法被读取
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property Point3D As String

        Public ReadOnly Property PointData As Point3D
            Get
                Dim v As Single() = Me.Point3D _
                    .Split() _
                    .Where(Function(t) Not t.StringEmpty) _
                    .ToArray(Function(s) CSng(s))
                Return New Point3D(v(Scan0), v(1), v(2))
            End Get
        End Property

        Sub New(pt As Point3D)
            With pt
                Point3D = { .X, .Y, .Z}.JoinBy(" ")
            End With
        End Sub

        Public Overrides Function ToString() As String
            Return PointData.GetJson
        End Function
    End Structure
End Namespace
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

''' <summary>
''' Physics world reactor
''' </summary>
Public Class World

    Public Enum Type As Byte
        Plain2D = 2
        Spatial3D = 3
    End Enum

    Public Delegate Function Reaction(m1 As MassPoint, m2 As MassPoint) As Force
    Public Delegate Sub Output(objs As IEnumerable(Of MassPoint))

    Dim objects As List(Of MassPoint)
    Dim reactions As New Dictionary(Of String, Dictionary(Of String, NamedValue(Of Reaction)))
    Dim forceSystem As Dictionary(Of String, List(Of Force))
    Dim outputs As Output

    Protected Overridable Sub RaiseEvents()
        If Not outputs Is Nothing Then
            Call outputs(objects)
        End If
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="time%">迭代的次数</param>
    Public Sub React(time As UInteger)
        For i As UInteger = 0 To time
            Call React()
            Call RaiseEvents()
        Next
    End Sub

    Private Sub React()
        For Each F In forceSystem.Values
            Call F.Clear()
        Next

        For Each a As MassPoint In objects.Where(Function(m) Me.reactions.ContainsKey(m.ID))
            Dim reactions = Me.reactions(a.ID)
            Dim forces As List(Of Force) = forceSystem(a.ID)

            For Each b As MassPoint In objects.Where(Function(m) reactions.ContainsKey(m.ID))
                Dim reaction As NamedValue(Of Reaction) = reactions(b.ID)

                With reaction.Value(a, b)
                    .source = reaction.Name
                    Call forces.Add(.ref)
                    Call forceSystem(b.ID).Add(- .ref)
                End With
            Next
        Next

        For Each m As MassPoint In objects
            m += forceSystem(m.ID).Sum  ' 力影响了物体的加速度
            m.Displacement()    ' 加速度改变了物体现有的运动状态
        Next
    End Sub
End Class

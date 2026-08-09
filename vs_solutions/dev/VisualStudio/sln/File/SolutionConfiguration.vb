Namespace sln.File

    ''' <summary>
    ''' A solution level build configuration / platform pair, e.g. ``Debug|AnyCPU``.
    ''' </summary>
    Public Class SolutionConfiguration
        ''' <summary>
        ''' The combined name, e.g. ``Debug|AnyCPU``.
        ''' </summary>
        Public Property Name As String
        ''' <summary>
        ''' The configuration part, e.g. ``Debug``.
        ''' </summary>
        Public Property Configuration As String
        ''' <summary>
        ''' The platform part, e.g. ``AnyCPU``.
        ''' </summary>
        Public Property Platform As String

        Public Sub New()
        End Sub

        Public Sub New(name As String)
            Me.Name = name

            If name IsNot Nothing Then
                Dim parts = name.Split({"|"c}, 2)
                Configuration = parts(0)

                If parts.Length > 1 Then
                    Platform = parts(1)
                End If
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Configuration}/{Platform}"
        End Function
    End Class
End Namespace
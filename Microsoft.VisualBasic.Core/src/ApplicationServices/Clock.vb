Namespace ApplicationServices

    Public Class Clock

        Private Shared INSTANCEField As Clock

        Private milliseconds As Long

        Private m_running As Boolean

        Private startTime As Long

        Private stopTime As Long

        Public Shared ReadOnly Property Instance As Clock
            Get
                If INSTANCEField Is Nothing Then
                    INSTANCEField = New Clock()
                End If
                Return INSTANCEField
            End Get
        End Property

        Public Sub New()
            milliseconds = 0
            stopTime = CurrentUnixTimeMillis
            m_running = False
        End Sub

        Public Overridable Sub start()
            m_running = True
            startTime = CurrentUnixTimeMillis
        End Sub

        Public Overridable Sub [stop]()
            milliseconds = 0
            m_running = False
            stopTime = CurrentUnixTimeMillis
        End Sub

        Public Overridable Sub pause()
            m_running = False
            stopTime = CurrentUnixTimeMillis
            milliseconds += stopTime - startTime
        End Sub

        Public Overridable Sub reset()
            [stop]()
            start()
        End Sub

        Public Overridable Function elapsedMillis() As Long
            If m_running Then
                Return milliseconds + CurrentUnixTimeMillis - startTime
            Else
                Return milliseconds
            End If
        End Function

        Public Overridable Function systemElapsedMillis() As Long
            Return CurrentUnixTimeMillis
        End Function

        Public Overridable Function elapsedSeconds() As Long
            Return elapsedMillis() / 1000
        End Function

        Public Overridable Function running() As Boolean
            Return m_running
        End Function

    End Class

End Namespace

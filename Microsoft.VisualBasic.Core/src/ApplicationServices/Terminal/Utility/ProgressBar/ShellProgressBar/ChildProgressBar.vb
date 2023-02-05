Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar

    Public Class ChildProgressBar : Inherits ProgressBarBase
        Implements IDisposable

        Private ReadOnly _scheduleDraw As Action
        Private ReadOnly _writeLine As Action(Of String)
        Private ReadOnly _writeError As Action(Of String)
        Private ReadOnly _growth As Action(Of ProgressBarHeight)

        Public ReadOnly Property StartDate As Date
            Get
                Return MyBase._startDate
            End Get
        End Property

        Protected Overrides Sub DisplayProgress()
            _scheduleDraw?.Invoke()
        End Sub

        Friend Sub New(maxTicks As Integer, message As String,
                       scheduleDraw As Action,
                       writeLine As Action(Of String),
                       writeError As Action(Of String),
                       Optional options As ProgressBarOptions = Nothing,
                       Optional growth As Action(Of ProgressBarHeight) = Nothing)

            MyBase.New(maxTicks, message, options)

            _scheduleDraw = scheduleDraw
            _writeLine = writeLine
            _writeError = writeError
            _growth = growth
            _growth?.Invoke(ProgressBarHeight.Increment)
        End Sub

        Protected Overrides Sub Grow(direction As ProgressBarHeight)
            _growth?.Invoke(direction)
        End Sub

        Private _calledDone As Boolean
        Private ReadOnly _callOnce As Object = New Object()
        Protected Overrides Sub OnDone()
            If _calledDone Then Return
            SyncLock _callOnce
                If _calledDone Then Return

                If EndTime Is Nothing Then EndTime = Date.Now

                If Options.CollapseWhenFinished Then _growth?.Invoke(ProgressBarHeight.Decrement)

                _calledDone = True
            End SyncLock
        End Sub

        Public Overrides Sub WriteLine(message As String) 'Implements IProgressBar.WriteLine
            _writeLine(message)
        End Sub
        Public Overrides Sub WriteErrorLine(message As String) 'Implements IProgressBar.WriteErrorLine
            _writeError(message)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            For Each c In Children
                c.Dispose()
            Next
            OnDone()
        End Sub

        Public Function AsProgress(Of T)(Optional message As Func(Of T, String) = Nothing, Optional percentage As Func(Of T, Double?) = Nothing) As IProgress(Of T) 'Implements IProgressBar.AsProgress
            Return New Progress(Of T)(Me, message, percentage)
        End Function
    End Class
End Namespace

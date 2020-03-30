Imports System.Drawing

Public Class Debugger(Of T As Individual) : Implements IDisposable

    Dim cache As List(Of Integer)()()
    Dim getInt As ToInteger(Of T)
    Dim file As String
    Dim simulator As Simulator(Of T)
    Dim size As Size

    Sub New(file As String, simulator As Simulator(Of T), getInt As ToInteger(Of T))
        Me.size = simulator.size
        Me.file = file
        Me.getInt = getInt
        Me.simulator = simulator
        Me.cache = MAT(Of List(Of Integer))(size.Height, size.Width)

        For j As Integer = 0 To size.Width - 1
            For i As Integer = 0 To size.Height - 1
                cache(i)(j) = New List(Of Integer)
            Next
        Next
    End Sub

    Public Sub TakeSnapshots()
        For j As Integer = 0 To size.Width - 1
            For i As Integer = 0 To size.Height - 1
                cache(i)(j).Add(getInt(simulator.grid(i)(j).data))
            Next
        Next
    End Sub

    Public Overrides Function ToString() As String
        Return file
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call WriteCDF.Flush(file, cache, GetType(T))
                Call cache.Free
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

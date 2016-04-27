Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

Namespace Imaging

    Public Class Wmf : Implements System.IDisposable

        ReadOnly curMetafile As Metafile
        ReadOnly gSource As Graphics
        ReadOnly gDrawing As Graphics
        ReadOnly hdc As IntPtr

        Public ReadOnly Property Graphics As Graphics
            Get
                Return gDrawing
            End Get
        End Property

        Public ReadOnly Property Size As Size
        Public ReadOnly Property FilePath As String

        Sub New(size As Size, Save As String)
            Dim bitmap As New Bitmap(size.Width, size.Height)
            gSource = Graphics.FromImage(bitmap)
            hdc = gSource.GetHdc()
            curMetafile = New Metafile(Save, hdc)
            gDrawing = Graphics.FromImage(curMetafile)
            gDrawing.SmoothingMode = SmoothingMode.HighQuality

            Me.Size = size
            Me.FilePath = Save
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call gSource.ReleaseHdc(hdc)
                    Call gDrawing.Dispose()
                    Call gSource.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
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
End Namespace
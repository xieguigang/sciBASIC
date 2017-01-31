Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace Drawing3D.Device

    ''' <summary>
    ''' 三维图形设备的工作线程管理器
    ''' </summary>
    Public Class Worker
        Implements IDisposable
        Implements IObjectModel_Driver

        Public Delegate Function ModelData() As IEnumerable(Of Surface)

        Dim buffer As IEnumerable(Of Polygon)
        Public Property model As ModelData
        Dim camera As Func(Of Camera)
        Dim WithEvents display As GDIDevice
        Dim spaceThread As New UpdateThread(10, AddressOf CreateBuffer)

        Public Property drawPath As Boolean

        Sub New(dev As GDIDevice, camera As Func(Of Camera))
            Me.camera = camera
            Me.display = dev
        End Sub

        Sub CreateBuffer()
            buffer = camera().PainterBuffer(model())
        End Sub

        Private Sub display_Paint(sender As Object, e As PaintEventArgs) Handles display.Paint
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear

            If Not buffer Is Nothing Then
                Call e.Graphics.BufferPainting(buffer, drawPath)
            End If
        End Sub

        Public Function Run() As Integer Implements IObjectModel_Driver.Run
            Return spaceThread.Start
        End Function

        Public Sub Pause()
            Call spaceThread.Stop()
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call spaceThread.Dispose()
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
End Namespace
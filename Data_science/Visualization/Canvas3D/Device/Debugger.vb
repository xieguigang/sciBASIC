#Region "Microsoft.VisualBasic::63c04126283cef81ac29097f36568bbe, sciBASIC#\Data_science\Visualization\Canvas3D\Device\Debugger.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 96
    '    Code Lines: 60
    ' Comment Lines: 17
    '   Blank Lines: 19
    '     File Size: 3.69 KB


    '     Class Debugger
    ' 
    '         Properties: FPS
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: ClearFPSCount, device_MouseMove, (+2 Overloads) Dispose, DrawInformation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace Drawing3D.Device

    Public Class Debugger : Inherits IDevice(Of GDIDevice)
        Implements IDisposable

        Public BufferWorker As Long
        Public RenderingWorker As Long
        Public FPSCount As Integer
        Public ReadOnly Property FPS As Integer

        Public Sub New(dev As GDIDevice)
            MyBase.New(dev)
            Call FPSThread.Start()
        End Sub

        Dim font As New Font(FontFace.Consolas, 9)
        Dim red As SolidBrush = Brushes.Red
        Dim mouse As Point
        Dim WithEvents FPSThread As New UpdateThread(1000, AddressOf ClearFPSCount)

        Private Sub ClearFPSCount()
            _FPS = FPSCount
            FPSCount = 0
        End Sub

        Public Sub DrawInformation(canvas As Graphics)
            Dim top! = 15, left! = 10
            Dim camera As Camera = device._camera
            Dim draw = Sub(msg$)
                           top += 14
                           Call canvas.DrawString(msg, font, red, New PointF(left, top))
                       End Sub

            ' 显示camera的调试信息
            Call draw(msg:=$"Rotation vector:       x={camera.angleX}, y={camera.angleY}, z={camera.angleZ}")
            Call draw(msg:=$"View distance:         {camera.viewDistance}")
            Call draw(msg:=$"FOV:                   {camera.fov}")
            Call draw(msg:=$"Screen size:           {camera.screen.Width}px X {camera.screen.Height}px")

            top += 14

            ' 显示系统的性能
            Call draw(msg:=$"Buffer worker time:    {BufferWorker} (ticks)")
            Call draw(msg:=$"Rendering worker time: {RenderingWorker} (ticks)")
            Call draw(msg:=$"FPS:                   {FPS} (frame/s)")

            top += 14

            ' 显示设备捕捉信息
            Call draw(msg:=$"Mouse device:          ({mouse.X}, {mouse.Y})")

            FPSCount += 1
        End Sub

        Private Sub device_MouseMove(sender As Object, e As MouseEventArgs) Handles device.MouseMove
            mouse = e.Location
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call FPSThread.Stop()
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

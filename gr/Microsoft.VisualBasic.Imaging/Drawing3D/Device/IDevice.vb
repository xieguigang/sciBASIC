#Region "Microsoft.VisualBasic::8f57fa2c96544f95b99df7b9c61cd0a9, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Device\IDevice.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class IDevice
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Delegate Sub
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

Namespace Drawing3D.Device

    Public MustInherit Class IDevice(Of T As UserControl)

        Protected WithEvents device As T

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(dev As T)
            device = dev
        End Sub

        Public Overrides Function ToString() As String
            Return device.ToString
        End Function
    End Class

    ''' <summary>
    ''' 3D plot for <see cref="Gdidevice"/>，由于是需要将图像显示到WinFom控件上面，所以在这里要求的是gdi+的图形驱动程序
    ''' </summary>
    ''' <param name="canvas">gdi+ handle</param>
    ''' <param name="camera">3d camera</param>
    Public Delegate Sub DrawGraphics(ByRef canvas As Graphics, camera As Camera)

End Namespace

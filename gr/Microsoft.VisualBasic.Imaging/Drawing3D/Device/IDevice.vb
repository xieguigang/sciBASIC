Imports System.Drawing

Namespace Drawing3D.Device

    Public MustInherit Class IDevice

        Protected WithEvents device As GDIDevice

        Sub New(dev As GDIDevice)
            device = dev
        End Sub
    End Class

    ''' <summary>
    ''' 3D plot for <see cref="Gdidevice"/>
    ''' </summary>
    ''' <param name="canvas">gdi+ handle</param>
    ''' <param name="camera">3d camera</param>
    Public Delegate Sub DrawGraphics(ByRef canvas As Graphics, camera As Camera)

End Namespace
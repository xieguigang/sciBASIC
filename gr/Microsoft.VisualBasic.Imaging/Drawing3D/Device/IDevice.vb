Imports System.Drawing

Namespace Drawing3D.Device

    Public MustInherit Class IDevice

        Protected WithEvents device As GDIDevice

        Sub New(dev As GDIDevice)
            device = dev
        End Sub
    End Class

    Public Delegate Sub DrawGraphics(ByRef canvas As Graphics, camera As Camera)

End Namespace
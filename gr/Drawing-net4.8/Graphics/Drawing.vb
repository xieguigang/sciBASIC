Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

Public Module Drawing

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function OpenGraphicsDevice(size As Size, dpi As Integer) As IGraphics
        Return Extensions.CreateGDIDevice(size.Width, size.Height, dpi:=$"{dpi},{dpi}")
    End Function
End Module

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text

Namespace Driver

    Public MustInherit Class MockGDIPlusGraphics : Inherits IGraphics

        ''' <summary>
        ''' 主要是需要进行字体的大小计算所需要使用的一个内部gdi+对象
        ''' </summary>
        ReadOnly gdi As Graphics = Graphics.FromImage(New Bitmap(10, 10))

        Public Overrides ReadOnly Property Size As Size
        Public Overrides ReadOnly Property DpiX As Single
        Public Overrides ReadOnly Property DpiY As Single

        Sub New(size As Size, dpi As Size)
            Me.DpiX = dpi.Width
            Me.DpiY = dpi.Height
            Me.Size = size
        End Sub

        Sub New(size As Size, dpiX As Single, dpiY As Single)
            Me.DpiX = dpiX
            Me.DpiY = dpiY
            Me.Size = size
        End Sub

        Private Shared Function FontScale(font As Font) As Font
            Return New Font(font, font.Size * 2)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FontMetrics(font As Font) As FontMetrics
            Return New FontMetrics(font, gdi)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureCharacterRanges(text As String, font As Font, layoutRect As RectangleF, stringFormat As StringFormat) As Region()
            Return gdi.MeasureCharacterRanges(text, font, layoutRect, stringFormat)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font) As SizeF
            Return gdi.MeasureString(text, FontScale(font))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, width As Integer) As SizeF
            Return gdi.MeasureString(text, FontScale(font), width)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF) As SizeF
            Return gdi.MeasureString(text, FontScale(font), layoutArea)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, width As Integer, format As StringFormat) As SizeF
            Return gdi.MeasureString(text, FontScale(font), width, format)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, origin As PointF, stringFormat As StringFormat) As SizeF
            Return gdi.MeasureString(text, FontScale(font), origin, stringFormat)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat) As SizeF
            Return gdi.MeasureString(text, FontScale(font), layoutArea, stringFormat)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function MeasureString(text As String,
                                                font As Font,
                                                layoutArea As SizeF,
                                                stringFormat As StringFormat,
                                                ByRef charactersFitted As Integer,
                                                ByRef linesFilled As Integer) As SizeF

            Return gdi.MeasureString(text, FontScale(font), layoutArea, stringFormat, charactersFitted, linesFilled)
        End Function
    End Class
End Namespace



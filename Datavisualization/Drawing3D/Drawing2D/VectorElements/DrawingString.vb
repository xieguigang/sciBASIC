Imports System.Drawing
Imports Microsoft.VisualBasic.DocumentFormat.HTML
Imports Microsoft.VisualBasic.Imaging

Namespace Drawing2D.VectorElements

    Public Class DrawingString : Inherits VectorObject

        Public Property Text As String
        Public Property Pen As Brush
        Public Property Font As Font

        Sub New(locat As Point, size As Size)
            Call MyBase.New(locat, size)
        End Sub

        Sub New(rect As Rectangle)
            Call MyBase.New(rect)
        End Sub

        Sub New(text As TextString, rect As Rectangle)
            Call Me.New(text.Text, text.Font, rect)
        End Sub

        Sub New(text As String, font As Font, rect As Rectangle)
            Call MyBase.New(rect)

            Me.Text = text
            Me.Font = font
        End Sub

        Public Overrides Function ToString() As String
            Return Text
        End Function

        Public Overrides Sub Draw(gdi As GDIPlusDeviceHandle)
            Call gdi.DrawString(Text, Font, Pen, New RectangleF(RECT.X, RECT.Y, RECT.Width, RECT.Height))
        End Sub
    End Class

    ''' <summary>
    ''' 基于HTML语法的字符串的绘制描述信息的解析
    ''' </summary>
    Public Module TextAPI

        ' html -->  <font face="Microsoft YaHei" size="1.5"><strong>text</strong><b><i>value</i></b></font> 
        ' 解析上述的表达式会产生一个栈，根据html标记来赋值字符串的gdi+属性

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="html">这里只是一个很小的html的片段，仅仅用来描述所需要进行绘制的字符串的gdi+属性</param>
        ''' <returns></returns>
        Public Function GetStrings(html As String) As DrawingString()

        End Function
    End Module
End Namespace
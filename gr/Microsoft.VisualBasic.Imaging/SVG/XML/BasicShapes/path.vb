Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.SVG.PathHelper

Namespace SVG.XML

    ''' <summary>
    ''' ``&lt;path>`` 标签用来定义路径。
    ''' </summary>
    Public Class path : Inherits node

        ''' <summary>
        ''' 下面的命令可用于路径数据：
        ''' 
        ''' M = moveto
        ''' L = lineto
        ''' H = horizontal lineto
        ''' V = vertical lineto
        ''' C = curveto
        ''' S = smooth curveto
        ''' Q = quadratic Belzier curve
        ''' T = smooth quadratic Belzier curveto
        ''' A = elliptical Arc
        ''' Z = closepath
        ''' 
        ''' 注释：以上所有命令均允许小写字母。大写表示绝对定位，小写表示相对定位。
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property d As String

        Sub New()
        End Sub

        Sub New(path As GraphicsPath)
            d = path.SVGPathData
        End Sub

        Public Shared Operator +(path As path, offset As PointF) As path
            Dim data = path.d.Split
            path = DirectCast(path.MemberwiseClone, path)

            ' 这里该如何进行偏移？

            Return path
        End Operator
    End Class

End Namespace
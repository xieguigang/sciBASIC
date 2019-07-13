#Region "Microsoft.VisualBasic::591fb035bc3894a0001c247c9a1de53a, Data_science\Visualization\Plots\g\Legends\Legend.vb"

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

    '     Class Legend
    ' 
    '         Properties: color, fontstyle, style, title
    ' 
    '         Function: GetFont, MeasureTitle, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Graphic.Legend

    ''' <summary>
    ''' 图例
    ''' </summary>
    Public Class Legend

        ''' <summary>
        ''' The shape of this legend
        ''' </summary>
        ''' <returns></returns>
        Public Property style As LegendStyles
        ''' <summary>
        ''' The legend label
        ''' </summary>
        ''' <returns></returns>
        Public Property title As String
        ''' <summary>
        ''' The color of the legend <see cref="style"/> shape.
        ''' </summary>
        ''' <returns></returns>
        Public Property color As String
        ''' <summary>
        ''' CSS expression, which can be parsing by <see cref="CSSFont"/>, drawing font of <see cref="title"/> 
        ''' </summary>
        ''' <returns></returns>
        Public Property fontstyle As String

        ''' <summary>
        ''' <see cref="fontstyle"/> to <see cref="Font"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function GetFont() As Font
            Return CSSFont.TryParse(fontstyle).GDIObject
        End Function

        ''' <summary>
        ''' 计算标题文本的绘制大小
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function MeasureTitle(g As IGraphics) As SizeF
            Return g.MeasureString(title, GetFont)
        End Function

        Public Overrides Function ToString() As String
            Return title
        End Function
    End Class
End Namespace

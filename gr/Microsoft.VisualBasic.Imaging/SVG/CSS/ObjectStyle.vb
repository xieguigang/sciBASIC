#Region "Microsoft.VisualBasic::97e9e4b3d2e231fd1587178c531ef4b1, gr\Microsoft.VisualBasic.Imaging\SVG\CSS\ObjectStyle.vb"

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

    '     Class ObjectStyle
    ' 
    '         Properties: CSSValue, fill, stroke
    ' 
    '         Function: ToString
    ' 
    '     Class CSSStyles
    ' 
    '         Properties: filters, linearGradients, radialGradients, styles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta

Namespace SVG.CSS

    Public Class ObjectStyle : Inherits ICSSValue

        Public Property stroke As Stroke
        Public Property fill As String

        Public Overrides ReadOnly Property CSSValue As String
            Get
                Return ToString()
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return stroke.ToString & " fill: " & fill
        End Function
    End Class

    ''' <summary>
    ''' 在这个SVG对象之中所定义的CSS样式数据
    ''' </summary>
    Public Class CSSStyles : Inherits Node

        <XmlElement("linearGradient")>
        Public Property linearGradients As linearGradient()
        <XmlElement("radialGradient")>
        Public Property radialGradients As radialGradient()
        <XmlElement("style")>
        Public Property styles As XmlMeta.CSS()
        <XmlElement("filter")>
        Public Property filters As Filter()

    End Class
End Namespace

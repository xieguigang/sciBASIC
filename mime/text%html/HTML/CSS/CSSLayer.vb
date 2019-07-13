#Region "Microsoft.VisualBasic::b4bd9e31aeda91e6f57bb85044c80423, mime\text%html\HTML\CSS\CSSLayer.vb"

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

    '     Interface CSSLayer
    ' 
    '         Properties: zIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace HTML.CSS

    ''' <summary>
    ''' 进行样式渲染的图层对象
    ''' </summary>
    Public Interface CSSLayer

        ''' <summary>
        ''' Drawing order, if this index value is greater, then it will be draw on the top most.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("z-index")>
        Property zIndex As Integer

    End Interface
End Namespace

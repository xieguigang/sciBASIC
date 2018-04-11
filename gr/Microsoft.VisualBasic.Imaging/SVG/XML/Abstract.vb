Imports System.Runtime.CompilerServices
Imports System.Xml
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON
Imports htmlNode = Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta.Node

Namespace SVG.XML

    ''' <summary>
    ''' The basically SVG XML document node, it can be tweaks on the style by using CSS
    ''' </summary>
    Public MustInherit Class node : Inherits htmlNode
        Implements CSSLayer, IAddressOf

        <XmlAttribute> Public Property fill As String
        <XmlAttribute> Public Property stroke As String

        ''' <summary>
        ''' Css layer index, this will controls the rendering order of the graphics layer.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("z-index")>
        Public Property zIndex As Integer Implements CSSLayer.zIndex, IAddressOf.Address

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 暂时未找到动态属性的解决方法，暂时忽略掉
        ''' </remarks>
        <XmlIgnore>
        Public Property attributes As Dictionary(Of String, String)

        ''' <summary>
        ''' 对当前的文档节点/图层信息的注释
        ''' </summary>
        Public XmlCommentValue$

        ''' <summary>
        ''' Read Only
        ''' </summary>
        ''' <returns></returns>
        <XmlAnyElement("gComment")> Public Property XmlComment As XmlComment
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return XmlCommentValue.CreateComment()
            End Get
            Set
            End Set
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
            zIndex = address
        End Sub

        Public Overrides Function ToString() As String
            Return MyClass.GetJson
        End Function
    End Class
End Namespace
Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors.OfficeAccent

    Public Structure ObjectColor
        Public Property sysClr As sysClr
    End Structure

    Public Structure Accent
        Public Property srgbClr As srgbClr
    End Structure

    Public Structure sysClr
        <XmlAttribute> Public Property val As String
        <XmlAttribute> Public Property lastClr As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    ''' <summary>
    ''' 颜色值
    ''' </summary>
    Public Structure srgbClr

        ''' <summary>
        ''' 颜色值
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property val As String

        Public ReadOnly Property Color As Color
            Get
                Return ColorTranslator.FromHtml("#" & val)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return val
        End Function
    End Structure
End Namespace
#Region "Microsoft.VisualBasic::062fd1e60ece0c42709904006ee49625, mime\application%xml\xsd\XML.vb"

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

    '     Class restriction
    ' 
    '         Properties: base, enumeration, length, maxInclusive, minInclusive
    '                     pattern, whiteSpace
    ' 
    '     Class simpleType
    ' 
    '         Properties: name, restriction
    ' 
    '     Class restrictionValue
    ' 
    '         Properties: value
    ' 
    '     Class include
    ' 
    '         Properties: schemaLocation
    ' 
    '         Function: ToString
    ' 
    '     Class complexType
    ' 
    '         Properties: attribute, name, sequence
    ' 
    '     Class extension
    ' 
    '         Properties: base
    ' 
    '     Class attribute
    ' 
    '         Properties: annotation, name, type, use
    ' 
    '         Function: ToString
    ' 
    '     Class sequence
    ' 
    '         Properties: element
    ' 
    '         Function: ToString
    ' 
    '     Class element
    ' 
    '         Properties: [default], annotation, fixed, maxOccurs, minOccurs
    '                     name, type
    ' 
    '         Function: ToString
    ' 
    '     Class annotation
    ' 
    '         Properties: documentation
    ' 
    '         Function: ToString
    ' 
    '     Class schema
    ' 
    '         Properties: elementFormDefault, targetNamespace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace xsd

    Public Class restriction

        <XmlAttribute>
        Public Property base As String
        Public Property minInclusive As restrictionValue
        Public Property maxInclusive As restrictionValue

        <XmlElement>
        Public Property enumeration As restrictionValue()
        Public Property pattern As restrictionValue
        Public Property whiteSpace As restrictionValue
        Public Property length As restrictionValue
    End Class

    Public Class simpleType
        <XmlAttribute>
        Public Property name As String
        Public Property restriction As restriction
    End Class

    Public Class restrictionValue

        <XmlAttribute>
        Public Property value As String
    End Class

    Public Class include

        <XmlAttribute>
        Public Property schemaLocation As String

        Public Overrides Function ToString() As String
            Return schemaLocation
        End Function
    End Class

    Public Class complexType

        <XmlAttribute>
        Public Property name As String

        Public Property sequence As sequence
        Public Property attribute As attribute
    End Class

    Public Class extension : Inherits complexType

        <XmlAttribute>
        Public Property base As String
    End Class

    Public Class attribute

        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property use As String

        Public Property annotation As annotation

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type}"
        End Function
    End Class

    Public Class sequence

        <XmlElement>
        Public Property element As element()

        Public Overrides Function ToString() As String
            Return element.ToString
        End Function
    End Class

    Public Class element

        <XmlAttribute> Public Property minOccurs As String
        <XmlAttribute> Public Property maxOccurs As String
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property [default] As String
        <XmlAttribute> Public Property fixed As String

        Public Property annotation As annotation

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type}"
        End Function
    End Class

    Public Class annotation

        Public Property documentation As String

        Public Overrides Function ToString() As String
            Return documentation
        End Function
    End Class

    Public Class schema

        <XmlAttribute>
        Public Property targetNamespace As String
        <XmlAttribute>
        Public Property elementFormDefault As String

    End Class
End Namespace

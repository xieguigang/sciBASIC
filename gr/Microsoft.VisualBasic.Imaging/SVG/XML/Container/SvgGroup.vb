Imports System.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;g> SVG element is a container used to group other SVG elements.
    '''
    ''' Transformations applied To the &lt;g> element are performed On its child elements, 
    ''' And its attributes are inherited by its children. It can also group multiple 
    ''' elements To be referenced later With the &lt;use> element.
    ''' </summary>
    Public NotInheritable Class SvgGroup : Inherits SvgContainer

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgGroup
            Dim element = parent.OwnerDocument.CreateElement("g")
            parent.AppendChild(element)
            Return New SvgGroup(element)
        End Function
    End Class
End Namespace

Imports System.Xml

Namespace SVG.XML

    ''' <summary>
    ''' ## Basic shapes
    ''' 
    ''' There are several basic shapes used for most SVG drawing. The purpose of these shapes 
    ''' is fairly obvious from their names. Some of the parameters that determine their position 
    ''' and size are given, but an element reference would probably contain more accurate and 
    ''' complete descriptions along with other properties that won't be covered in here. 
    ''' However, since they're used in most SVG documents, it's necessary to give them some 
    ''' sort of introduction.
    '''
    ''' To insert a shape, you create an element in the document. Different elements correspond 
    ''' to different shapes And take different parameters to describe the size And position of 
    ''' those shapes. Some are slightly redundant in that they can be created by other shapes, 
    ''' but they're all there for your convenience and to keep your SVG documents as short and 
    ''' as readable as possible. 
    ''' </summary>
    Public MustInherit Class SvgBasicShape : Inherits SvgElement

        Protected Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub
    End Class
End Namespace

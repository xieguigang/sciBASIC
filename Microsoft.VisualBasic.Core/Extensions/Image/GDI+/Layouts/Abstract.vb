#Region "Microsoft.VisualBasic::8e6a85756b8c22b42bc9fa0cfce08b6b, Microsoft.VisualBasic.Core\Extensions\Image\GDI+\Layouts\Abstract.vb"

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

    '     Interface ILayoutedObject
    ' 
    '         Properties: Location
    ' 
    '     Interface ILayoutCoordinate
    ' 
    '         Properties: ID, X, Y
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Language

Namespace Imaging.LayoutModel

    ''' <summary>
    ''' Any typed object with a location layout value
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface ILayoutedObject(Of T)
        Inherits Value(Of T).IValueOf

        Property Location As PointF
    End Interface

    Public Interface ILayoutCoordinate
        Property ID As String
        Property X As Double
        Property Y As Double
    End Interface

End Namespace

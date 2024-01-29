Imports System.Xml.Serialization

Namespace COLLADA

    ''' <summary>
    ''' # 3D Asset Exchange Schema
    ''' 
    ''' COLLADA™ defines an XML-based schema to make it easy to transport 3D assets 
    ''' between applications - enabling diverse 3D authoring and content processing 
    ''' tools to be combined into a production pipeline. The intermediate language 
    ''' provides comprehensive encoding of visual scenes including: geometry, shaders
    ''' and effects, physics, animation, kinematics, and even multiple version representations 
    ''' of the same asset.COLLADA FX enables leading 3D authoring tools to work 
    ''' effectively together to create shader and effects applications and assets to 
    ''' be authored and packaged using OpenGL® Shading Language, Cg, CgFX, and DirectX® 
    ''' FX.
    ''' 
    ''' https://www.khronos.org/collada/
    ''' </summary>
    ''' 
    <XmlType("COLLADA", [Namespace]:="http://www.collada.org/2005/11/COLLADASchema")>
    Public Class COLLADA

        <XmlAttribute> Public Property version As String



    End Class

    Public Class asset

    End Class
End Namespace
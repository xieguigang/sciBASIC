#Region "Microsoft.VisualBasic::0e608f18f2dc1a66404b02f0a57395db, gr\Landscape\COLLADA\COLLADA.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 33
    '    Code Lines: 9 (27.27%)
    ' Comment Lines: 16 (48.48%)
    '    - Xml Docs: 81.25%
    ' 
    '   Blank Lines: 8 (24.24%)
    '     File Size: 1.17 KB


    '     Class COLLADA
    ' 
    '         Properties: version
    ' 
    '     Class asset
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

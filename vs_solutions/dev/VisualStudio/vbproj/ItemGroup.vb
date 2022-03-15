#Region "Microsoft.VisualBasic::fb6e250282baa62911e6aa1ebaf10e83, sciBASIC#\vs_solutions\dev\VisualStudio\vbproj\ItemGroup.vb"

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

    '   Total Lines: 80
    '    Code Lines: 66
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 2.69 KB


    '     Class ItemGroup
    ' 
    '         Properties: [Imports], BootstrapperPackages, Compiles, Contents, EmbeddedResources
    '                     Nones, ProjectReferences, References, TypeScriptCompiles
    ' 
    '         Function: ToString
    ' 
    '     Class ProjectReference
    ' 
    '         Properties: Name, Project
    ' 
    '     Class IncludeItem
    ' 
    '         Properties: [Private], HintPath, Include
    ' 
    '         Function: ToString
    ' 
    '     Class Compile
    ' 
    '         Properties: AutoGen, DependentUpon, DesignTime, DesignTimeSharedInput, SubType
    ' 
    '     Class None
    ' 
    '         Properties: CustomToolNamespace, Generator, LastGenOutput
    ' 
    '     Class EmbeddedResource
    ' 
    '         Properties: CustomToolNamespace, DependentUpon, Generator, LastGenOutput, SubType
    ' 
    '     Class BootstrapperPackage
    ' 
    '         Properties: Install, ProductName, Visible
    ' 
    '     Class Content
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace vbproj

    Public Class ItemGroup

        <XmlElement("Reference")>
        Public Property References As IncludeItem()
        <XmlElement("Import")>
        Public Property [Imports] As IncludeItem()
        <XmlElement("Compile")>
        Public Property Compiles As Compile()
        <XmlElement("TypeScriptCompile")>
        Public Property TypeScriptCompiles As Compile()
        <XmlElement("None")>
        Public Property Nones As None()
        <XmlElement("BootstrapperPackage")>
        Public Property BootstrapperPackages As BootstrapperPackage()
        <XmlElement("EmbeddedResource")>
        Public Property EmbeddedResources As EmbeddedResource()
        <XmlElement("Content")>
        Public Property Contents As Content()
        <XmlElement("ProjectReference")>
        Public Property ProjectReferences As ProjectReference()

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class ProjectReference : Inherits IncludeItem
        Public Property Project As String
        Public Property Name As String
    End Class

    Public Class IncludeItem

        <XmlAttribute>
        Public Property Include As String
        Public Property HintPath As String
        Public Property [Private] As String

        Public Overrides Function ToString() As String
            Return Include
        End Function
    End Class

    Public Class Compile : Inherits IncludeItem
        Public Property AutoGen As String
        Public Property DesignTime As String
        Public Property DependentUpon As String
        Public Property DesignTimeSharedInput As String
        Public Property SubType As String
    End Class

    Public Class None : Inherits IncludeItem
        Public Property Generator As String
        Public Property LastGenOutput As String
        Public Property CustomToolNamespace As String
    End Class

    Public Class EmbeddedResource : Inherits IncludeItem
        Public Property DependentUpon As String
        Public Property Generator As String
        Public Property LastGenOutput As String
        Public Property CustomToolNamespace As String
        Public Property SubType As String
    End Class

    Public Class BootstrapperPackage : Inherits IncludeItem
        Public Property Visible As String
        Public Property ProductName As String
        Public Property Install As String
    End Class

    Public Class Content : Inherits IncludeItem

    End Class
End Namespace

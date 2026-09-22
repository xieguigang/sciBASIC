#Region "Microsoft.VisualBasic::bc64882fd65f3082d5a85aba2b0c924e, vs_solutions\dev\VisualStudio\VBProject\Project\ProjectXml.vb"

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

    '   Total Lines: 117
    '    Code Lines: 64 (54.70%)
    ' Comment Lines: 31 (26.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 22 (18.80%)
    '     File Size: 4.51 KB


    '     Class VBProjectReference
    ' 
    '         Properties: [Include], [Private], Aliases, Condition, ReferenceOutputAssembly
    '                     SetTargetFramework
    ' 
    '     Class VBReference
    ' 
    '         Properties: [Include], [Private], Condition, HintPath
    ' 
    '     Class VBPackageReference
    ' 
    '         Properties: Condition, ExcludeAssets, Id, IncludeAssets, PrivateAssets
    '                     Version
    ' 
    '     Class VBBuildConfiguration
    ' 
    '         Properties: Condition, Configuration, DebugType, DefineConstants, Extra
    '                     Optimize, OutputPath, Platform
    ' 
    '     Class VBProjectMetadata
    ' 
    '         Properties: AssemblyOriginatorKeyFile, EnableDefaultCompileItems, ImplicitUsings, LangVersion, Nullable
    '                     Other, Platforms, SignAssembly, TargetFramework, TargetFrameworks
    ' 
    '     Class VBNuGetMetadata
    ' 
    '         Properties: Authors, Company, Copyright, Description, GeneratePackageOnBuild
    '                     Other, PackageId, PackageLicenseExpression, PackageProjectUrl, PackageReadmeFile
    '                     PackageTags, Product, RepositoryType, RepositoryUrl, RestoreSources
    '                     Version
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace VBProj.ProjectXml

    ''' <summary>
    ''' external project reference (ProjectReference item)
    ''' </summary>
    Public Class VBProjectReference

        ''' <summary>relative or absolute path to the referenced vbproj</summary>
        Public Property [Include] As String
        Public Property Condition As String
        Public Property Aliases As String
        Public Property [Private] As String
        Public Property SetTargetFramework As String
        Public Property ReferenceOutputAssembly As String

    End Class

    ''' <summary>
    ''' external assembly reference: a <c>Reference</c> item carrying a <c>HintPath</c>.
    ''' </summary>
    Public Class VBReference

        ''' <summary>assembly simple name (the Include attribute)</summary>
        Public Property [Include] As String

        ''' <summary>path of the referenced dll (relative to the project file or absolute)</summary>
        Public Property HintPath As String

        ''' <summary>whether the referenced assembly should be copied into the output folder</summary>
        Public Property [Private] As String

        ''' <summary>optional Condition attribute</summary>
        Public Property Condition As String

    End Class

    ''' <summary>
    ''' nuget package reference (PackageReference item)
    ''' </summary>
    Public Class VBPackageReference

        ''' <summary>package id (the Include attribute)</summary>
        Public Property Id As String
        Public Property Version As String
        Public Property Condition As String
        Public Property IncludeAssets As String
        Public Property ExcludeAssets As String
        Public Property PrivateAssets As String

    End Class

    ''' <summary>
    ''' a single build configuration derived from a conditional PropertyGroup
    ''' (e.g. '$(Configuration)|$(Platform)' == 'Debug|AnyCPU').
    ''' </summary>
    Public Class VBBuildConfiguration

        ''' <summary>the raw Condition string of the source PropertyGroup</summary>
        Public Property Condition As String
        ''' <summary>Debug / Release / ...</summary>
        Public Property Configuration As String
        ''' <summary>AnyCPU / x64 / ...</summary>
        Public Property Platform As String
        Public Property DefineConstants As String
        Public Property Optimize As Boolean
        Public Property DebugType As String
        Public Property OutputPath As String
        ''' <summary>any other properties declared inside the conditional PropertyGroup</summary>
        Public Property Extra As Dictionary(Of String, String)

    End Class

    ''' <summary>
    ''' generic, non-packaging project metadata parsed from PropertyGroup elements.
    ''' </summary>
    Public Class VBProjectMetadata

        Public Property TargetFramework As String
        Public Property TargetFrameworks As String
        Public Property Platforms As String
        Public Property Nullable As String
        Public Property LangVersion As String
        Public Property ImplicitUsings As String
        Public Property EnableDefaultCompileItems As String
        Public Property SignAssembly As String
        Public Property AssemblyOriginatorKeyFile As String
        ''' <summary>any unrecognized project properties</summary>
        Public Property Other As Dictionary(Of String, String)

    End Class

    ''' <summary>
    ''' nuget / packaging metadata parsed from PropertyGroup elements.
    ''' </summary>
    Public Class VBNuGetMetadata

        Public Property PackageId As String
        Public Property Version As String
        Public Property Authors As String
        Public Property Company As String
        Public Property Product As String
        Public Property Description As String
        Public Property Copyright As String
        Public Property PackageLicenseExpression As String
        Public Property PackageProjectUrl As String
        Public Property PackageTags As String
        Public Property PackageReadmeFile As String
        Public Property RepositoryUrl As String
        Public Property RepositoryType As String
        Public Property GeneratePackageOnBuild As String
        Public Property RestoreSources As String
        ''' <summary>any other unrecognized packaging properties</summary>
        Public Property Other As Dictionary(Of String, String)

    End Class

End Namespace

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
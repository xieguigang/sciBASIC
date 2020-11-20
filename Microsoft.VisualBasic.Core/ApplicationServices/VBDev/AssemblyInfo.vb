Imports System.Xml.Serialization

Namespace ApplicationServices.Development

    ''' <summary>
    ''' ``My Project\AssemblyInfo.vb``
    ''' </summary>
    Public Class AssemblyInfo

        ' General Information about an assembly is controlled through the following
        ' set of attributes. Change these attribute values to modify the information
        ' associated with an assembly.

        ' Review the values of the assembly attributes

        Public Property AssemblyTitle As String
        Public Property AssemblyDescription As String
        Public Property AssemblyCompany As String
        Public Property AssemblyProduct As String
        Public Property AssemblyCopyright As String
        Public Property AssemblyTrademark As String

        Public Property ComVisible As Boolean

        ''' <summary>
        ''' The following GUID is for the ID of the typelib if this project is exposed to COM
        ''' </summary>
        ''' <returns></returns>
        Public Property Guid As String

        ' Version information for an assembly consists of the following four values:
        '
        '      Major Version
        '      Minor Version
        '      Build Number
        '      Revision
        '
        ' You can specify all the values or you can default the Build and Revision Numbers
        ' by using the '*' as shown below:
        ' <Assembly: AssemblyVersion("1.0.*")>

        Public Property AssemblyVersion As String
        Public Property AssemblyFileVersion As String

        <XmlAttribute>
        Public Property AssemblyFullName As String

        ''' <summary>
        ''' The compile date and time of the assembly file.
        ''' </summary>
        ''' <returns></returns>
        Public Property BuiltTime As Date

        Public Const ProjectFile As String = "My Project\AssemblyInfo.vb"

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return AssemblyTitle
        End Function
    End Class
End Namespace
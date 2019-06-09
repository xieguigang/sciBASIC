Imports System.ComponentModel

Namespace sln

    ''' <summary>
    ''' Microsoft Visual Studio Solution File
    ''' </summary>
    Public Class Solution

        Public Property FormatVersion As String
        Public Property VisualStudioVersion As String
        Public Property MinimumVisualStudioVersion As String
        Public Property Projects As Project()

    End Class

    Public Enum TypeId
        <Description("2150E333-8FDC-42A3-9474-1A3956D46DE8")> FolderGroup
        <Description("F184B08F-C81C-45F6-A57F-5ABD9991F28F")> VBProject
        <Description("9092AA53-FB77-4645-B42D-1CCCA6BD08BD")> NjsProject
    End Enum

    Public Class Project
        Public Property NodeType As TypeId
        Public Property Guid As String
        ''' <summary>
        ''' The node display name
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String
        ''' <summary>
        ''' Includes virtual solution folder and vbproj file path 
        ''' </summary>
        ''' <returns></returns>
        Public Property TreePath As String
    End Class
End Namespace



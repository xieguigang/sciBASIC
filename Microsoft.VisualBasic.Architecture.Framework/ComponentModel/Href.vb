Imports System.Xml.Serialization

Namespace ComponentModel

    ''' <summary>
    ''' Resource link data.
    ''' </summary>
    ''' <remarks></remarks>
    <XmlType("href-text", namespace:="Microsoft.VisualBasic/Href_Annotation-Text")>
    Public Class Href : Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable

#Region "Public Property"

        <XmlAttribute("Resource.Id", Namespace:="Microsoft.VisualBasic/Href_Annotation-ResourceId")>
        Public Property ResourceId As String Implements Collection.Generic.sIdEnumerable.Identifier
        ''' <summary>
        ''' The relative path of the target resource object in the file system.(资源对象在文件系统之中的相对路径)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlElement("href-text", namespace:="Microsoft.VisualBasic/Href_Annotation-Text-Data")>
        Public Property Value As String
        <XmlElement("Annotations", namespace:="Microsoft.VisualBasic/Href_Annotation-Text-Annotations")>
        Public Property Annotations As String
#End Region

        ''' <summary>
        ''' 获取<see cref="Value"></see>所指向的资源文件的完整路径
        ''' </summary>
        ''' <param name="DIR"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetFullPath(DIR As String) As String
            Dim previous As String = FileIO.FileSystem.CurrentDirectory

            FileIO.FileSystem.CurrentDirectory = DIR
            Dim url As String = System.IO.Path.GetFullPath(Me.Value)
            FileIO.FileSystem.CurrentDirectory = previous
            Return url
        End Function

        Public Overrides Function ToString() As String
            Return Value
        End Function
    End Class
End Namespace
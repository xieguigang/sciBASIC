Namespace ComponentModel

    ''' <summary>
    ''' Resource link data.
    ''' </summary>
    ''' <remarks></remarks>
    <System.Xml.Serialization.XmlType("href-text", namespace:="Microsoft.VisualBasic/Href_Annotation-Text")>
    Public Class Href : Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.IDEnumerable

#Region "Public Property"

        <Xml.Serialization.XmlAttribute("Resource.Id", Namespace:="Microsoft.VisualBasic/Href_Annotation-ResourceId")>
        Public Property ResourceId As String Implements Collection.Generic.IDEnumerable.Identifier
        ''' <summary>
        ''' The relative path of the target resource object in the file system.(资源对象在文件系统之中的相对路径)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Xml.Serialization.XmlElement("href-text", namespace:="Microsoft.VisualBasic/Href_Annotation-Text-Data")>
        Public Property Value As String
        <Xml.Serialization.XmlElement("Annotations", namespace:="Microsoft.VisualBasic/Href_Annotation-Text-Annotations")>
        Public Property Annotations As String
#End Region

        ''' <summary>
        ''' 获取<see cref="Value"></see>所指向的资源文件的完整路径
        ''' </summary>
        ''' <param name="Dir"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetFullPath(Dir As String) As String
            Dim previous As String = FileIO.FileSystem.CurrentDirectory

            FileIO.FileSystem.CurrentDirectory = Dir
            Dim url As String = IO.Path.GetFullPath(Me.Value)
            FileIO.FileSystem.CurrentDirectory = previous
            Return url
        End Function

        Public Overrides Function ToString() As String
            Return Value
        End Function
    End Class
End Namespace
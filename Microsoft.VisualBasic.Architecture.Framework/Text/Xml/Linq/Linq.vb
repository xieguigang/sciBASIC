Imports System.Runtime.CompilerServices
Imports System.Xml

Namespace Text.Xml.Linq

    Public Module Data

        <Extension>
        Public Function LoadXmlDocument(path$) As XmlDocument
            Dim XmlDoc As New XmlDocument()
            Call XmlDoc.Load(path)
            Return XmlDoc
        End Function

        ''' <summary>
        ''' 这个函数只建议在读取超大的XML文件的时候使用，并且这个XML文件仅仅是一个数组或者列表的序列化结果
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="XML$">超大的XML文件的文件路径</param>
        ''' <param name="typeName">列表之中的节点在XML之中的tag标记名称</param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function LoadXmlDataSet(Of T As Class)(XML$, typeName$) As IEnumerable(Of T)
            Dim XmlNodeList As XmlNodeList = XML _
                .LoadXmlDocument _
                .GetElementsByTagName(typeName)
            Dim o As T

            For Each xmlNode As XmlNode In XmlNodeList
                ' Reset the user
                XML = xmlNode.InnerXml
                o = XML.CreateObjectFromXmlFragment(Of T)

                Yield o
            Next
        End Function
    End Module
End Namespace
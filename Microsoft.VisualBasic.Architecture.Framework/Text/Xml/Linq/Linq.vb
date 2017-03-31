Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml

Namespace Text.Xml.Linq

    ''' <summary>
    ''' Using large xml file as Linq data source
    ''' </summary>
    Public Module Data

        ''' <summary>
        ''' Load a specific xml file from a file location <paramref name="path"/> into a <see cref="XmlDocument"/> object.
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadXmlDocument(path$) As XmlDocument
            Dim XmlDoc As New XmlDocument()
            Call XmlDoc.Load(path)
            Return XmlDoc
        End Function

        ''' <summary>
        ''' Using <paramref name="default"/> string name or <see cref="Type.Name"/>
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="default$">
        ''' If this parameter value is <see cref="StringEmpty"/>, then <see cref="Type.Name"/> will be use as the xml node name.
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function GetTypeName(type As Type, default$) As String
            If [default].StringEmpty Then
                Return type.Name
            Else
                Return [default]
            End If
        End Function

        ''' <summary>
        ''' Only works for the xml file that contains a list or array of xml element, and then this function using this list element as linq data source.
        ''' (这个函数只建议在读取超大的XML文件的时候使用，并且这个XML文件仅仅是一个数组或者列表的序列化结果)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="XML$">超大的XML文件的文件路径</param>
        ''' <param name="typeName">
        ''' 列表之中的节点在XML之中的tag标记名称，假若这个参数值为空的话，则会默认使用目标类型名称<see cref="Type.Name"/>
        ''' </param>
        ''' <param name="xmlns">
        ''' Using for the namespace replacement.
        ''' (当这个参数存在的时候，目标命名空间申明将会被替换为空字符串，数据对象才会被正确的加载)
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function LoadXmlDataSet(Of T As Class)(XML$, Optional typeName$ = Nothing, Optional xmlns$ = Nothing) As IEnumerable(Of T)
            Dim nodeName$ = GetType(T).GetTypeName([default]:=typeName)
            Dim XmlNodeList As XmlNodeList = XML _
                .LoadXmlDocument _
                .GetElementsByTagName(nodeName)
            Dim o As T
            Dim sb As New StringBuilder

            For Each xmlNode As XmlNode In XmlNodeList
                XML = xmlNode.InnerXml

                Call sb.Clear()
                Call sb.AppendLine("<?xml version=""1.0"" encoding=""utf-16""?>")
                Call sb.Append($"<{typeName} xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""")
                Call sb.Append(" ")

                For Each attr As XmlAttribute In xmlNode.Attributes
                    Call sb.Append($"{attr.Name}=""{attr.Value}""")
                    Call sb.Append(" ")
                Next

                Call sb.AppendLine(">")
                Call sb.AppendLine(XML)
                Call sb.AppendLine($"</{typeName}>")

                If Not xmlns.StringEmpty Then
                    Call sb.Replace($"xmlns=""{xmlns}""", "")
                End If

                XML = sb.ToString
                o = XML.LoadFromXml(Of T)

                Yield o
            Next
        End Function
    End Module
End Namespace
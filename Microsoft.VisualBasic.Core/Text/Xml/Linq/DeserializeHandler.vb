Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml.Serialization

Namespace Text.Xml.Linq

    ''' <summary>
    ''' The xml deserialize helper
    ''' </summary>
    Public Class DeserializeHandler(Of T As Class)

        ReadOnly process As Func(Of String, String)
        ''' <summary>
        ''' Get type name that defined on the class type decoration with custom attributes:
        ''' <see cref="XmlTypeAttribute"/> or <see cref="XmlRootAttribute"/>
        ''' </summary>
        ReadOnly Tname$ = GetType(T).GetNodeNameDefine

        Public Property ReplaceXmlns As String

        Sub New(xmlNode As String)
            ' 2017-12-22
            ' 假若对象是存储在一个数组之中的，那么，可能会出现的一种情况就是
            ' 在类型的定义之中，使用了xmlelement重新定义了节点的名字
            ' 例如 <XmlElement("A")>
            ' 那么在生成的XML文件之中的节点名称就是A
            ' 但是元素A的类型定义却是 Public Class B ... End Class
            ' 因为A不等于B，所以将无法正确的加载XML节点数据
            ' 在这里进行名称的替换来消除这种错误
            If Tname = xmlNode Then
                ' 不需要进行替换
                process = Function(s) s
            Else
                Dim leftTag% = 1 + xmlNode.Length
                Dim rightTag% = 3 + xmlNode.Length

                ' 在这里不尝试做直接替换，可能会误杀其他的节点
                process = Function(block)
                              block = block.Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB)
                              block = block.Substring(leftTag, block.Length - leftTag)
                              block = block.Substring(0, block.Length - rightTag)
                              block = "<" & Tname & block & "</" & Tname & ">"

                              Return block
                          End Function
            End If
        End Sub

        ReadOnly sb As New StringBuilder

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RemoveXmlns(xml As String) As String
            Return xml.Replace($"xmlns=""{ReplaceXmlns}""", "")
        End Function

        Public Function LoadXml(xml As String) As T
            Call sb.Clear()
            Call sb.AppendLine("<?xml version=""1.0"" encoding=""utf-16""?>")
            Call sb.AppendLine(process(xml))

            If Not ReplaceXmlns.StringEmpty Then
                Call sb.Replace($"xmlns=""{ReplaceXmlns}""", "")
            End If

            xml = sb.ToString

            ' 对调整好的Xml文档执行反序列化操作
            Return xml.LoadFromXml(Of T)
        End Function

        Public Overrides Function ToString() As String
            Return $"LoadXml(Of {Tname})"
        End Function
    End Class
End Namespace
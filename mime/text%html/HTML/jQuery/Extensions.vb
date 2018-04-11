Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta

Namespace HTML.jQuery

    Public Module Extensions

        ''' <summary>
        ''' 如果定义了<see cref="XmlTypeAttribute"/>，则优先使用这个属性之中的<see cref="XmlTypeAttribute.TypeName"/>
        ''' 作为文档元素标签的名字，没有找到的话，则直接使用``Class Name``来作为文档元素标签的名字
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="element"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TagName(Of T As Node)(element As T) As String
            Dim type As Type = GetType(T)
            Dim xmlType As XmlTypeAttribute = type.GetCustomAttribute(Of XmlTypeAttribute)

            If xmlType Is Nothing Then
                Return type.Name
            Else
                Return xmlType.TypeName
            End If
        End Function
    End Module
End Namespace
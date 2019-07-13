#Region "Microsoft.VisualBasic::4ae207281fa3fb4e0208111d9c26bab6, Microsoft.VisualBasic.Core\Text\Xml\XmlDoc.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class XmlDoc
    ' 
    '         Properties: encoding, rootNode, standalone, version, xmlns
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __rootString, CreateObject, FromObject, FromXmlFile, Save
    '                   SaveTo, (+2 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Text.Xml

    ''' <summary>
    ''' 请使用<see cref="XmlDoc.ToString"/>方法获取修改之后的Xml文档
    ''' </summary>
    Public Class XmlDoc : Implements ISaveHandle

        Public Const XmlDeclares As String = "<\?xml.+?>"

        ReadOnly xml As String

        Public Property version As String
        Public Property standalone As Boolean
        Public Property encoding As XmlEncodings

        Public ReadOnly Property rootNode As String
        ''' <summary>
        ''' Xml namespace definitions
        ''' </summary>
        ''' <returns></returns>
        Public Property xmlns As Xmlns

        ''' <summary>
        ''' Create a xml tools from xml document text.
        ''' </summary>
        ''' <param name="xml"></param>
        Sub New(xml As String)
            Dim [declare] As New XmlDeclaration(
                Regex.Match(xml, XmlDeclares, RegexICSng).Value)
            version = [declare].version
            standalone = [declare].standalone
            encoding = [declare].encoding

            Dim root As NamedValue(Of Xmlns) =
                Xmlns.RootParser(__rootString(xml))
            rootNode = root.Name
            xmlns = root.Value
            Me.xml = xml
        End Sub

        Protected Friend Shared Function __rootString(xml As String) As String
            xml = Regex.Match(xml, XmlDeclares & ".*?<.+?>", RegexICSng).Value
            xml = xml.Replace(Regex.Match(xml, XmlDeclares, RegexICSng).Value, "").Trim
            Return xml
        End Function

        ''' <summary>
        ''' 使用这个函数可以得到修改之后的Xml文档
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return ToString(False)
        End Function

        ''' <summary>
        ''' 使用这个函数可以得到修改之后的Xml文档
        ''' </summary>
        ''' <param name="usingDefault_xmlns"><see cref="xmlns.DefaultXmlns"/></param>
        ''' <returns></returns>
        Public Overloads Function ToString(Optional usingDefault_xmlns As Boolean = False) As String
            Dim [declare] As String = Regex.Match(xml, XmlDeclares, RegexICSng).Value
            Dim setDeclare As New XmlDeclaration With {
                .encoding = encoding,
                .standalone = standalone,
                .version = version
            }

            Dim doc As New StringBuilder(xml)
            Call doc.Replace([declare], setDeclare.ToString)
            Call xmlns.WriteNamespace(doc, usingDefault_xmlns)
            Return doc.ToString
        End Function

        ''' <summary>
        ''' 从新修改过的xml文档之中通过反序列化构建目标对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="usingDefault_xmlns"></param>
        ''' <returns></returns>
        Public Function CreateObject(Of T)(Optional usingDefault_xmlns As Boolean = False) As T
            Return ToString(usingDefault_xmlns).LoadFromXml(Of T)
        End Function

        Public Shared Function FromObject(Of T As Class)(x As T) As XmlDoc
            Return New XmlDoc(x.GetXml)
        End Function

        Public Shared Function FromXmlFile(path As String) As XmlDoc
            Return New XmlDoc(path.ReadAllText)
        End Function

        ''' <summary>
        ''' Me.ToString.SaveTo(Path, encoding)
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Function SaveTo(Path$, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Return Me.ToString.SaveTo(Path, encoding)
        End Function

        Public Function Save(Path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveTo(Path, encoding.CodePage)
        End Function
    End Class
End Namespace

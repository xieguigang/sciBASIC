Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization

Public Class XmlDoc : Implements ISaveHandle

    Public Const XmlDeclares As String = "<\?xml.+?>"

    Public Enum XmlEncodings
        UTF8
        UTF16
    End Enum

    ReadOnly xml As String

    Public Property version As String
    Public Property standalone As Boolean
    Public Property encoding As XmlDoc.XmlEncodings

    Public ReadOnly Property rootNode As String
    ''' <summary>
    ''' Xml namespace definitions
    ''' </summary>
    ''' <returns></returns>
    Public Property xmlns As Xmlns

    Sub New(xml As String)
        Dim [declare] As New XmlDeclaration(
            Regex.Match(xml, XmlDeclares, RegexICSng).Value)
        version = [declare].version
        standalone = [declare].standalone
        encoding = [declare].encoding

        Dim root As NamedValue(Of Xmlns) =
            Xmlns.RootParser(__rootString(xml))
        rootNode = root.Name
        xmlns = root.x
        Me.xml = xml
    End Sub

    Protected Friend Shared Function __rootString(xml As String) As String
        xml = Regex.Match(xml, XmlDeclares & ".+?<.+?>", RegexICSng).Value
        xml = xml.Replace(Regex.Match(xml, XmlDeclares, RegexICSng).Value, "").Trim
        Return xml
    End Function

    Public Overrides Function ToString() As String
        Dim [declare] As String = Regex.Match(xml, XmlDeclares, RegexICSng).Value
        Dim setDeclare As New XmlDeclaration With {
            .encoding = encoding,
            .standalone = standalone,
            .version = version
        }

        Dim doc As New StringBuilder(xml)
        Call doc.Replace([declare], setDeclare.ToString)
        Call xmlns.WriteNamespace(doc)
        Return doc.ToString
    End Function

    Public Shared Function FromObject(Of T As Class)(x As T) As XmlDoc
        Return New XmlDoc(x.GetXml)
    End Function

    Public Function SaveTo(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
        Return Me.ToString.SaveTo(Path, encoding)
    End Function

    Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Return SaveTo(Path, encoding.GetEncodings)
    End Function
End Class

Public Class Xmlns

    Public Property xmlns As String
    Public Property xlink As String
    Public Property xsd As String
    Public Property xsi As String

    Sub New(root As String)
        Dim s As String

        s = Regex.Match(root, "xmlns:xsd="".+?""", RegexICSng).Value
        xsd = s.GetStackValue("""", """")
        s = Regex.Match(root, "xmlns:xsi="".+?""", RegexICSng).Value
        xsi = s.GetStackValue("""", """")
        s = Regex.Match(root, "xmlns="".+?""", RegexICSng).Value
        xmlns = s.GetStackValue("""", """")
        s = Regex.Match(root, "xmlns:xlink="".+?""", RegexICSng).Value
        xlink = s.GetStackValue("""", """")
    End Sub

    ''' <summary>
    ''' The parser for the xml root node.
    ''' </summary>
    ''' <param name="root"></param>
    ''' <returns></returns>
    Public Shared Function RootParser(root As String) As NamedValue(Of Xmlns)
        Dim ns As New Xmlns(root)
        Dim name As String = root.Split.First
        name = Mid(name, 2)
        Return New NamedValue(Of Xmlns)(name, ns)
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="xml"></param>
    ''' <remarks>当前的这个对象是新值，需要替换掉文档里面的旧值</remarks>
    Public Sub WriteNamespace(xml As StringBuilder)
        Dim rs As String = XmlDoc.__rootString(xml.ToString)
        Dim root As Xmlns = New Xmlns(rs) ' old xmlns value
        Dim ns As New StringBuilder(rs)  ' 可能还有其他的属性，所以在这里还不可以直接拼接字符串然后直接进行替换

        If Not String.IsNullOrEmpty(root.xsd) Then
            Call ns.Replace($"xmlns:xsd=""{root.xsd}""", If(String.IsNullOrEmpty(xsd), "", $"xmlns:xsd=""{xsd}"""))
        Else
            If Not String.IsNullOrEmpty(xsd) Then
                Call ns.Replace(">", $" xmlns:xsd=""{xsd}"">")
            End If
        End If

        If Not String.IsNullOrEmpty(root.xsi) Then
            Call ns.Replace($"xmlns:xsi=""{root.xsi}""", If(String.IsNullOrEmpty(xsi), "", $"xmlns:xsi=""{xsi}"""))
        Else
            If Not String.IsNullOrEmpty(xsi) Then
                Call ns.Replace(">", $" xmlns:xsi=""{xsi}"">")
            End If
        End If

        If Not String.IsNullOrEmpty(root.xmlns) Then
            Call ns.Replace($"xmlns=""{root.xmlns}""", If(String.IsNullOrEmpty(xmlns), "", $"xmlns=""{xmlns}"""))
        Else
            If Not String.IsNullOrEmpty(xmlns) Then
                Call ns.Replace(">", $" xmlns=""{xmlns}"">")
            End If
        End If

        If Not String.IsNullOrEmpty(root.xlink) Then
            Call ns.Replace($"xmlns:xlink=""{root.xlink}""", If(String.IsNullOrEmpty(xlink), "", $"xmlns:xlink=""{xlink}"""))
        Else
            If Not String.IsNullOrEmpty(xlink) Then
                Call ns.Replace(">", $" xmlns:xlink=""{xlink}"">")
            End If
        End If

        Call xml.Replace(rs, ns.ToString)
    End Sub
End Class

Public Structure XmlDeclaration

    Public version As String
    Public standalone As Boolean
    Public encoding As XmlDoc.XmlEncodings

    Sub New(declares As String)
        Dim s As String

        s = Regex.Match(declares, "encoding=""\S+""", RegexICSng).Value
        encoding = EncodingParser(s.GetStackValue("""", """"))
        s = Regex.Match(declares, "standalone=""\S+""", RegexICSng).Value
        standalone = s.GetStackValue("""", """").getBoolean
        s = Regex.Match(declares, "version=""\S+""", RegexICSng).Value
        version = s.GetStackValue("""", """")
    End Sub

    Public Shared ReadOnly Property [Default] As XmlDeclaration =
        New XmlDeclaration With {
            .version = "1.0",
            .standalone = True,
            .encoding = XmlDoc.XmlEncodings.UTF16
    }

    ''' <summary>
    ''' &lt;?xml version="{<see cref="version"/>}" encoding="{<see cref="encoding"/>}" standalone="{<see cref="standalone"/>}"?>
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function ToString() As String
        If String.IsNullOrEmpty(version) Then
            version = "1.0"
        End If
        Return $"<?xml version=""{version}"" encoding=""{XmlEncodingString(encoding)}"" standalone=""{XmlStandaloneString(standalone)}""?>"
    End Function

    Public Shared Function XmlEncodingString(enc As XmlDoc.XmlEncodings) As String
        Return If(enc = XmlDoc.XmlEncodings.UTF8, "utf-8", "utf-16")
    End Function

    Public Shared Function EncodingParser(enc As String) As XmlDoc.XmlEncodings
        If String.Equals(enc, "utf-8", StringComparison.OrdinalIgnoreCase) Then
            Return XmlDoc.XmlEncodings.UTF8
        Else
            Return XmlDoc.XmlEncodings.UTF16
        End If
    End Function

    Public Shared Function XmlStandaloneString(standalone As Boolean) As String
        Return If(standalone, "yes", "no")
    End Function
End Structure
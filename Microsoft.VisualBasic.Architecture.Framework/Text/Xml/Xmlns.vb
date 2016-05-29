Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization

Namespace Text.Xml

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
End Namespace
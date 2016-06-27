Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml

    ''' <summary>
    ''' Xml namespace
    ''' </summary>
    Public Class Xmlns

        Public Property xmlns As String
        Public Property [namespace] As Dictionary(Of NamedValue(Of String))

        Public Property xsd As String
            Get
                Return Me(xmlns_xsd)
            End Get
            Set(value As String)
                [namespace](xmlns_xsd) = New NamedValue(Of String)(xmlns_xsd, value)
            End Set
        End Property

        Public Property xsi As String
            Get
                Return Me(xmlns_xsi)
            End Get
            Set(value As String)
                [namespace](xmlns_xsi) = New NamedValue(Of String)(xmlns_xsi, value)
            End Set
        End Property

        Default Public ReadOnly Property ns(name As String) As String
            Get
                If [namespace].ContainsKey(name) Then
                    Return [namespace](name).x
                Else
                    Return ""
                End If
            End Get
        End Property

        Const xmlnsRegex As String = "\sxmlns:\S+="".+?"""
        Const xmlns_xsd As String = "xmlns:xsd"
        Const xmlns_xsi As String = "xmlns:xsi"

        Sub New(root As String)
            Dim s As String

            [namespace] = New Dictionary(Of NamedValue(Of String))
            s = Regex.Match(root, "xmlns="".+?""", RegexICSng).Value
            xmlns = s.GetStackValue("""", """")

            Dim nsList As String() =
                Regex.Matches(root, xmlnsRegex, RegexICSng) _
                .ToArray(AddressOf Trim)

            For Each ns As String In nsList
                [namespace] += ns.GetTagValue("=") _
                    .FixValue(Function(x) x.GetStackValue("""", """"))
            Next
        End Sub

        ''' <summary>
        ''' Set the value of a new xml namespace.(<paramref name="ns"/>命名空间参数不需要添加 xmlns: 前缀)
        ''' </summary>
        ''' <param name="ns"></param>
        ''' <param name="value"></param>
        Public Sub [Set](ns As String, value As String)
            ns = $"xmlns:{ns}"
            [namespace](ns) = New NamedValue(Of String)(ns, value)
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

            For Each nsValue As NamedValue(Of String) In [namespace].Values
                Dim rootNs As String = root(nsValue.Name)

                If Not String.IsNullOrEmpty(rootNs) Then
                    Dim s As String =
                        If(String.IsNullOrEmpty(nsValue.x),
                        "",
                        $"{nsValue.Name}=""{nsValue.x}""")
                    Call ns.Replace($"{nsValue.Name}=""{rootNs}""", s)
                Else
                    If Not String.IsNullOrEmpty(nsValue.x) Then
                        Call ns.Replace(">", $" {nsValue.Name}=""{nsValue.x}"">")
                    End If
                End If
            Next

            If Not String.IsNullOrEmpty(xmlns) Then
                If Not String.IsNullOrEmpty(root.xmlns) Then
                    Call ns.Replace($"xmlns=""{root.xmlns}""", "")
                End If

                Call ns.Replace(">", $" xmlns=""{xmlns}"">")
            End If

            Call xml.Replace(rs, ns.ToString)
        End Sub
    End Class
End Namespace
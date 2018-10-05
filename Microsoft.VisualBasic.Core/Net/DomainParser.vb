#Region "Microsoft.VisualBasic::acd52a4700c2bedeb47904664bf7bcab, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\DomainParser.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Structure DomainName
    ' 
    '         Properties: Domain, Invalid, TLD
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Module DomainParser
    ' 
    '         Function: Trim, TrimPathAndQuery, (+2 Overloads) TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace Net

    Public Structure DomainName : Implements IKeyValuePairObject(Of String, String)

        Public Property Domain As String Implements IKeyValuePairObject(Of String, String).Key
        ''' <summary>
        ''' 顶级域名
        ''' </summary>
        ''' <returns></returns>
        Public Property TLD As String Implements IKeyValuePairObject(Of String, String).Value

        Sub New(url As String)
            url = TryParse(url)
            Dim Tokens As String() = url.Split(CChar("."))
            Domain = Tokens(0)
            TLD = Tokens.Skip(1).JoinBy(".")
        End Sub

        Public ReadOnly Property Invalid As Boolean
            Get
                Return (String.IsNullOrEmpty(Domain) OrElse String.IsNullOrEmpty(TLD))
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Domain}.{TLD}"
        End Function
    End Structure

    ''' <summary>
    ''' http://sub.domain.com/somefolder/index.html -> domain.com
    ''' somedomain.info -> somedomain.info
    ''' http://anotherdomain.org/home -> anotherdomain.org
    ''' www.subdomain.anothersubdomain.maindomain.com/something/ -> maindomain.com
    ''' </summary>
    Public Module DomainParser

        ''' <summary>
        ''' 解析错误会返回空字符串
        ''' </summary>
        ''' <param name="url"></param>
        ''' <returns></returns>
        Public Function TryParse(url As String, Optional preserveSubdomain As Boolean = False) As String
            url = Trim(url)
            url = TrimPathAndQuery(url, preserveSubdomain)
            Return url
        End Function

        Public Function TryParse(url As Value(Of String), ByRef DomainName As DomainName) As Boolean
            If String.IsNullOrEmpty(url = TryParse(+url)) Then
                Return False
            End If

            DomainName = New DomainName(+url)
            Return True
        End Function

        Private Function TrimPathAndQuery(url As String, preserveSubdomain As Boolean) As String
            url = url.Split(CChar("/")).First

            If preserveSubdomain Then
                Return url
            End If

            Dim tokens As New List(Of String)(url.Split(CChar(".")))

            If tokens.Count = 2 Then
                Return url
            ElseIf tokens.Count = 1 Then
                Return ""
            End If

            ' 剩下的这些事token数量大于等于3的情况
            Dim tld2 As String = tokens(tokens.Count - 2)  ' 处理类似于.com.cn这种情况

            ' .com.cn
            ' .co.uk
            ' .ac.cn
            If InStr("co|ac|com|org|net|edu", tld2, CompareMethod.Text) > 0 Then  ' .com.cn,,..co.uk的情况，则直接返回
                ' 取最后的三个token
                If tokens.Count > 3 Then
                    tokens = New List(Of String)(tokens.GetRange(tokens.Count - 3, 3))
                End If
                url = String.Join(".", tokens.ToArray)
            Else
                url = $"{tokens(tokens.Count - 2)}.{tokens(tokens.Count - 1)}"
            End If

            Return url
        End Function

        Private Function Trim(url As String) As String

            For Each protocol As String In {"http://", "file://", "https://", "ftp://"}
                If InStr(url, protocol, CompareMethod.Text) = 1 Then
                    url = Mid(url, Len(protocol) + 1)
                    Return url
                End If
            Next

            If InStr(url, "mailto://", CompareMethod.Text) = 1 Then
                url = url.Split("@"c).Last
            End If

            Return url
        End Function
    End Module
End Namespace

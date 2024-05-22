#Region "Microsoft.VisualBasic::7c9c474c26fb4e48d9651e5976b144f1, mime\text%html\Document\Head\Meta.vb"

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


    ' Code Statistics:

    '   Total Lines: 173
    '    Code Lines: 132 (76.30%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 41 (23.70%)
    '     File Size: 4.84 KB


    '     Class Charset
    ' 
    '         Properties: CharSet
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __attrs
    ' 
    '     Class ContentType
    ' 
    '         Properties: type
    ' 
    '         Function: __attrs
    ' 
    '     Class MetaData
    ' 
    '         Function: ToString
    ' 
    '     Class Meta
    ' 
    '         Properties: content, Name
    ' 
    '         Function: ToString
    ' 
    '     Class Description
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Author
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Copyright
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class Keywords
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class Viewport
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class Link
    ' 
    '         Properties: href, rel
    ' 
    '         Function: ToString
    ' 
    '     Class Script
    ' 
    '         Properties: type
    ' 
    '     Class ScriptRef
    ' 
    '         Properties: src
    ' 
    '         Function: ToString
    ' 
    '     Class ScriptDeclare
    ' 
    '         Properties: Content
    ' 
    '         Function: ToString
    ' 
    '     Class HeadMeta
    ' 
    '         Properties: Links, MetaDatas, Metas, Scripts, Title
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Linq

Namespace HTML.Head

    Public Class Charset : Inherits MetaData

        Public Property CharSet As String

        Sub New(charset As String)
            Me.CharSet = charset
        End Sub

        Protected Overrides Function __attrs() As String
            Return $"charset=""{CharSet}"""
        End Function
    End Class

    Public Class ContentType : Inherits MetaData

        Public Property type As String = "text/html; charset=UTF-8"

        Protected Overrides Function __attrs() As String
            Return $"http-equiv=""Content-Type"" content=""{type}"""
        End Function
    End Class

    Public MustInherit Class MetaData

        Protected MustOverride Function __attrs() As String

        Public Overrides Function ToString() As String
            Return $"<meta {__attrs()} />"
        End Function
    End Class

    Public Class Meta
        Public Property Name As String
        Public Property content As String

        Public Overrides Function ToString() As String
            Return $"<meta name=""{Name}"" content=""{content}"" />"
        End Function
    End Class

    Public Class Description : Inherits Meta

        Sub New()
            Name = "description"
        End Sub
    End Class

    Public Class Author : Inherits Meta

        Sub New()
            Name = "author"
        End Sub
    End Class

    Public Class Copyright : Inherits Meta

        Sub New()
            Name = "copyright"
        End Sub

        Sub New(Company As String, Year As String)
            Call Me.New
            content = $"{Company} Copyright(c) {Year}. All rights reserved."
        End Sub
    End Class

    Public Class Keywords : Inherits Meta

        Sub New()
            Name = "keywords"
        End Sub

        Sub New(lstKeys As Generic.IEnumerable(Of String))
            Call Me.New
            content = lstKeys.JoinBy("; ")
        End Sub
    End Class

    Public Class Viewport : Inherits Meta

        Sub New()
            Name = "viewport"
        End Sub

        Sub New(width As String, initialScale As String)
            Call Me.New
            content = $"width={width}, initial-scale={initialScale}"
        End Sub
    End Class

    Public Class Link
        Public Property rel As String
        Public Property href As String

        Public Overrides Function ToString() As String
            Return $"<link rel=""{rel}"" href=""{href}"" />"
        End Function
    End Class

    Public MustInherit Class Script
        Public Property type As String = "text/javascript"

    End Class

    Public Class ScriptRef : Inherits Script

        Public Property src As String

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(type) Then
                Return $"<script src=""{src}""></script>"
            Else
                Return $"<script type=""{type}"" src=""{src}""></script>"
            End If
        End Function
    End Class

    Public Class ScriptDeclare : Inherits Script

        Public Property Content As String

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(type) Then
                Return $"<script>
{Content}
</script>"
            Else
                Return $"<script type=""{type}"">
{Content}
</script>"
            End If
        End Function
    End Class

    Public Class HeadMeta

        Public Property Metas As Meta()
        Public Property MetaDatas As MetaData()
        Public Property Links As Link()
        Public Property Scripts As Script()
        Public Property Title As String

        Public Overrides Function ToString() As String
            Dim htmlBuilder As New StringBuilder("<head>")

            For Each meta In Metas.SafeQuery
                Call htmlBuilder.AppendLine(vbTab & meta.ToString)
            Next
            Call htmlBuilder.AppendLine()
            For Each meta In MetaDatas.SafeQuery
                Call htmlBuilder.AppendLine(vbTab & meta.ToString)
            Next
            Call htmlBuilder.AppendLine()
            For Each link In Links.SafeQuery
                Call htmlBuilder.AppendLine(vbTab & link.ToString)
            Next
            Call htmlBuilder.AppendLine()
            For Each script In Scripts.SafeQuery
                Call htmlBuilder.AppendLine(vbTab & script.ToString)
            Next
            Call htmlBuilder.AppendLine()
            Call htmlBuilder.AppendLine(vbTab & $"<title>{Title}</title>")
            Call htmlBuilder.AppendLine("</head>")

            Return htmlBuilder.ToString
        End Function
    End Class
End Namespace

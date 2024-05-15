#Region "Microsoft.VisualBasic::527088b05cd9e8df35457f3a57f650e1, Data\GraphQuery\Query\Parser\Parser.vb"

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

    '   Total Lines: 67
    '    Code Lines: 51
    ' Comment Lines: 2
    '   Blank Lines: 14
    '     File Size: 2.22 KB


    ' Class Parser
    ' 
    '     Properties: func, parameters, pipeNext
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: GetElementByIndex, Parse, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.Html.Document

Public MustInherit Class Parser

    Public Property func As String
    Public Property parameters As String()
    Public Property pipeNext As Parser

    Sub New()
    End Sub

    Sub New(func As String, parameters As String())
        Me.func = func
        Me.parameters = parameters
    End Sub

    Public Function Parse(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText
        ' only the last parser works in array mode
        ' if the isArray parameter is set to TRUE
        Dim arrayMode As Boolean = isArray AndAlso pipeNext Is Nothing
        Dim value As InnerPlantText = ParseImpl(document, arrayMode, env)

        If Not pipeNext Is Nothing Then
            value = pipeNext.Parse(value, isArray, env)
        End If

        Return value
    End Function

    Protected MustOverride Function ParseImpl(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText

    Protected Shared Function GetElementByIndex(list As InnerPlantText(), i As Integer?) As InnerPlantText
        If i Is Nothing Then
            Return New HtmlElement With {
                .HtmlElements = list,
                .Attributes = {AutoContext.Attribute}
            }
        ElseIf i >= list.Length Then
            Return New InnerPlantText With {.InnerText = ""}
        Else
            Return list(i)
        End If
    End Function

    Public Overrides Function ToString() As String
        Dim thisText As String = $"{func}({parameters.JoinBy(", ")})"

        If Not pipeNext Is Nothing Then
            Return $"{thisText} -> {pipeNext}"
        Else
            Return thisText
        End If
    End Function

    Public Shared Operator &(left As Parser, [next] As Parser) As Parser
        If left.pipeNext Is Nothing Then
            left.pipeNext = [next]
        Else
#Disable Warning BC42004 ' Expression recursively calls the containing Operator
            left.pipeNext = left.pipeNext & [next]
#Enable Warning BC42004 ' Expression recursively calls the containing Operator
        End If

        Return left
    End Operator

End Class

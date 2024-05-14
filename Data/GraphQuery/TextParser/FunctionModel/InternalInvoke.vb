#Region "Microsoft.VisualBasic::9dc43e741a7877e50f74b37ecc4ad130, Data\GraphQuery\TextParser\FunctionModel\InternalInvoke.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 617 B


    '     Class InternalInvoke
    ' 
    '         Properties: method, name
    ' 
    '         Function: GetToken, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.MIME.Html.Document

Namespace TextParser

    Public Class InternalInvoke : Inherits ParserFunction

        Public Property name As String
        Public Property method As MethodInfo

        Public Overrides Function GetToken(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Return method.Invoke(Nothing, {document, parameters, isArray})
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class
End Namespace

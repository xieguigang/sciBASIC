#Region "Microsoft.VisualBasic::051141ddd257d0b4b3e565485ba68e66, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\docProps\app.xml.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace docProps

    Public Class app : Inherits IXml

        Public Property Application As String
        Public Property DocSecurity As String
        Public Property ScaleCrop As Boolean
        Public Property HeadingPairs As HeadingPairs
        Public Property Company As String
        Public Property LinksUpToDate As Boolean
        Public Property SharedDoc As Boolean
        Public Property HyperlinksChanged As Boolean
        Public Property AppVersion As String

        Protected Overrides Function filePath() As String
            Return "docProps/app.xml"
        End Function

        Protected Overrides Function toXml() As String
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class HeadingPairs

    End Class
End Namespace

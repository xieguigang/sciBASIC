#Region "Microsoft.VisualBasic::9d1eeb0947b743ccaca3e66276936d28, sciBASIC#\mime\text%markdown\Markups\Paragraph.vb"

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

    '   Total Lines: 36
    '    Code Lines: 23
    ' Comment Lines: 3
    '   Blank Lines: 10
    '     File Size: 814.00 B


    ' Class ParagraphText
    ' 
    '     Properties: Nodes
    ' 
    '     Function: ToString
    ' 
    ' Class Bold
    ' 
    ' 
    ' 
    ' Class PlantText
    ' 
    '     Properties: Text
    ' 
    '     Function: ToString
    ' 
    ' Class Hyperlink
    ' 
    '     Properties: Links, Title
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class ParagraphText : Inherits PlantText

    Public Property Nodes As PlantText()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class Bold : Inherits ParagraphText
End Class

''' <summary>
''' 单纯的文本对象
''' </summary>
Public Class PlantText

    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Text
    End Function
End Class

Public Class Hyperlink : Inherits PlantText

    Public Property Links As String
    Public Property Title As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

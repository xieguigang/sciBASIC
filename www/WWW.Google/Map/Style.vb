#Region "Microsoft.VisualBasic::d19aea302410f605b59a8c09be4b64ec, sciBASIC#\www\WWW.Google\Map\Style.vb"

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

    '   Total Lines: 72
    '    Code Lines: 56
    ' Comment Lines: 0
    '   Blank Lines: 16
    '     File Size: 1.99 KB


    '     Class Style
    ' 
    '         Properties: IconStyle, id, LabelStyle
    ' 
    '         Function: ToString
    ' 
    '     Class StyleMap
    ' 
    '         Properties: id, Pairs
    ' 
    '         Function: ToString
    ' 
    '     Class Pair
    ' 
    '         Properties: key, styleUrl
    ' 
    '         Function: ToString
    ' 
    '     Class LabelStyle
    ' 
    '         Properties: scale
    ' 
    '         Function: ToString
    ' 
    '     Class IconStyle
    ' 
    '         Properties: color, hotSpot, Icon, scale
    ' 
    '         Function: ToString
    ' 
    '     Class hotSpot
    ' 
    '         Properties: x, xunits, y, yunits
    ' 
    '         Function: ToString
    ' 
    '     Class Link
    ' 
    '         Properties: href
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Map

    Public Class Style
        <XmlAttribute> Public Property id As String
        Public Property IconStyle As IconStyle
        Public Property LabelStyle As LabelStyle

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class StyleMap
        <XmlAttribute> Public Property id As String
        <XmlElement("Pair")> Public Property Pairs As Pair()

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class Pair
        Public Property key As String
        Public Property styleUrl As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class LabelStyle
        Public Property scale As Double

        Public Overrides Function ToString() As String
            Return scale
        End Function
    End Class

    Public Class IconStyle
        Public Property color As String
        Public Property scale As Double
        Public Property Icon As Link
        Public Property hotSpot As hotSpot

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class hotSpot
        <XmlAttribute> Public Property x As Double
        <XmlAttribute> Public Property y As Double
        <XmlAttribute> Public Property xunits As String
        <XmlAttribute> Public Property yunits As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class Link

        Public Property href As String

        Public Overrides Function ToString() As String
            Return href
        End Function
    End Class
End Namespace

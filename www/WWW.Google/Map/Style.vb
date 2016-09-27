#Region "Microsoft.VisualBasic::ce90094922c48c1e39fd624fbf9e80e6, ..\visualbasic_App\www\WWW.Google\Map\Style.vb"

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

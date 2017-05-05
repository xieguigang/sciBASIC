#Region "Microsoft.VisualBasic::7292fb0e586cecce96486f189c75464c, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\ColorBrewer.vb"

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

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors

    Public Class ColorBrewer

        Public Property c3 As String()
        Public Property c4 As String()
        Public Property c5 As String()
        Public Property c6 As String()
        Public Property c7 As String()
        Public Property c8 As String()
        Public Property c9 As String()
        Public Property c10 As String()
        Public Property c11 As String()
        Public Property c12 As String()

        Public Property type As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function GetColors(name$) As Color()
            Select Case LCase(name)
                Case NameOf(c3)
                    Return c3.ToArray(AddressOf ToColor)
                Case NameOf(c4)
                    Return c4.ToArray(AddressOf ToColor)
                Case NameOf(c5)
                    Return c5.ToArray(AddressOf ToColor)
                Case NameOf(c6)
                    Return c6.ToArray(AddressOf ToColor)
                Case NameOf(c7)
                    Return c7.ToArray(AddressOf ToColor)
                Case NameOf(c8)
                    Return c8.ToArray(AddressOf ToColor)
                Case NameOf(c9)
                    Return c9.ToArray(AddressOf ToColor)
                Case NameOf(c10)
                    Return c10.ToArray(AddressOf ToColor)
                Case NameOf(c11)
                    Return c11.ToArray(AddressOf ToColor)
                Case NameOf(c12)
                    Return c12.ToArray(AddressOf ToColor)
                Case Else
                    Return c3.ToArray(AddressOf ToColor)
            End Select
        End Function

        ''' <summary>
        ''' example: ``Accent:c6``
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <returns></returns>
        Public Shared Function ParseName(term$) As NamedValue(Of String)
            Return term.GetTagValue(":")
        End Function

#Region "div"

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Spectral As Color()
            Get
                Return Designer.GetColors("Spectral:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RdYlGn As Color()
            Get
                Return Designer.GetColors("RdYlGn:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RdBu As Color()
            Get
                Return Designer.GetColors("RdBu:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PiYG As Color()
            Get
                Return Designer.GetColors("PiYG:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PRGn As Color()
            Get
                Return Designer.GetColors("PRGn:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RdYlBu As Color()
            Get
                Return Designer.GetColors("RdYlBu:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BrBG As Color()
            Get
                Return Designer.GetColors("BrBG:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RdGy As Color()
            Get
                Return Designer.GetColors("RdGy:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PuOr As Color()
            Get
                Return Designer.GetColors("PuOr:c11")
            End Get
        End Property
#End Region

#Region "qual"

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Set2 As Color()
            Get
                Return Designer.GetColors("Set2:c8")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Accent As Color()
            Get
                Return Designer.GetColors("Accent:c8")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Set1 As Color()
            Get
                Return Designer.GetColors("Set1:c9")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Set3 As Color()
            Get
                Return Designer.GetColors("Set3:c12")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Dark2 As Color()
            Get
                Return Designer.GetColors("Dark2:c8")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Paired As Color()
            Get
                Return Designer.GetColors("Paired:c12")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Pastel2 As Color()
            Get
                Return Designer.GetColors("Pastel2:c8")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Pastel1 As Color()
            Get
                Return Designer.GetColors("Pastel1:c9")
            End Get
        End Property
#End Region

#Region "seq"

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property OrRd As Color()
            Get
                Return Designer.GetColors("OrRd:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PuBu As Color()
            Get
                Return Designer.GetColors("PuBu:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BuPu As Color()
            Get
                Return Designer.GetColors("BuPu:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Oranges As Color()
            Get
                Return Designer.GetColors("Oranges:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BuGn As Color()
            Get
                Return Designer.GetColors("BuGn:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property YlOrBr As Color()
            Get
                Return Designer.GetColors("YlOrBr:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property YlGn As Color()
            Get
                Return Designer.GetColors("YlGn:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Reds As Color()
            Get
                Return Designer.GetColors("Reds:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RdPu As Color()
            Get
                Return Designer.GetColors("RdPu:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Greens As Color()
            Get
                Return Designer.GetColors("Greens:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property YlGnBu As Color()
            Get
                Return Designer.GetColors("YlGnBu:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Purples As Color()
            Get
                Return Designer.GetColors("Purples:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property GnBu As Color()
            Get
                Return Designer.GetColors("GnBu:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Greys As Color()
            Get
                Return Designer.GetColors("Greys:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property YlOrRd As Color()
            Get
                Return Designer.GetColors("YlOrRd:c8")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PuRd As Color()
            Get
                Return Designer.GetColors("PuRd:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Blues As Color()
            Get
                Return Designer.GetColors("Blues:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PuBuGn As Color()
            Get
                Return Designer.GetColors("PuBuGn:c9")
            End Get
        End Property
#End Region

    End Class
End Namespace

#Region "Microsoft.VisualBasic::655841810f34e7370a25b3536fb3c8ea, mime\application%pdf\PdfFileWriter\Font\WinOutlineTextMetric.vb"

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

    '   Total Lines: 184
    '    Code Lines: 69
    ' Comment Lines: 100
    '   Blank Lines: 15
    '     File Size: 5.67 KB


    ' Class WinOutlineTextMetric
    ' 
    '     Properties: otmAscent, otmDescent, otmEMSquare, otmfsSelection, otmfsType
    '                 otmItalicAngle, otmLineGap, otmMacAscent, otmMacDescent, otmMacLineGap
    '                 otmPanoseNumber, otmpFaceName, otmpFamilyName, otmpFullName, otmpStyleName
    '                 otmptSubscriptOffset, otmptSubscriptSize, otmptSuperscriptOffset, otmptSuperscriptSize, otmrcFontBox
    '                 otmsCapEmHeight, otmsCharSlopeRise, otmsCharSlopeRun, otmSize, otmsStrikeoutPosition
    '                 otmsStrikeoutSize, otmsUnderscorePosition, otmsUnderscoreSize, otmsXHeight, otmTextMetric
    '                 otmusMinimumPPEM
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

''' <summary>
''' Outline text metric class
''' </summary>
''' <remarks>
''' The OUTLINETEXTMETRIC structure contains metrics describing
''' a TrueType font.
''' </remarks>
Public Class WinOutlineTextMetric

    ''' <summary>
    ''' Outline text metric size
    ''' </summary>
    Public Property otmSize As UInteger

    ''' <summary>
    ''' Outline text metric TextMetric
    ''' </summary>
    Public Property otmTextMetric As WinTextMetric

    ''' <summary>
    ''' Outline text metric panose number
    ''' </summary>
    Public Property otmPanoseNumber As WinPanose

    ''' <summary>
    ''' Outline text metric FS selection
    ''' </summary>
    Public Property otmfsSelection As UInteger

    ''' <summary>
    ''' Outline text metric FS type
    ''' </summary>
    Public Property otmfsType As UInteger

    ''' <summary>
    ''' Outline text metric char slope rise
    ''' </summary>
    Public Property otmsCharSlopeRise As Integer

    ''' <summary>
    ''' Outline text metric char slope run
    ''' </summary>
    Public Property otmsCharSlopeRun As Integer

    ''' <summary>
    ''' Outline text metric italic angle
    ''' </summary>
    Public Property otmItalicAngle As Integer

    ''' <summary>
    ''' Outline text metric EM square
    ''' </summary>
    Public Property otmEMSquare As UInteger

    ''' <summary>
    ''' Outline text metric ascent
    ''' </summary>
    Public Property otmAscent As Integer

    ''' <summary>
    ''' Outline text metric descent
    ''' </summary>
    Public Property otmDescent As Integer
    ''' <summary>
    ''' Outline text metric line gap
    ''' </summary>
    Public Property otmLineGap As UInteger
    ''' <summary>
    ''' Outline text metric capital M height
    ''' </summary>
    Public Property otmsCapEmHeight As UInteger
    ''' <summary>
    ''' Outline text metric X height
    ''' </summary>
    Public Property otmsXHeight As UInteger
    ''' <summary>
    ''' Outline text metric Font box class
    ''' </summary>
    Public Property otmrcFontBox As FontBox
    ''' <summary>
    ''' Outline text metric Mac ascent
    ''' </summary>
    Public Property otmMacAscent As Integer
    ''' <summary>
    ''' Outline text metric Mac descent
    ''' </summary>
    Public Property otmMacDescent As Integer
    ''' <summary>
    ''' Outline text metric Mac line gap
    ''' </summary>
    Public Property otmMacLineGap As UInteger
    ''' <summary>
    ''' Outline text metric minimum PPEM
    ''' </summary>
    Public Property otmusMinimumPPEM As UInteger
    ''' <summary>
    ''' Outline text metric subscript size
    ''' </summary>
    Public Property otmptSubscriptSize As Point
    ''' <summary>
    ''' Outline text metric subscript offset
    ''' </summary>
    Public Property otmptSubscriptOffset As Point
    ''' <summary>
    ''' Outline text metric superscript size
    ''' </summary>
    Public Property otmptSuperscriptSize As Point
    ''' <summary>
    ''' Outline text metric superscript offset
    ''' </summary>
    Public Property otmptSuperscriptOffset As Point
    ''' <summary>
    ''' Outline text metric strikeout size
    ''' </summary>
    Public Property otmsStrikeoutSize As UInteger
    ''' <summary>
    ''' Outline text metric strikeout position
    ''' </summary>
    Public Property otmsStrikeoutPosition As Integer
    ''' <summary>
    ''' Outline text metric underscore size
    ''' </summary>
    Public Property otmsUnderscoreSize As Integer
    ''' <summary>
    ''' Outline text metric underscore position
    ''' </summary>
    Public Property otmsUnderscorePosition As Integer
    ''' <summary>
    ''' Outline text metric family name
    ''' </summary>
    Public Property otmpFamilyName As String
    ''' <summary>
    ''' Outline text metric face name
    ''' </summary>
    Public Property otmpFaceName As String

    ''' <summary>
    ''' Outline text metric style name
    ''' </summary>
    Public Property otmpStyleName As String

    ''' <summary>
    ''' Outline text metric full name
    ''' </summary>
    Public Property otmpFullName As String

    Friend Sub New(DC As FontApi)
        otmSize = DC.ReadUInt32()
        otmTextMetric = New WinTextMetric(DC)
        DC.Align4()
        otmPanoseNumber = New WinPanose(DC)
        DC.Align4()
        otmfsSelection = DC.ReadUInt32()
        otmfsType = DC.ReadUInt32()
        otmsCharSlopeRise = DC.ReadInt32()
        otmsCharSlopeRun = DC.ReadInt32()
        otmItalicAngle = DC.ReadInt32()
        otmEMSquare = DC.ReadUInt32()
        otmAscent = DC.ReadInt32()
        otmDescent = DC.ReadInt32()
        otmLineGap = DC.ReadUInt32()
        otmsCapEmHeight = DC.ReadUInt32()
        otmsXHeight = DC.ReadUInt32()
        otmrcFontBox = New FontBox(DC)
        otmMacAscent = DC.ReadInt32()
        otmMacDescent = DC.ReadInt32()
        otmMacLineGap = DC.ReadUInt32()
        otmusMinimumPPEM = DC.ReadUInt32()
        otmptSubscriptSize = DC.ReadWinPoint()
        otmptSubscriptOffset = DC.ReadWinPoint()
        otmptSuperscriptSize = DC.ReadWinPoint()
        otmptSuperscriptOffset = DC.ReadWinPoint()
        otmsStrikeoutSize = DC.ReadUInt32()
        otmsStrikeoutPosition = DC.ReadInt32()
        otmsUnderscoreSize = DC.ReadInt32()
        otmsUnderscorePosition = DC.ReadInt32()
        otmpFamilyName = DC.ReadString()
        otmpFaceName = DC.ReadString()
        otmpStyleName = DC.ReadString()
        otmpFullName = DC.ReadString()
    End Sub
End Class

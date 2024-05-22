#Region "Microsoft.VisualBasic::ce2496741d60c466569e840e07c9a237, mime\application%pdf\PdfFileWriter\Font\WinPanose.vb"

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

    '   Total Lines: 75
    '    Code Lines: 25 (33.33%)
    ' Comment Lines: 39 (52.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (14.67%)
    '     File Size: 1.85 KB


    ' Class WinPanose
    ' 
    '     Properties: bArmStyle, bContrast, bFamilyType, bLetterform, bMidline
    '                 bProportion, bSerifStyle, bStrokeVariation, bWeight, bXHeight
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Panose class
''' </summary>
''' <remarks>
''' The PANOSE structure describes the PANOSE font-classification
''' values for a TrueType font. These characteristics are then
''' used to associate the font with other fonts of similar
''' appearance but different names.
''' </remarks>
Public Class WinPanose

    ''' <summary>
    ''' Panose family type
    ''' </summary>
    Public Property bFamilyType As Byte

    ''' <summary>
    ''' Panose serif style
    ''' </summary>
    Public Property bSerifStyle As Byte

    ''' <summary>
    ''' Panose weight
    ''' </summary>
    Public Property bWeight As Byte

    ''' <summary>
    ''' Panose proportion
    ''' </summary>
    Public Property bProportion As Byte

    ''' <summary>
    ''' Panose contrast
    ''' </summary>
    Public Property bContrast As Byte

    ''' <summary>
    ''' Panose stroke variation
    ''' </summary>
    Public Property bStrokeVariation As Byte

    ''' <summary>
    ''' Panose arm style
    ''' </summary>
    Public Property bArmStyle As Byte

    ''' <summary>
    ''' Panose letter form
    ''' </summary>
    Public Property bLetterform As Byte

    ''' <summary>
    ''' Panose mid line
    ''' </summary>
    Public Property bMidline As Byte

    ''' <summary>
    ''' Panose X height
    ''' </summary>
    Public Property bXHeight As Byte

    Friend Sub New(DC As FontApi)
        bFamilyType = DC.ReadByte()
        bSerifStyle = DC.ReadByte()
        bWeight = DC.ReadByte()
        bProportion = DC.ReadByte()
        bContrast = DC.ReadByte()
        bStrokeVariation = DC.ReadByte()
        bArmStyle = DC.ReadByte()
        bLetterform = DC.ReadByte()
        bMidline = DC.ReadByte()
        bXHeight = DC.ReadByte()
        Return
    End Sub
End Class

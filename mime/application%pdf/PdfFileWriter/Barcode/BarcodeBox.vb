#Region "Microsoft.VisualBasic::84e6253b7ab172b64d784b1d8ee25089, mime\application%pdf\PdfFileWriter\Barcode\BarcodeBox.vb"

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

    '   Total Lines: 54
    '    Code Lines: 18
    ' Comment Lines: 31
    '   Blank Lines: 5
    '     File Size: 1.59 KB


    ' Class BarcodeBox
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Barcode box class
''' </summary>
''' <remarks>
''' The barcode box class represent the total size of the barcode
''' plus optional text. It is used by PdfTable class.
''' </remarks>
Public Class BarcodeBox
    ''' <summary>
    ''' Barcode origin X
    ''' </summary>
    Public OriginX As Double

    ''' <summary>
    ''' Barcode origin Y
    ''' </summary>
    Public OriginY As Double

    ''' <summary>
    ''' Total width including optional text.
    ''' </summary>
    Public TotalWidth As Double

    ''' <summary>
    ''' Total height including optional text.
    ''' </summary>
    Public TotalHeight As Double

    ''' <summary>
    ''' Constructor for no text
    ''' </summary>
    ''' <param name="TotalWidth">Total width</param>
    ''' <param name="TotalHeight">Total height</param>
    Public Sub New(TotalWidth As Double, TotalHeight As Double)
        Me.TotalWidth = TotalWidth
        Me.TotalHeight = TotalHeight
        Return
    End Sub

    ''' <summary>
    ''' Constructor for text included
    ''' </summary>
    ''' <param name="OriginX">Barcode origin X</param>
    ''' <param name="OriginY">Barcode origin Y</param>
    ''' <param name="TotalWidth">Total width</param>
    ''' <param name="TotalHeight">Total height</param>
    Public Sub New(OriginX As Double, OriginY As Double, TotalWidth As Double, TotalHeight As Double)
        Me.OriginX = OriginX
        Me.OriginY = OriginY
        Me.TotalWidth = TotalWidth
        Me.TotalHeight = TotalHeight
        Return
    End Sub
End Class

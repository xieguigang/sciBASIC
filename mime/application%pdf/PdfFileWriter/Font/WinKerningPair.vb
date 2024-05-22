#Region "Microsoft.VisualBasic::ca3512461bf42e0d47f98332b399dadc, mime\application%pdf\PdfFileWriter\Font\WinKerningPair.vb"

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

    '   Total Lines: 47
    '    Code Lines: 19 (40.43%)
    ' Comment Lines: 22 (46.81%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (12.77%)
    '     File Size: 1.37 KB


    ' Class WinKerningPair
    ' 
    '     Properties: First, KernAmount, Second
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: CompareTo
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Kerning pair class
''' </summary>
Public Class WinKerningPair : Implements IComparable(Of WinKerningPair)

    ''' <summary>
    ''' Gets first character
    ''' </summary>
    Public Property First As Char

    ''' <summary>
    ''' Gets second character
    ''' </summary>
    Public Property Second As Char

    ''' <summary>
    ''' Gets kerning amount in design units
    ''' </summary>
    Public Property KernAmount As Integer

    Friend Sub New(DC As FontApi)
        First = DC.ReadChar()
        Second = DC.ReadChar()
        KernAmount = DC.ReadInt32()
        Return
    End Sub

    ''' <summary>
    ''' Kerning pair constructor
    ''' </summary>
    ''' <param name="First">First character</param>
    ''' <param name="Second">Second character</param>
    Public Sub New(First As Char, Second As Char)
        Me.First = First
        Me.Second = Second
        Return
    End Sub

    ''' <summary>
    ''' Compare kerning pairs
    ''' </summary>
    ''' <param name="Other">Other pair</param>
    ''' <returns>Compare result</returns>
    Public Function CompareTo(Other As WinKerningPair) As Integer Implements IComparable(Of WinKerningPair).CompareTo
        Return If(First <> Other.First, AscW(First) - AscW(Other.First), AscW(Second) - AscW(Other.Second))
    End Function
End Class

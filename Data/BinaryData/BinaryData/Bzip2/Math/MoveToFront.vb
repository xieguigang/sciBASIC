#Region "Microsoft.VisualBasic::630b9b0ad2d45201a39f301719f51855, Data\BinaryData\BinaryData\Bzip2\Math\MoveToFront.vb"

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

    '   Total Lines: 58
    '    Code Lines: 35 (60.34%)
    ' Comment Lines: 15 (25.86%)
    '    - Xml Docs: 73.33%
    ' 
    '   Blank Lines: 8 (13.79%)
    '     File Size: 1.95 KB


    '     Class MoveToFront
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: IndexToFront, ValueToFront
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports System

Namespace Bzip2.Math
    ''' <summary>
    ''' A 256 entry Move To Front transform
    ''' </summary>
    Friend Class MoveToFront
#Region "Private fields"
        ''' <summary>The Move To Front list</summary> 
        Private ReadOnly mtf As Byte()
#End Region

#Region "Public methods"
        ''' <summary>Public constructor</summary>
        Public Sub New()
            mtf = New Byte(255) {}

            For i = 0 To 256 - 1
                mtf(i) = CByte(i)
            Next
        End Sub

        ''' <summary>Moves a value to the head of the MTF list (forward Move To Front transform)</summary>
        ''' <param name="value">The value to move</param>
        ''' <return>The position the value moved from</return>
        Public Function ValueToFront(value As Byte) As Integer
            Dim index = 0
            Dim temp = mtf(0)
            If value = temp Then Return index
            mtf(0) = value

            While temp <> value
                index += 1
                Dim temp2 = temp
                temp = mtf(index)
                mtf(index) = temp2
            End While

            Return index
        End Function

        ''' <summary>Gets the value from a given index and moves it to the front of the MTF list (inverse Move To Front transform)</summary>
        ''' <param name="index">The index to move</param>
        ''' <return>The value at the given index</return>
        Public Function IndexToFront(index As Integer) As Byte
            Dim value = mtf(index)
            Array.ConstrainedCopy(mtf, 0, mtf, 1, index)
            mtf(0) = value
            Return value
        End Function
#End Region
    End Class
End Namespace

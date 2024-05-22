#Region "Microsoft.VisualBasic::e9fe436ed8a291e398486aa4aae39493, Data_science\DataMining\DataMining\ComponentModel\Encoder\Variable\Binary.vb"

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
    '    Code Lines: 23 (48.94%)
    ' Comment Lines: 18 (38.30%)
    '    - Xml Docs: 72.22%
    ' 
    '   Blank Lines: 6 (12.77%)
    '     File Size: 1.47 KB


    '     Class Binary
    ' 
    '         Properties: id
    ' 
    '         Function: ContingencyDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel.Encoder.Variable

    Public Class Binary : Inherits EntityBase(Of Boolean)
        Implements INamedValue

        Public Property id As String Implements INamedValue.Key

        ''' <summary>
        ''' Contingency table of two binary variable:
        ''' 
        ''' ```
        '''        _____Y___________
        '''    ___|__1__|__0__|_sum_|
        '''   |1  | a   | b   | a+b |
        ''' X |0  | c   | d   | c+d |
        '''   |sum| a+c | b+d | p   |
        ''' ```
        ''' </summary>
        ''' <param name="X"></param>
        ''' <param name="Y"></param>
        ''' <returns></returns>
        Public Shared Function ContingencyDistance(X As Binary, Y As Binary) As Double
            Dim a, b, c, d As Integer
            Dim nsize As Integer = X.Length

            For i As Integer = 0 To nsize - 1
                If X(i) AndAlso Y(i) Then
                    ' 11
                    a += 1
                ElseIf X(i) AndAlso Not Y(i) Then
                    ' 10
                    b += 1
                ElseIf (Not X(i)) AndAlso Y(i) Then
                    ' 01
                    c += 1
                Else
                    ' 00
                    d += 1
                End If
            Next

            Return (b + c) / (a + b + c)
        End Function
    End Class
End Namespace

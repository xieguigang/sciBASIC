#Region "Microsoft.VisualBasic::904cb0bd632364e5046c34dbbe499738, Data_science\Mathematica\Math\Gibbs\Score.vb"

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

    '   Total Lines: 17
    '    Code Lines: 13 (76.47%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (23.53%)
    '     File Size: 373 B


    ' Class Score
    ' 
    '     Properties: len, pwm, seq, start
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Public Class Score

    Public ReadOnly Property pwm As String
        Get
            Return Mid(seq, start + 1, len)
        End Get
    End Property

    Public Property seq As String
    Public Property start As Integer
    Public Property len As Integer

    Public Overrides Function ToString() As String
        Return pwm
    End Function

End Class

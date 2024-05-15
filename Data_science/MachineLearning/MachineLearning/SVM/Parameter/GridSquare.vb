#Region "Microsoft.VisualBasic::82425df8f9ac5aed02641be5d166f397, Data_science\MachineLearning\MachineLearning\SVM\Parameter\GridSquare.vb"

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

    '   Total Lines: 25
    '    Code Lines: 10
    ' Comment Lines: 12
    '   Blank Lines: 3
    '     File Size: 638 B


    '     Class GridSquare
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVM

    ''' <summary>
    ''' Class representing a grid square result.
    ''' </summary>
    Public Class GridSquare

        ''' <summary>
        ''' The C value
        ''' </summary>
        Public C As Double
        ''' <summary>
        ''' The Gamma value
        ''' </summary>
        Public Gamma As Double
        ''' <summary>
        ''' The cross validation score
        ''' </summary>
        Public Score As Double

        Public Overrides Function ToString() As String
            Return String.Format("{0} {1} {2}", C, Gamma, Score)
        End Function
    End Class
End Namespace

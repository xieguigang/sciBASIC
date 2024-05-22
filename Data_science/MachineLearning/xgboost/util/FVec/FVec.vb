#Region "Microsoft.VisualBasic::2c3c8c0f39eaa7133a69d9bae7c9dc5a, Data_science\MachineLearning\xgboost\util\FVec\FVec.vb"

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
    '    Code Lines: 5 (29.41%)
    ' Comment Lines: 8 (47.06%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (23.53%)
    '     File Size: 383 B


    '     Interface FVec
    ' 
    '         Function: fvalue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace util

    ''' <summary>
    ''' Interface of feature vector.
    ''' </summary>
    Public Interface FVec

        ''' <summary>
        ''' Gets index-th value.
        ''' </summary>
        ''' <param name="index"> index </param>
        ''' <returns> value </returns>
        Function fvalue(index As Integer) As Double

    End Interface

End Namespace

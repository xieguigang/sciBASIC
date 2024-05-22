#Region "Microsoft.VisualBasic::01b0c2f023e1060f629626119189e49a, Data_science\Mathematica\Math\Math\Algebra\Polynomial\Formula.vb"

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

    '   Total Lines: 22
    '    Code Lines: 13 (59.09%)
    ' Comment Lines: 4 (18.18%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (22.73%)
    '     File Size: 827 B


    '     Class Formula
    ' 
    '         Properties: Factors
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace LinearAlgebra

    Public MustInherit Class Formula

        ''' <summary>
        ''' 多项式系数向量
        ''' </summary>
        ''' <returns></returns>
        Public Property Factors As Double()

        Public MustOverride Function Evaluate(ParamArray x As Double()) As Double
        Public MustOverride Overloads Function ToString(format As String, Optional html As Boolean = False) As String
        Public MustOverride Overloads Function ToString(variables As String(), format As String, Optional html As Boolean = False) As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return ToString(format:="G3")
        End Function
    End Class
End Namespace

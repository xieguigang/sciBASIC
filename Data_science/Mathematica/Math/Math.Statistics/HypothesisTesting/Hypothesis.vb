#Region "Microsoft.VisualBasic::5e52fb923691786f6ed99bbb88af77c4, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\Hypothesis.vb"

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

'     Enum Hypothesis
' 
'         Greater, Less, TwoSided
' 
'  
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace Hypothesis

    Public Enum Hypothesis
        ''' <summary>
        ''' ``mu > mu0``
        ''' </summary>
        <Description("greater than")> Greater = 1
        ''' <summary>
        ''' ``mu &lt; mu0``
        ''' </summary>
        <Description("less than")> Less = -1
        ''' <summary>
        ''' not equals ``mu &lt;> mu0``
        ''' </summary>
        <Description("not equal to")> TwoSided = 0
    End Enum
End Namespace

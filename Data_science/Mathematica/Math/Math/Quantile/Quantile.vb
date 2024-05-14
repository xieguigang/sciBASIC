#Region "Microsoft.VisualBasic::f4fdb039183b9a69943bf62d88e05470, Data_science\Mathematica\Math\Math\Quantile\Quantile.vb"

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

    '   Total Lines: 34
    '    Code Lines: 14
    ' Comment Lines: 15
    '   Blank Lines: 5
    '     File Size: 1.16 KB


    '     Structure Point
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
'   Copyright 2012 Andrew Wang (andrew@umbrant.com)
'
'   Licensed under the Apache License, Version 2.0 (the "License");
'   you may not use this file except in compliance with the License.
'   You may obtain a copy of the License at
'
'       http://www.apache.org/licenses/LICENSE-2.0
'
'   Unless required by applicable law or agreed to in writing, software
'   distributed under the License is distributed on an "AS IS" BASIS,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'   See the License for the specific language governing permissions and
'   limitations under the License.
'

Namespace Quantile

    Public Structure Point

        Public ReadOnly quantile#, error#, u#, v#

        Public Sub New(quantile#, error#)
            Me.quantile = quantile
            Me.error = [error]
            Me.u = 2.0 * [error] / (1.0 - quantile)
            Me.v = 2.0 * [error] / quantile
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("Q{{q={0:F3}, eps={1:F3}}})", quantile, [error])
        End Function
    End Structure
End Namespace

#Region "Microsoft.VisualBasic::4691e1bc7df46f9cdd85facdf8f52039, Data_science\DataMining\HMM\Models\Alpha.vb"

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

    '   Total Lines: 12
    '    Code Lines: 6
    ' Comment Lines: 3
    '   Blank Lines: 3
    '     File Size: 249 B


    '     Class Alpha
    ' 
    '         Properties: alphaF, alphas
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Models

    ''' <summary>
    ''' alpha and alpha matrix
    ''' </summary>
    Public Class Alpha

        Public Property alphaF As Double
        Public Property alphas As List(Of List(Of Double))

    End Class
End Namespace

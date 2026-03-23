#Region "Microsoft.VisualBasic::333f36ad900b2fd75705bf92e57e7080, Data_science\DataMining\HMM\Models\Objects.vb"

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

    '   Total Lines: 55
    '    Code Lines: 26 (47.27%)
    ' Comment Lines: 14 (25.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (27.27%)
    '     File Size: 1.36 KB


    '     Class StatesObject
    ' 
    '         Properties: prob, state
    ' 
    '         Function: ToString
    ' 
    '     Class Observable
    ' 
    '         Properties: obs, prob
    ' 
    '         Function: ToString
    ' 
    '     Class Alpha
    ' 
    '         Properties: alphaF, alphas
    ' 
    '     Class Beta
    ' 
    '         Properties: betaF, betas
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

Namespace Models

    Public Class StatesObject

        ''' <summary>
        ''' the state name
        ''' </summary>
        ''' <returns></returns>
        Public Property state As String
        Public Property prob As Double()

        Public Overrides Function ToString() As String
            Return $"{state}: {prob.Select(Function(d) std.Round(d, 3)).ToArray.GetJson}"
        End Function

    End Class

    Public Class Observable

        ''' <summary>
        ''' the observed state name
        ''' </summary>
        ''' <returns></returns>
        Public Property obs As String
        Public Property prob As Double()

        Public Overrides Function ToString() As String
            Return $"{obs}: {prob.Select(Function(d) std.Round(d, 3)).ToArray.GetJson}"
        End Function

    End Class

    ''' <summary>
    ''' alpha and alpha matrix
    ''' </summary>
    Public Class Alpha

        Public Property alphaF As Double
        Public Property alphas As List(Of List(Of Double))

    End Class

    ''' <summary>
    ''' beta and beta matrix
    ''' </summary>
    Public Class Beta

        Public Property betas As Double()()
        Public Property betaF As Double

    End Class
End Namespace

#Region "Microsoft.VisualBasic::4fc00f25d57e8bb1b5f847a4a897cc01, Microsoft.VisualBasic.Core\src\Extensions\Math\Correlations\Scripting.vb"

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
    '    Code Lines: 15 (88.24%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (11.76%)
    '     File Size: 592 B


    '     Module Scripting
    ' 
    '         Function: GetComputeAPI
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Math.Correlations

    <HideModuleName>
    Public Module Scripting

        Public Function GetComputeAPI(name As String) As ICorrelation
            Select Case name.ToLower
                Case "pearson"
                    Return AddressOf Correlations.GetPearson
                Case "spearman"
                    Return AddressOf Correlations.Spearman
                Case Else
                    Throw New NotImplementedException($"Method `{name}` is not implemented or support yet!")
            End Select
        End Function
    End Module

    Public Class CorrelationNetwork

        Public Property u As String
        Public Property v As String
        Public Property cor As Double
        Public Property pvalue As Double
        Public Property z As Double

    End Class
End Namespace

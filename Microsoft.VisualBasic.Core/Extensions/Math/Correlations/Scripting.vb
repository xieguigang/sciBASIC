#Region "Microsoft.VisualBasic::b59e639b3e0dcc39165975e51b39fbb9, Microsoft.VisualBasic.Core\Extensions\Math\Correlations\Scripting.vb"

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
End Namespace

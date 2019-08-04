#Region "Microsoft.VisualBasic::b55d93a32664c4d60521f303fd95bd9d, Data_science\Darwinism\NonlinearGrid\TopologyInference\Storage\GridMatrix.vb"

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

    ' Class GridMatrix
    ' 
    '     Properties: [const], [error], correlations, direction, samples
    ' 
    '     Function: CreateSystem, ToString
    ' 
    ' Class Constants
    ' 
    '     Properties: A, Amplify, B, Delay
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Class GridMatrix : Inherits XmlDataModel

    Public Property [error] As Double

    Public Property direction As NumericVector
    Public Property [const] As Constants

    <XmlElement("correlations")>
    Public Property correlations As NumericVector()
    ''' <summary>
    ''' 训练这个网格模型所使用的样本的数据分布矩阵
    ''' </summary>
    ''' <returns></returns>
    Public Property samples As NormalizeMatrix

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CreateSystem() As GridSystem
        Return New GridSystem With {
            .A = direction.vector,
            .C = correlations _
                .Select(Function(r, i)
                            Return New Correlation With {
                                .B = r.vector,
                                .BC = If([const] Is Nothing, 0, [const].B(i))
                            }
                        End Function) _
                .ToArray,
            .AC = If([const] Is Nothing, 0, [const].A),
            .Amplify = [const].Amplify,
            .delay = [const].Delay
        }
    End Function

    Public Overrides Function ToString() As String
        Return ToString(Languages.VisualBasic)
    End Function

End Class

Public Class Constants
    Public Property A As Double
    Public Property B As NumericVector
    Public Property Amplify As Double
    Public Property Delay As Double
End Class

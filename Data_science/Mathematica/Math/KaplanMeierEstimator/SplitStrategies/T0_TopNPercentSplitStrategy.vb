#Region "Microsoft.VisualBasic::04ce934598f3f075aa23b08da857d882, Data_science\Mathematica\Math\KaplanMeierEstimator\SplitStrategies\T0_TopNPercentSplitStrategy.vb"

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

    '   Total Lines: 43
    '    Code Lines: 34
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.93 KB


    '     Class T0_TopNPercentSplitStrategy
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: DoSplit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Math.KaplanMeierEstimator.Models

Namespace SplitStrategies
    Public Class T0_TopNPercentSplitStrategy
        Implements ISplitStrategy
        Private ReadOnly m_percent As Integer
        Private ReadOnly m_patients As IDictionary(Of Integer, Patient)

        Public Sub New(ByVal percent As Integer, ByVal patients As IDictionary(Of Integer, Patient))
            If percent <= 0 OrElse percent > 50 Then
                Throw New ArgumentOutOfRangeException("percent")
            End If

            If patients Is Nothing Then
                Throw New ArgumentNullException("patients")
            End If

            m_percent = percent
            m_patients = patients
        End Sub

        Public Sub DoSplit(ByVal genes As IEnumerable(Of GeneExpression), <Out> ByRef groupA As IEnumerable(Of Patient), <Out> ByRef groupB As IEnumerable(Of Patient)) Implements ISplitStrategy.DoSplit
            Dim relevantGenes = genes.Where(Function(gene) Not Double.IsNaN(gene.Before))
            relevantGenes = relevantGenes.Where(Function(gene) m_patients.ContainsKey(gene.PatientId))
            Dim orderedGenes = relevantGenes.OrderBy(Function(gene) gene.Before)

            Dim groupSize As Integer = CInt((orderedGenes.Count() * (m_percent / 100.0)))

            Dim groupAGenes = orderedGenes.Take(groupSize)
            Dim groupBGenes = orderedGenes.Skip(orderedGenes.Count() - groupSize)

            groupA = groupAGenes.[Select](Function(gene) m_patients(gene.PatientId)).ToList()
            groupB = groupBGenes.[Select](Function(gene) m_patients(gene.PatientId)).ToList()
        End Sub

        Public ReadOnly Property Name As String Implements ISplitStrategy.Name
            Get
                Return "T0_Top" & m_percent
            End Get
        End Property
    End Class
End Namespace

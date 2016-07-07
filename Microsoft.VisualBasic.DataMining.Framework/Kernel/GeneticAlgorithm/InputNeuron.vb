#Region "Microsoft.VisualBasic::13ce4da6f6a65eead8bd8754cca61fed, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.DataMining.Framework\Kernel\GeneticAlgorithm\InputNeuron.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Kernel.GeneticAlgorithm
    ''' <summary>
    ''' The math neurons actually never see the real input value, they only get a value
    ''' from another neuron. This neuron provides the user input value to those neurons.
    ''' </summary>
    Public NotInheritable Class InputNeuron
        Implements INeuron
        Public Shared ReadOnly Instance As New InputNeuron()

        Public ReadOnly Property Complexity() As Integer Implements INeuron.Complexity
            Get
                Return 1
            End Get
        End Property

        Public Function Execute(input As Integer) As System.Nullable(Of Integer) Implements INeuron.Execute
            Return input
        End Function

        Public Function Reproduce() As INeuron Implements INeuron.Reproduce
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return "x"
        End Function
    End Class
End Namespace

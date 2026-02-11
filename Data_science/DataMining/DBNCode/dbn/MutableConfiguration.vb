#Region "Microsoft.VisualBasic::ef531feb2f06454cb463d08d553ad5b9, Data_science\DataMining\DBNCode\dbn\MutableConfiguration.vb"

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

    '   Total Lines: 54
    '    Code Lines: 34 (62.96%)
    ' Comment Lines: 12 (22.22%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 8 (14.81%)
    '     File Size: 2.08 KB


    '     Class MutableConfiguration
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: applyMask
    ' 
    '         Sub: update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace dbn

    ''' <summary>
    ''' Provides network configurations that can be changed, for assisting the
    ''' process of sampling new observations.
    ''' </summary>
    ''' <seealsocref="BayesNet"/>
    '''  </seealso>
    Public Class MutableConfiguration
        Inherits Configuration

        Public Sub New(attributes As IList(Of Attribute), markovLag As Integer, extendedObservation As Integer())
            MyBase.New(attributes, markovLag)
            reset()
            If extendedObservation IsNot Nothing Then
                Array.Copy(extendedObservation, 0, configuration, 0, extendedObservation.Length)
            End If
        End Sub

        ''' 
        ''' <param name="parentNodes">
        '''            must be sorted </param>
        ''' <param name="childNode">
        '''            must be in range [0,attributes.size()[ </param>
        Public Overridable Function applyMask(parentNodes As IList(Of Integer), childNode As Integer) As Configuration
            Dim n = attributes.Count
            Dim size = configuration.Length
            Dim newConfiguration = New Integer(size - 1) {}

            Dim numParents = parentNodes.Count
            Dim currentParent = 0

            For i = 0 To size - 1
                If i = childNode + n * markovLag Then
                    newConfiguration(i) = 0
                ElseIf currentParent < numParents AndAlso i = parentNodes(currentParent) Then
                    newConfiguration(i) = configuration(i)
                    currentParent += 1
                Else
                    newConfiguration(i) = -1
                End If
            Next

            Return New Configuration(attributes, newConfiguration, markovLag, childNode)
        End Function

        Public Overridable Sub update(node As Integer, value As Integer)
            ' TODO: validate node and value bounds
            Dim n = attributes.Count
            configuration(node + n * markovLag) = value
        End Sub
    End Class

End Namespace


#Region "Microsoft.VisualBasic::deccc2f6b362f0a7b47a55a17e00ee00, Data_science\MachineLearning\DeepLearning\NeuralNetwork\NetworkViews.vb"

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

    '   Total Lines: 108
    '    Code Lines: 55 (50.93%)
    ' Comment Lines: 38 (35.19%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (13.89%)
    '     File Size: 4.24 KB


    '     Class NetworkLayerView
    ' 
    '         Properties: Count, Output
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class HiddenLayersView
    ' 
    '         Properties: Count, Layers
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Linq

Namespace NeuralNetwork

    ''' <summary>
    ''' Read-only view of a single neural network layer (input, hidden or output).
    ''' </summary>
    ''' <remarks>
    ''' The view does not depend on any legacy compute type (Layer/Neuron/Synapse). Its size
    ''' (<see cref="Count"/>) and last activation vector (<see cref="Output"/>) are derived from the CNN
    ''' kernel, and it backs the <see cref="Network.InputLayer"/>, <see cref="Network.HiddenLayer"/> and
    ''' <see cref="Network.OutputLayer"/> properties.
    ''' </remarks>
    Public Class NetworkLayerView

        Private ReadOnly m_count As Integer
        Private m_output As Double()

        ''' <summary>Number of neuron nodes in this layer.</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return m_count
            End Get
        End Property

        ''' <summary>
        ''' The activation vector produced by the most recent forward pass of this layer (a read-only
        ''' mirror derived from the CNN kernel).
        ''' </summary>
        Public Property Output As Double()
            Get
                Return m_output
            End Get
            Friend Set(value As Double())
                m_output = value
            End Set
        End Property

        ''' <summary>
        ''' Creates a layer view with the given neuron count.
        ''' </summary>
        ''' <param name="count">Number of neuron nodes in the layer.</param>
        Sub New(count As Integer)
            m_count = count
            m_output = New Double(System.Math.Max(count, 1) - 1) {}
        End Sub

        ''' <summary>Returns a short description of the layer size.</summary>
        ''' <returns>A text of the form <c>layer with N neurons</c>.</returns>
        Public Overrides Function ToString() As String
            Return $"layer with {m_count} neurons"
        End Function
    End Class

    ''' <summary>
    ''' Read-only view over all hidden layers of a network.
    ''' </summary>
    ''' <remarks>
    ''' The legacy <c>HiddenLayers</c> interface is preserved through <see cref="Count"/> and the indexer,
    ''' but the view no longer maintains a Layer/Neuron/Synapse data graph internally.
    ''' </remarks>
    Public Class HiddenLayersView : Implements IEnumerable(Of NetworkLayerView)

        Private ReadOnly m_layers As NetworkLayerView()

        ''' <summary>Number of hidden layers.</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return m_layers.Length
            End Get
        End Property

        ''' <summary>The hidden layer views.</summary>
        Public ReadOnly Property Layers As NetworkLayerView()
            Get
                Return m_layers
            End Get
        End Property

        ''' <summary>Gets the hidden layer view at the given index.</summary>
        ''' <param name="index">Zero based index of the hidden layer.</param>
        ''' <returns>The hidden layer view stored at <paramref name="index"/>.</returns>
        Default Public ReadOnly Property Item(index%) As NetworkLayerView
            Get
                Return m_layers(index)
            End Get
        End Property

        ''' <summary>
        ''' Creates the hidden layer collection view.
        ''' </summary>
        ''' <param name="layers">The hidden layer views, materialized into an array.</param>
        Sub New(layers As IEnumerable(Of NetworkLayerView))
            m_layers = layers.ToArray
        End Sub

        ''' <summary>Enumerates the hidden layer views.</summary>
        ''' <returns>An enumerator over the hidden layers.</returns>
        Public Function GetEnumerator() As IEnumerator(Of NetworkLayerView) Implements IEnumerable(Of NetworkLayerView).GetEnumerator
            Return m_layers.AsEnumerable.GetEnumerator()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return m_layers.GetEnumerator()
        End Function
    End Class
End Namespace

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

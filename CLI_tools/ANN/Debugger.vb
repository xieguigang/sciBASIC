Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MIME.application.netCDF

Module Debugger

    Public Sub WriteCDF(network As Network,
                        fileSave$,
                        synapses As Synapse(),
                        errors As List(Of Double),
                        index As List(Of Integer),
                        synapsesWeights As Dictionary(Of String, List(Of Double)))

        Using debugger As New CDFWriter(fileSave)
            Dim attrs = {
                 New Components.attribute With {.name = "Date", .type = CDFDataTypes.CHAR, .value = Now.ToString},
                 New Components.attribute With {.name = "input_layer", .type = CDFDataTypes.CHAR, .value = network.InputLayer.Neurons.Length},
                 New Components.attribute With {.name = "output_layer", .type = CDFDataTypes.CHAR, .value = network.OutputLayer.Neurons.Length},
                 New Components.attribute With {.name = "hidden_layers", .type = CDFDataTypes.CHAR, .value = network.HiddenLayer.Select(Function(l) l.Neurons.Length).JoinBy(", ")},
                 New Components.attribute With {.name = "synapse_edges", .type = CDFDataTypes.CHAR, .value = synapses.Length},
                 New Components.attribute With {.name = "times", .type = CDFDataTypes.CHAR, .value = App.ElapsedMilliseconds},
                 New Components.attribute With {.name = "ANN", .type = CDFDataTypes.CHAR, .value = network.GetType.FullName}
            }
            Dim dimensions = {
                New Components.Dimension With {.name = "index_number", .size = 4},
                New Components.Dimension With {.name = GetType(Double).FullName, .size = 8},
                New Components.Dimension With {.name = GetType(String).FullName, .size = 1024}
            }
            Dim inputLayer = network.InputLayer.Neurons.Select(Function(n) n.Guid).Indexing
            Dim outputLayer = network.OutputLayer.Neurons.Select(Function(n) n.Guid).Indexing
            Dim hiddens As New List(Of SeqValue(Of Index(Of String)))

            For Each layer In network.HiddenLayer.SeqIterator
                hiddens.Add(New SeqValue(Of Index(Of String)) With {.i = layer, .value = layer.value.Neurons.Select(Function(n) n.Guid).Indexing})
            Next

            Dim getLocation = Function(guid As String) As String
                                  If inputLayer.IndexOf(guid) > -1 Then
                                      Return "in"
                                  ElseIf outputLayer.IndexOf(guid) > -1 Then
                                      Return "out"
                                  Else
                                      For Each layer In hiddens
                                          If layer.value.IndexOf(guid) > -1 Then
                                              Return $"hiddens-{layer.i}"
                                          End If
                                      Next
                                  End If

                                  Return "NA"
                              End Function

            debugger.GlobalAttributes(attrs).Dimensions(dimensions)

            Call debugger.AddVariable("iterations", index.ToArray, {"index_number"})
            Call debugger.AddVariable("fitness", errors.ToArray, {GetType(Double).FullName})

            For Each s In synapses
                attrs = {
                    New Components.attribute With {.name = "input", .type = CDFDataTypes.CHAR, .value = s.InputNeuron.Guid},
                    New Components.attribute With {.name = "output", .type = CDFDataTypes.CHAR, .value = s.OutputNeuron.Guid},
                    New Components.attribute With {.name = "input_location", .type = CDFDataTypes.CHAR, .value = getLocation(s.InputNeuron.Guid)},
                    New Components.attribute With {.name = "output_location", .type = CDFDataTypes.CHAR, .value = getLocation(s.OutputNeuron.Guid)}
                }
                debugger.AddVariable(s.ToString, synapsesWeights(s.ToString).ToArray, {GetType(Double).FullName}, attrs)
            Next
        End Using
    End Sub
End Module

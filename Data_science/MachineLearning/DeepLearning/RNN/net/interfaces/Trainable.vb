Namespace RNN
    ' Trainable neural network.
    Public Interface Trainable

        ' 
        ' 			Performs a forward-backward pass for the given indices.
        ' 	
        ' 			ix.length and iy.length lengths must match.
        ' 			All indices must be less than the vocabulary size.
        ' 	
        ' 			Returns the cross-entropy loss.
        ' 		
        Function forwardBackward(ix As Integer(), iy As Integer()) As Double
    End Interface


End Namespace
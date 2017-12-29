Imports Microsoft.VisualBasic.MachineLearning.Darwinism
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF.Abstract

    Public Class ParameterVector(Of T As ParameterVector(Of T))
        Implements Chromosome(Of ParameterVector(Of T)), ICloneable
        Implements IIndividual

    End Class
End Namespace
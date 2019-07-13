#Region "Microsoft.VisualBasic::a7777fd874b89d14b530350b76b58d9a, Data_science\MachineLearning\Bootstrapping\Darwinism\GAF\ODEs\GAFModel.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Model
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class RefModel
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Math.Calculus

Namespace Darwinism.GAF.ODEs

    Public MustInherit Class Model : Inherits MonteCarlo.Model

        Sub New()
            Call MyBase.New()
        End Sub

        Protected Sub New(vars As var())
            Call MyBase.New(vars)
        End Sub

#Region "Not Required"
        Public NotOverridable Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
            Return Nothing
        End Function

        Public NotOverridable Overrides Function params() As ValueRange()
            Return Nothing
        End Function

        Public NotOverridable Overrides Function yinit() As ValueRange()
            Return Nothing
        End Function
#End Region

    End Class

    Public MustInherit Class RefModel : Inherits MonteCarlo.RefModel

#Region "Not Required"
        Public NotOverridable Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
            Return Nothing
        End Function

        Public NotOverridable Overrides Function params() As ValueRange()
            Return Nothing
        End Function

        Public NotOverridable Overrides Function yinit() As ValueRange()
            Return Nothing
        End Function
#End Region

    End Class
End Namespace

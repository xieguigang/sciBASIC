Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Analysis
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.Calculus

''' <summary>
''' ##### Kinetics of influenza A virus infection in humans
'''
''' > **DOI** 10.3390/v7102875
''' </summary>
''' <remarks>假设为实验观测数据</remarks>
Public Class Kinetics_of_influenza_A_virus_infection_in_humans : Inherits ODEs

    Dim T As var
    Dim I As var
    Dim V As var

    Dim p As Double = 3 * 10 ^ -2
    Dim c As Double = 2
    Dim beta As Double = 8.8 * 10 ^ -6
    Dim delta As Double = 2.6

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        dy(T) = -beta * T * V
        dy(I) = beta * T * V - delta * I
        dy(V) = p * I - c * V
    End Sub

    Protected Overrides Function y0() As var()
        Return {
            V = 1.4 * 10 ^ -2,
            T = 4 * 10 ^ 8,
            I = 0
        }
    End Function
End Class

Public Class Kinetics_of_influenza_A_virus_infection_in_humans_Model : Inherits MonteCarlo.Model

    Dim T As var
    Dim I As var
    Dim V As var

    Dim p As Double = Integer.MaxValue
    Dim c As Double = Integer.MaxValue
    Dim beta As Double = Integer.MaxValue
    Dim delta As Double = Integer.MaxValue

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        dy(T) = -beta * T * V
        dy(I) = beta * T * V - delta * I
        dy(V) = p * I - c * V
    End Sub

    Public Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
        Return Nothing
    End Function

    Public Overrides Function params() As MonteCarlo.VariableModel()
        Return {
            New MonteCarlo.VariableModel(NameOf(p), 0.000000000000001, 1000),
            New MonteCarlo.VariableModel(NameOf(c), 0, 1000),
            New MonteCarlo.VariableModel(NameOf(beta), 0.00000000000001, 1000),
            New MonteCarlo.VariableModel(NameOf(delta), 0, 1000)
        }
    End Function

    Public Overrides Function yinit() As MonteCarlo.VariableModel()
        Return {
            New MonteCarlo.VariableModel(NameOf(V), 0.0000000000001, 1000.0),
            New MonteCarlo.VariableModel(NameOf(T), 0, 1.0E+20),
            New MonteCarlo.VariableModel(NameOf(I), 0, 1000000)
        }
    End Function
End Class
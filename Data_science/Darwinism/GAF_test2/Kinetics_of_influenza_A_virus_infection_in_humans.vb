#Region "Microsoft.VisualBasic::ab1854e7b35f4e1435cce11a1418628b, Data_science\Darwinism\GAF_test2\Kinetics_of_influenza_A_virus_infection_in_humans.vb"

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

    ' Class Kinetics_of_influenza_A_virus_infection_in_humans
    ' 
    '     Function: y0
    ' 
    '     Sub: func
    ' 
    ' Class Kinetics_of_influenza_A_virus_infection_in_humans_Model
    ' 
    '     Sub: func
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping.Darwinism
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.LinearAlgebra

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

    Const p# = 3 * 10 ^ -2
    Const c# = 2
    Const beta# = 8.8 * 10 ^ -6
    Const delta# = 2.6

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

Public Class Kinetics_of_influenza_A_virus_infection_in_humans_Model : Inherits GAF.ODEs.Model

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
End Class

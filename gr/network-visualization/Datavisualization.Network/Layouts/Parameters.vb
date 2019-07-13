#Region "Microsoft.VisualBasic::e7683e904bbd6bef66ef310c4d0662bc, gr\network-visualization\Datavisualization.Network\Layouts\Parameters.vb"

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

    '     Class Parameters
    ' 
    '         Properties: ForceDirectedArgs
    ' 
    '         Function: Load
    ' 
    '     Class ForceDirectedArgs
    ' 
    '         Properties: Damping, Iterations, Repulsion, Stiffness
    ' 
    '         Function: DefaultNew, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Layouts

    ''' <summary>
    ''' The network graph layout parameters
    ''' </summary>
    <IniMapIO("@" & Parameters.DefaultFileName)>
    Public Class Parameters

        Public Property ForceDirectedArgs As ForceDirectedArgs

        Public Const DefaultStiffness As Single = 81.76!
        Public Const DefaultRepulsion As Single = 2000.0!
        Public Const DefaultDamping As Single = 0.5!

        Public Const DefaultFileName$ = "ForceDirectedArgs.ini"

        Public Shared Function Load(Optional out$ = Nothing) As ForceDirectedArgs
            Dim b As Boolean = False
            Dim ini As Parameters = LoadProfile(Of Parameters)(b)

            If Not b Then
                ini.ForceDirectedArgs = New ForceDirectedArgs With {
                    .Damping = DefaultDamping,
                    .Repulsion = DefaultRepulsion,
                    .Stiffness = DefaultStiffness
                }
                If out.StringEmpty Then
                    Call ini.WriteProfile
                Else
                    Call ini.WriteProfile(out)
                End If
            End If

            Return ini.ForceDirectedArgs
        End Function
    End Class

    ''' <summary>
    ''' Function parameters for <see cref="forceNetwork.doForceLayout(ByRef Graph.NetworkGraph, Double, Double, Double, Integer, Boolean)"/>
    ''' </summary>
    <ClassName(NameOf(ForceDirectedArgs))>
    Public Class ForceDirectedArgs

        <DataFrameColumn> Public Property Stiffness As Single = 41.76!
        <DataFrameColumn> Public Property Repulsion As Single = 10000.0!
        <DataFrameColumn> Public Property Damping As Single = 0.4!
        <DataFrameColumn> Public Property Iterations As Integer = 1000%

        Public Shared Function DefaultNew() As ForceDirectedArgs
            Return New ForceDirectedArgs With {
                .Damping = Parameters.DefaultDamping,
                .Iterations = 1500,
                .Repulsion = Parameters.DefaultRepulsion,
                .Stiffness = Parameters.DefaultStiffness
            }
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace

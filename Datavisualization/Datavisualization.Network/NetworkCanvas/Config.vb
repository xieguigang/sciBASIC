Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.Serialization.JSON

<IniMapIO("#/ForceDirectedArgs.ini")>
Public Class Config

    Public Property ForceDirectedArgs As ForceDirectedArgs

    Public Const DefaultStiffness As Single = 81.76!
    Public Const DefaultRepulsion As Single = 20000.0!
    Public Const DefaultDamping As Single = 0.5!

    Public Shared Function Load() As ForceDirectedArgs
        Dim b As Boolean = False
        Dim ini As Config = LoadProfile(Of Config)(b)

        If Not b Then
            ini.ForceDirectedArgs = New ForceDirectedArgs With {
                .Damping = DefaultDamping,
                .Repulsion = DefaultRepulsion,
                .Stiffness = DefaultStiffness
            }
            Call ini.WriteProfile
        End If

        Return ini.ForceDirectedArgs
    End Function
End Class

<ClassName(NameOf(ForceDirectedArgs))>
Public Class ForceDirectedArgs
    <DataFrameColumn> Public Property Stiffness As Single = 41.76!
    <DataFrameColumn> Public Property Repulsion As Single = 10000.0!
    <DataFrameColumn> Public Property Damping As Single = 0.4!

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
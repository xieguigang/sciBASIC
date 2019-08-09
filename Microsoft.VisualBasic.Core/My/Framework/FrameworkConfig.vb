Namespace My.FrameworkInternal

    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Module Or AttributeTargets.Struct,
                    AllowMultiple:=True,
                    Inherited:=True)>
    Public Class FrameworkConfigAttribute : Inherits Attribute

        ''' <summary>
        ''' The config name in ``/@set`` or ``config.ini`` file
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String

        Public ReadOnly Property Config As String
            Get
                Return App.GetVariable(Name)
            End Get
        End Property

        Sub New(configName As String)
            Name = configName
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim {Name} As String = {App.GetVariable(Name)}"
        End Function
    End Class
End Namespace
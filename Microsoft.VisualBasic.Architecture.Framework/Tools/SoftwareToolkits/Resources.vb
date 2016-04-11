Imports System.Reflection

Namespace SoftwareToolkits

    Public Class Resources

        Public ReadOnly Property FileName As String

        Sub New()
            Call Me.New(Assembly.GetExecutingAssembly)
        End Sub

        Sub New(type As Type)
            Call Me.New(type.Assembly)
        End Sub

        ''' <summary>
        ''' 默认是<see cref="App.HOME"/>/Resources/assmFile
        ''' </summary>
        ''' <param name="assm"></param>
        Sub New(assm As Assembly)
            FileName = assm.Location.ParentPath & "/Resources/" & FileIO.FileSystem.GetFileInfo(assm.Location).Name
            Call __resParser()
        End Sub

        Sub New(dll As String)
            FileName = FileIO.FileSystem.GetFileInfo(dll).FullName
            Call __resParser()
        End Sub

        Private Sub __resParser()
            Dim assm As Assembly = Assembly.LoadFile(FileName)
        End Sub
    End Class
End Namespace
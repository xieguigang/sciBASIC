Imports System.IO
Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports AssemblyMeta = Microsoft.VisualBasic.ApplicationServices.Development.AssemblyInfo

Namespace ApplicationServices

    Public Class Application

        Shared ReadOnly main As Assembly = Assembly.GetEntryAssembly()
        Shared ReadOnly meta As AssemblyMeta = main.FromAssembly

        ''' <summary>
        ''' Gets the path for the executable file that started the application, 
        ''' not including the executable name.
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property StartupPath As String
            Get
                Return Path.GetDirectoryName(main.Location)
            End Get
        End Property

        Public Shared ReadOnly Property ExecutablePath As String
            Get
                Return main.Location
            End Get
        End Property

        Public Shared ReadOnly Property ProductName As String
            Get
                Return meta.AssemblyProduct
            End Get
        End Property

        Public Shared ReadOnly Property ProductVersion As String
            Get
                Return meta.AssemblyVersion
            End Get
        End Property
    End Class
End Namespace
#Region "Microsoft.VisualBasic::4c4bc00b15ec114103b991207f7ba1bd, Microsoft.VisualBasic.Core\ComponentModel\Settings\Inf\Serialization.vb"

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

    '     Class ClassName
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class IniMapIO
    ' 
    '         Properties: Path
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' 定义在Ini配置文件之中的Section的名称
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class ClassName : Inherits Attribute

        Public ReadOnly Property Name As String

        ''' <summary>
        ''' Defines the section name in the ini profile data.(定义在Ini配置文件之中的Section的名称)
        ''' </summary>
        ''' <param name="name"></param>
        Sub New(name As String)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    ''' <summary>
    ''' The path parameter can be shortcut by method <see cref="PathMapper.GetMapPath"/>.
    ''' additional, using ``@fileName`` for using <see cref="App.GetFile(String)"/> API.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class IniMapIO : Inherits Attribute

        Public ReadOnly Property Path As String

        ''' <summary>
        ''' The path parameter can be shortcut by method <see cref="PathMapper.GetMapPath"/>
        ''' </summary>
        ''' <param name="path"></param>
        Sub New(path As String)
            If path.First = "@"c Then
                Me.Path = App.GetFile(Mid(path, 2))
            Else
                Me.Path = PathMapper.GetMapPath(path)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

End Namespace

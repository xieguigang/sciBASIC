#Region "Microsoft.VisualBasic::4997b427ff7828d26117155c6bab2511, Microsoft.VisualBasic.Core\CommandLine\CliResCommon.vb"

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

    '     Class CliResCommon
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString, TryRelease
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace CommandLine

    ''' <summary>
    ''' CLI resources manager
    ''' 
    ''' (将程序内部自身的资源数据释放到文件的帮助对象)
    ''' </summary>
    Public Class CliResCommon

        Private ReadOnly bufType As Type = GetType(Byte())
        Private ReadOnly Resource As Dictionary(Of String, Func(Of Byte()))

        ReadOnly EXPORT As String

        ''' <summary>
        ''' 内部资源为一个可执行文件，其将会被释放到<paramref name="EXPORT"/>文件夹之中
        ''' </summary>
        ''' <param name="EXPORT">资源文件的数据缓存文件夹</param>
        Sub New(EXPORT As String, ResourceManager As Type)
            Dim tag As BindingFlags = BindingFlags.NonPublic Or BindingFlags.Static
            Dim propBufs = From [Property] As PropertyInfo
                           In ResourceManager.GetProperties(bindingAttr:=tag)
                           Where [Property].PropertyType.Equals(bufType)
                           Select [Property]

            Me.EXPORT = EXPORT
            Me.Resource = propBufs.ToDictionary(Of String, Func(Of Byte()))(
                Function(x) x.Name,
                Function(x) New Func(Of Byte())(Function() DirectCast(x.GetValue(Nothing, Nothing), Byte())))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name">使用 NameOf 操作符来获取资源</param>
        ''' <returns></returns>
        Public Function TryRelease(Name As String, Optional ext As String = "exe") As String
            Dim path As String = $"{EXPORT}/{Name}.{ext}"

            If path.FileExists Then
                Return path
            End If

            If Not Resource.ContainsKey(Name) Then
                Return ""
            End If

            Dim buf As Byte() = Resource(Name)()
            Try
                If buf.FlushStream(path) Then
                    Call Console.WriteLine(resReleaseMsg, path.ToFileURL, buf.Length)
                    Return path
                Else
                    Return ""
                End If
            Catch ex As Exception
                ex = New Exception(path, ex)
                Call App.LogException(ex)
                Return ""
            End Try
        End Function

        Const resReleaseMsg As String = "Release resource to {0} // length={1} bytes"

        Public Overrides Function ToString() As String
            Return EXPORT
        End Function
    End Class
End Namespace

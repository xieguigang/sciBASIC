#Region "Microsoft.VisualBasic::59bd2fa95b9c61d2dba7e52326f4afae, Microsoft.VisualBasic.Core\src\CommandLine\CliResCommon.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 73
    '    Code Lines: 47
    ' Comment Lines: 14
    '   Blank Lines: 12
    '     File Size: 2.62 KB


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
            Me.Resource = propBufs.ToDictionary(
                Function(x) x.Name,
                Function(x)
                    Return New Func(Of Byte())(Function() DirectCast(x.GetValue(Nothing, Nothing), Byte()))
                End Function)
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

#Region "Microsoft.VisualBasic::f180cf5b8f380623a43b79caa17821f7, vs_solutions\dev\VisualStudio\VBProject\NuGet\NuGetFramework.vb"

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

    '   Total Lines: 205
    '    Code Lines: 121 (59.02%)
    ' Comment Lines: 42 (20.49%)
    '    - Xml Docs: 90.48%
    ' 
    '   Blank Lines: 42 (20.49%)
    '     File Size: 8.23 KB


    '     Module NuGetFramework
    ' 
    '         Function: GetBest, GetCurrentRuntimeIdentifier, GetLevel, IsAny, IsCompatible
    '                   Normalize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions

Namespace VBProj.NuGet

    ''' <summary>
    ''' 目标框架 moniker(TFM)的兼容性排序、最优匹配与当前运行平台识别。
    ''' </summary>
    ''' <remarks>
    ''' 采用"等级"模型: 每个 TFM 映射为一个整数等级, 只有当资产 TFM 的等级
    ''' 不高于目标框架等级时才认为兼容, 并在全部兼容资产中取等级最高者。
    ''' <list type="bullet">
    ''' <item><c>netstandard1.0</c> → 1100 ... <c>netstandard2.1</c> → 1201</item>
    ''' <item><c>netcoreapp1.0</c> → 2100 ... <c>netcoreapp3.1</c> → 2301</item>
    ''' <item><c>net5.0</c> → 3500 ... <c>net10.0</c> → 4000</item>
    ''' <item>.NET Framework(<c>net48</c> 之类)与未知 TFM 返回 -1(不可用)</item>
    ''' </list>
    ''' 注意: 本模型针对现代 .NET(net5.0+)目标优化, 对 netcoreapp/netstandard 之间的
    ''' 细粒度相互兼容性(例如 netstandard2.1 需要 netcoreapp3.0+)不做精确建模。
    ''' </remarks>
    Public Module NuGetFramework

        ''' <summary>现代 .NET(<c>netX.Y</c>, X &gt;= 5)的等级基数</summary>
        Private Const ModernBase As Integer = 3000

        ''' <summary>netcoreapp 系列的等级基数</summary>
        Private Const CoreAppBase As Integer = 2000

        ''' <summary>netstandard 系列的等级基数</summary>
        Private Const StandardBase As Integer = 1000

        Private ReadOnly ModernPattern As New Regex("^net(?<major>\d+)\.(?<minor>\d+)(?:\.(?<patch>\d+))?$", RegexOptions.IgnoreCase)
        Private ReadOnly LegacyNetPattern As New Regex("^net(?<digits>\d{2,4})$", RegexOptions.IgnoreCase)
        Private ReadOnly CoreAppPattern As New Regex("^netcoreapp(?<major>\d+)\.(?<minor>\d+)$", RegexOptions.IgnoreCase)
        Private ReadOnly StandardPattern As New Regex("^netstandard(?<major>\d+)\.(?<minor>\d+)$", RegexOptions.IgnoreCase)

        ''' <summary>无框架约束的资产目录名(等价于兜底)</summary>
        Private ReadOnly AnyNames As String() = {"", "any", "none"}

        ''' <summary>
        ''' 计算 TFM 的兼容等级; 无法识别(或不与现代 .NET 兼容)时返回 -1。
        ''' </summary>
        ''' <param name="tfm">目标框架 moniker, 例如 <c>net10.0</c> / <c>netstandard2.0</c></param>
        Public Function GetLevel(tfm As String) As Integer
            If String.IsNullOrWhiteSpace(tfm) Then
                Return -1
            End If

            Dim key As String = Normalize(tfm)
            Dim m As Match = ModernPattern.Match(key)

            If m.Success Then
                Dim major As Integer = Integer.Parse(m.Groups("major").Value)
                Dim minor As Integer = Integer.Parse(m.Groups("minor").Value)

                If major >= 5 Then
                    Return ModernBase + major * 100 + minor
                Else
                    ' net4.8 / net3.5 这类"点分形式"的 .NET Framework
                    Return -1
                End If
            End If

            If LegacyNetPattern.IsMatch(key) Then
                ' net48 / net472 : .NET Framework, 与现代 .NET 不兼容
                Return -1
            End If

            m = CoreAppPattern.Match(key)

            If m.Success Then
                Return CoreAppBase +
                    Integer.Parse(m.Groups("major").Value) * 100 +
                    Integer.Parse(m.Groups("minor").Value)
            End If

            m = StandardPattern.Match(key)

            If m.Success Then
                Return StandardBase +
                    Integer.Parse(m.Groups("major").Value) * 100 +
                    Integer.Parse(m.Groups("minor").Value)
            End If

            Return -1
        End Function

        ''' <summary>判断某个资产 TFM 是否可用于给定的目标框架</summary>
        Public Function IsCompatible(tfm As String, targetFramework As String) As Boolean
            Dim target As Integer = GetLevel(targetFramework)
            Dim level As Integer = GetLevel(tfm)

            If target < 0 OrElse level < 0 Then
                Return False
            End If

            Return level <= target
        End Function

        ''' <summary>
        ''' 从候选 TFM 列表中选出对目标框架最优的一个:
        ''' 兼容候选中等级最高者; 没有兼容候选时回退到"无框架约束"项(<c>any</c>/空串)。
        ''' </summary>
        ''' <param name="frameworks">候选 TFM(例如 <c>lib/</c> 下的子目录名)</param>
        ''' <param name="targetFramework">目标框架 moniker</param>
        ''' <returns>命中的候选原文; 均不匹配时返回 Nothing</returns>
        Public Function GetBest(frameworks As IEnumerable(Of String), targetFramework As String) As String
            If frameworks Is Nothing Then
                Return Nothing
            End If

            Dim target As Integer = GetLevel(targetFramework)
            Dim best As String = Nothing
            Dim bestLevel As Integer = -1

            For Each tfm As String In frameworks
                If tfm Is Nothing Then
                    Continue For
                End If

                Dim level As Integer = GetLevel(tfm)

                If level < 0 OrElse level > target Then
                    Continue For
                End If

                If level > bestLevel Then
                    bestLevel = level
                    best = tfm
                End If
            Next

            If best IsNot Nothing Then
                Return best
            End If

            ' 兜底: nuspec 允许存在不带 targetFramework 的依赖组
            For Each tfm As String In frameworks
                If IsAny(tfm) Then
                    Return tfm
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>该 TFM 是否为"任意框架"的兜底标记</summary>
        Public Function IsAny(tfm As String) As Boolean
            If tfm Is Nothing Then
                Return True
            End If

            Return AnyNames.Any(Function(name) String.Equals(name, tfm.Trim(), StringComparison.OrdinalIgnoreCase))
        End Function

        ''' <summary>
        ''' 当前进程的运行平台标识, 形如 <c>win-x64</c> / <c>linux-x64</c> / <c>osx-arm64</c>。
        ''' </summary>
        Public Function GetCurrentRuntimeIdentifier() As String
            Dim arch As String

            Select Case RuntimeInformation.ProcessArchitecture
                Case Architecture.X64 : arch = "x64"
                Case Architecture.X86 : arch = "x86"
                Case Architecture.Arm64 : arch = "arm64"
                Case Architecture.Arm : arch = "arm"
                Case Else : arch = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
            End Select

            If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
                Return "win-" & arch
            End If

            If RuntimeInformation.IsOSPlatform(OSPlatform.OSX) Then
                Return "osx-" & arch
            End If

            If RuntimeInformation.IsOSPlatform(OSPlatform.Linux) Then
                Return "linux-" & arch
            End If

            Return "any"
        End Function

        ''' <summary>
        ''' 归一化 TFM: 去空白、转小写、去掉平台后缀(<c>net6.0-windows</c> → <c>net6.0</c>)。
        ''' </summary>
        Public Function Normalize(tfm As String) As String
            Dim key As String = If(tfm, "").Trim().ToLowerInvariant()

            ' nuspec v2 格式会写成 ".NETStandard2.0" /.NETCoreApp3.1"
            key = key.TrimStart("."c)

            Dim dash As Integer = key.IndexOf("-"c)

            If dash > 0 Then
                key = key.Substring(0, dash)
            End If

            Return key
        End Function
    End Module
End Namespace


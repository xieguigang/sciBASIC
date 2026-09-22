#Region "Microsoft.VisualBasic::3144fc055897ad1ec31ec18220e4b9c4, vs_solutions\dev\VisualStudio\VBProject\NuGet\NuGetVersion.vb"

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

    '   Total Lines: 383
    '    Code Lines: 261 (68.15%)
    ' Comment Lines: 32 (8.36%)
    '    - Xml Docs: 90.62%
    ' 
    '   Blank Lines: 90 (23.50%)
    '     File Size: 13.20 KB


    '     Class NuGetVersion
    ' 
    '         Properties: FolderName, IsPrerelease, Major, Minor, OriginalString
    '                     Patch, ReleaseLabel, Revision
    ' 
    '         Function: CompareRelease, (+2 Overloads) CompareTo, (+2 Overloads) Equals, GetHashCode, NormalizedString
    '                   Parse, ToString, TryParse
    ' 
    '     Class VersionRange
    ' 
    '         Properties: All, IsExact, IsMaxInclusive, IsMinInclusive, MaxVersion
    '                     MinVersion, OriginalString
    ' 
    '         Function: Parse, Satisfies, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text

Namespace VBProj.NuGet

    ''' <summary>
    ''' NuGet 所使用的语义化版本号: <c>major.minor.patch[.revision][-prerelease][+build]</c>。
    ''' </summary>
    ''' <remarks>
    ''' 只实现 NuGet flat-container 依赖解析所需的最小语义:
    ''' 四段数字(缺失段视为 0)、预发布标签(按 SemVer 规则比较), 以及构建元数据(<c>+build</c> 直接忽略)。
    ''' </remarks>
    Public Class NuGetVersion : Implements IComparable, IComparable(Of NuGetVersion), IEquatable(Of NuGetVersion)

        Public Property Major As Integer
        Public Property Minor As Integer
        Public Property Patch As Integer
        Public Property Revision As Integer

        ''' <summary>预发布标签(例如 <c>beta.1</c>); 稳定版为 Nothing</summary>
        Public Property ReleaseLabel As String

        ''' <summary>解析之前的原始文本</summary>
        Public Property OriginalString As String

        ''' <summary>是否为预发布版本(例如 1.0.0-beta)</summary>
        Public ReadOnly Property IsPrerelease As Boolean
            Get
                Return Not String.IsNullOrEmpty(ReleaseLabel)
            End Get
        End Property

        ''' <summary>用于 nuget 全局包目录的规范化小写版本号</summary>
        Public ReadOnly Property FolderName As String
            Get
                Return NormalizedString().ToLowerInvariant()
            End Get
        End Property

        Public Shared Function Parse(text As String) As NuGetVersion
            Dim version As NuGetVersion = Nothing

            If Not TryParse(text, version) Then
                Throw New FormatException($"无效的 nuget 版本号: {text}")
            End If

            Return version
        End Function

        ''' <summary>
        ''' 解析版本号文本; 失败时返回 False 并保持 <paramref name="version"/> 为 Nothing。
        ''' </summary>
        Public Shared Function TryParse(text As String, ByRef version As NuGetVersion) As Boolean
            version = Nothing

            If String.IsNullOrWhiteSpace(text) Then
                Return False
            End If

            Dim raw As String = text.Trim()

            If raw.StartsWith("v"c) OrElse raw.StartsWith("V"c) Then
                raw = raw.Substring(1)
            End If

            ' 构建元数据(+build)不参与版本比较, 直接丢弃
            Dim plus As Integer = raw.IndexOf("+"c)

            If plus >= 0 Then
                raw = raw.Substring(0, plus)
            End If

            Dim release As String = Nothing
            Dim dash As Integer = raw.IndexOf("-"c)

            If dash >= 0 Then
                release = raw.Substring(dash + 1).Trim()
                raw = raw.Substring(0, dash)
            End If

            Dim parts As String() = raw.Split("."c)

            If parts.Length = 0 OrElse parts.Length > 4 Then
                Return False
            End If

            Dim nums As Integer() = {0, 0, 0, 0}

            For i As Integer = 0 To parts.Length - 1
                Dim n As Integer

                If Not Integer.TryParse(parts(i).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, n) Then
                    Return False
                End If

                nums(i) = n
            Next

            If String.IsNullOrEmpty(release) Then
                release = Nothing
            End If

            version = New NuGetVersion With {
                .Major = nums(0),
                .Minor = nums(1),
                .Patch = nums(2),
                .Revision = nums(3),
                .ReleaseLabel = release,
                .OriginalString = text.Trim()
            }

            Return True
        End Function

        Public Function CompareTo(other As NuGetVersion) As Integer Implements IComparable(Of NuGetVersion).CompareTo
            If other Is Nothing Then
                Return 1
            End If

            Dim c As Integer = Major.CompareTo(other.Major)

            If c <> 0 Then
                Return c
            End If

            c = Minor.CompareTo(other.Minor)

            If c <> 0 Then
                Return c
            End If

            c = Patch.CompareTo(other.Patch)

            If c <> 0 Then
                Return c
            End If

            c = Revision.CompareTo(other.Revision)

            If c <> 0 Then
                Return c
            End If

            Return CompareRelease(ReleaseLabel, other.ReleaseLabel)
        End Function

        ''' <summary>
        ''' 预发布标签比较: 稳定版大于任何预发布版; 标签按点号分段, 数字段按数值比较, 数字段小于字母段。
        ''' </summary>
        Private Shared Function CompareRelease(left As String, right As String) As Integer
            Dim isLeftStable As Boolean = String.IsNullOrEmpty(left)
            Dim isRightStable As Boolean = String.IsNullOrEmpty(right)

            If isLeftStable AndAlso isRightStable Then
                Return 0
            End If

            If isLeftStable Then
                Return 1
            End If

            If isRightStable Then
                Return -1
            End If

            Dim lparts As String() = left.Split("."c)
            Dim rparts As String() = right.Split("."c)
            Dim n As Integer = lparts.Length

            If rparts.Length < n Then
                n = rparts.Length
            End If

            For i As Integer = 0 To n - 1
                Dim ln As Integer, rn As Integer
                Dim isLeftNumber As Boolean = Integer.TryParse(lparts(i), ln)
                Dim isRightNumber As Boolean = Integer.TryParse(rparts(i), rn)

                If isLeftNumber AndAlso isRightNumber Then
                    Dim c As Integer = ln.CompareTo(rn)

                    If c <> 0 Then
                        Return c
                    End If
                ElseIf isLeftNumber Then
                    Return -1
                ElseIf isRightNumber Then
                    Return 1
                Else
                    Dim c As Integer = String.Compare(lparts(i), rparts(i), StringComparison.OrdinalIgnoreCase)

                    If c <> 0 Then
                        Return c
                    End If
                End If
            Next

            Return lparts.Length.CompareTo(rparts.Length)
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Return CompareTo(TryCast(obj, NuGetVersion))
        End Function

        Public Overloads Function Equals(other As NuGetVersion) As Boolean Implements IEquatable(Of NuGetVersion).Equals
            If other Is Nothing Then
                Return False
            End If

            Return CompareTo(other) = 0
        End Function

        Public Overloads Overrides Function Equals(obj As Object) As Boolean
            Return Equals(TryCast(obj, NuGetVersion))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return NormalizedString().ToLowerInvariant().GetHashCode()
        End Function

        ''' <summary>规范化版本号文本: 去掉多余的 0 段, 保留预发布标签(小写不改写)</summary>
        Public Function NormalizedString() As String
            Dim sb As New StringBuilder()

            Call sb.Append(Major).Append("."c).Append(Minor).Append("."c).Append(Patch)

            If Revision > 0 Then
                Call sb.Append("."c).Append(Revision)
            End If

            If IsPrerelease Then
                Call sb.Append("-"c).Append(ReleaseLabel)
            End If

            Return sb.ToString()
        End Function

        Public Overrides Function ToString() As String
            Return NormalizedString()
        End Function
    End Class

    ''' <summary>
    ''' NuGet 版本范围: <c>1.2.3</c>(最小版本)、<c>[1.2.3]</c>(精确)、<c>[1.0,)</c>、<c>[1.0,2.0)</c>、<c>(,2.0]</c> 等。
    ''' </summary>
    Public Class VersionRange

        ''' <summary>下界; Nothing 表示无下界</summary>
        Public Property MinVersion As NuGetVersion

        ''' <summary>上界; Nothing 表示无上界</summary>
        Public Property MaxVersion As NuGetVersion

        ''' <summary>下界是否可取等号</summary>
        Public Property IsMinInclusive As Boolean = True

        ''' <summary>上界是否可取等号</summary>
        Public Property IsMaxInclusive As Boolean = False

        ''' <summary>解析之前的原始文本</summary>
        Public Property OriginalString As String

        ''' <summary>是否为精确版本约束</summary>
        Public ReadOnly Property IsExact As Boolean
            Get
                Return MinVersion IsNot Nothing AndAlso
                    MaxVersion IsNot Nothing AndAlso
                    MinVersion.CompareTo(MaxVersion) = 0 AndAlso
                    IsMinInclusive AndAlso
                    IsMaxInclusive
            End Get
        End Property

        ''' <summary>无约束范围(任意版本)</summary>
        Public Shared ReadOnly Property All As VersionRange
            Get
                Return New VersionRange With {
                    .IsMinInclusive = True,
                    .IsMaxInclusive = False
                }
            End Get
        End Property

        Public Shared Function Parse(text As String) As VersionRange
            If String.IsNullOrWhiteSpace(text) Then
                Return All
            End If

            Dim raw As String = text.Trim()
            Dim range As New VersionRange With {.OriginalString = raw}
            Dim isBracketed As Boolean = raw.StartsWith("["c) OrElse raw.StartsWith("("c)

            If Not isBracketed Then
                ' 裸版本号 => ">= version"
                range.MinVersion = NuGetVersion.Parse(raw)
                range.IsMinInclusive = True
                range.IsMaxInclusive = False
                Return range
            End If

            If raw.Length < 2 Then
                Throw New FormatException($"无效的 nuget 版本范围: {text}")
            End If

            Dim minInclusive As Boolean = raw.StartsWith("["c)
            Dim maxInclusive As Boolean = raw.EndsWith("]"c)
            Dim body As String = raw.Substring(1, raw.Length - 2).Trim()
            Dim sep As Integer = body.IndexOf(","c)

            If sep < 0 Then
                ' [1.0] => 精确版本
                Dim exact As NuGetVersion = NuGetVersion.Parse(body)

                range.MinVersion = exact
                range.MaxVersion = exact
                range.IsMinInclusive = True
                range.IsMaxInclusive = True
                Return range
            End If

            Dim lower As String = body.Substring(0, sep).Trim()
            Dim upper As String = body.Substring(sep + 1).Trim()

            If lower.Length > 0 Then
                range.MinVersion = NuGetVersion.Parse(lower)
            End If

            If upper.Length > 0 Then
                range.MaxVersion = NuGetVersion.Parse(upper)
            End If

            range.IsMinInclusive = minInclusive
            range.IsMaxInclusive = maxInclusive

            Return range
        End Function

        ''' <summary>判断给定版本是否落在本范围之内</summary>
        Public Function Satisfies(version As NuGetVersion) As Boolean
            If version Is Nothing Then
                Return False
            End If

            If MinVersion IsNot Nothing Then
                Dim c As Integer = version.CompareTo(MinVersion)

                If c < 0 Then
                    Return False
                End If

                If c = 0 AndAlso Not IsMinInclusive Then
                    Return False
                End If
            End If

            If MaxVersion IsNot Nothing Then
                Dim c As Integer = version.CompareTo(MaxVersion)

                If c > 0 Then
                    Return False
                End If

                If c = 0 AndAlso Not IsMaxInclusive Then
                    Return False
                End If
            End If

            Return True
        End Function

        Public Overrides Function ToString() As String
            If IsExact Then
                Return "[" & MinVersion.ToString() & "]"
            End If

            Dim lower As String = If(MinVersion Is Nothing, "", MinVersion.ToString())
            Dim upper As String = If(MaxVersion Is Nothing, "", MaxVersion.ToString())

            Return If(IsMinInclusive, "[", "(") & lower & ", " & upper & If(IsMaxInclusive, "]", ")")
        End Function
    End Class
End Namespace

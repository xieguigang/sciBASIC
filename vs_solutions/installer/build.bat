@echo off

REM Imports MSBuild environment
REM run this script directly will not working???
SET vs_dev="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
SET currentWork=%cd%
SET drive=%currentWork:~0,2%

echo Current workspace: %currentWork% is located at Drive %drive%

CALL %vs_dev%

REM restore the workspace location
CD /D %currentWork%
echo %cd%

REM batch script for build sciBASIC# installer project
REM output location is at ``./output`` directory
REM redistribute the ``sciBASIC_installer.exe``

REM installer packaged based on the MSBuild platform

SET sciBASIC_sln="../Microsoft.VisualBasic.Architecture.Framework - ALL.sln"
SET installer_sln="./vs_installer.sln"

REM check the visual studio solution file exists or not?

if exist %sciBASIC_sln% (
    echo "%sciBASIC_sln% exist!"
) else (
    echo "Can not found sciBASIC# project!"	
    pause
	
    exit -100
)

if exist %installer_sln% (
    echo "%installer_sln% exist!"
) else (
    echo "Can not found sciBASIC# installer project!"	
    pause
	
    exit -120
)

echo "[ALL] check success!"
echo "Now run build for the sciBASIC# framework"

REM cleanup the output workspace at first and then run build

SET output=../../.vs/installer

RD /S /Q %output%
MSBuild %sciBASIC_sln% /t:Rebuild /p:Configuration=installer_x64;Platform=x64 /fl /flp:logfile=./sciBASIC-build.log;verbosity=diagnostic

REM package the output into a zip package
SET WinRAR="C:\Program Files\WinRAR\RAR.exe"
SET zip=%output%/installer.zip
%WinRAR% a -r %zip% %output%

REM add it as installer project resource file
SET resource="./Installer/Resources/installer.zip"
DEL /a /f %resource%
echo YES | MOVE %zip% %resource%

REM cleanup the installer output folder
REM and then run msbuild for installer project

SET output="./bootstrap/Resources/installer.zip"

DEL /a /f %output%
MSBuild %installer_sln% /t:Rebuild /p:Configuration=installer_x64;Platform=x64 /fl /flp:logfile=./installer-build-1.log;verbosity=diagnostic 

REM now installer program is located at the bootstrap loader resource folder
REM zip the output as a zip file

SET output="./bootstrap/Resources"
SET zip="./bootstrap/Resources/installer.zip"
echo "  --> %output%"
%WinRAR% a -r %zip% %output%

REM Rebuild the installer project
REM and then we can generates the final installer file
SET output=./output/

RD /S /Q "%output%"
MSBuild %installer_sln% /t:Rebuild /p:Configuration=installer_x64;Platform=x64 /fl /flp:logfile=./installer-build-2.log;verbosity=diagnostic 

if exist %output%/sciBASIC_installer.exe (
    echo [Done] Build sciBASIC# Framework installer success!
	
    REM open the output directory
    explorer "%cd%\output"
) else (
    echo "Build installer project failured!"	
)
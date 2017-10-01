@echo off

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
MSBuild %sciBASIC_sln% /t:Rebuild/p:Configuration=installer_x64;Platform=x64 /fl /flp:logfile=./sciBASIC-build.log;verbosity=diagnostic

REM package the output into a zip package
SET zip=%output%/installer.zip
WinRAR a -r %zip% %output%

REM add it as installer project resource file
SET resource="./Installer/Resources/installer.zip"
DEL /a /f %resource%
MOVE %zip% %resource%

 


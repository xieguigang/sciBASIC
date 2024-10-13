@echo off

SET R_proj=../../../R-sharp
SET R_HOME=%R_proj%/App/net8.0
SET REnv="%R_HOME%/R#.exe"
SET updater=%R_proj%/studio/code_banner.R

%REnv% %updater% --banner-xml ../../../../gpl3.xml --proj-folder ../

cd ../

git add -A
git commit -m "update source file banner headers!"

cd ./.nuget/

pause
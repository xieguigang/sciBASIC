@echo off

REM git remote add local http://192.168.3.48:8418/xieguigang/scibasic.git

git pull gitlink HEAD
git pull gitee HEAD
git pull local HEAD

git push gitlink HEAD
git push gitee HEAD
git push local HEAD

echo synchronization of this code repository job done!
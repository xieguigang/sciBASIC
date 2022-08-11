@echo off

REM git remote add gitlink https://gitlink.org.cn/xieguigang/sciBASIC.git

git pull local HEAD
git pull gitee HEAD
git pull gitlink HEAD

git push local HEAD
git push gitee HEAD
git push gitlink HEAD

echo synchronization of this code repository job done!
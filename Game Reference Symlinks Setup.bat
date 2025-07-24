@echo off

set /p path="Please enter the folder location of your SpaceEngineers.exe: "

cd %~dp0
rmdir ClientBinaries > nul 2>&1
mklink /J ClientBinaries "%path%"
if errorlevel 1 goto Error
echo Done!


echo You can now open the Mod without issue.
goto EndFinal

:Error
echo An error occurred creating the symlink.
goto EndFinal

:EndFinal
pause
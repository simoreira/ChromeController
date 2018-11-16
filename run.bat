cd speechModality\speechModality\bin\Debug
start speechModality.exe
cd ..\..\..\..
cd AppGui\AppGui\bin\x86\Debug\
start AppGui.exe
cd ..\..\..\..
cd IM
java -jar mmiframeworkV2.jar

Taskkill /IM AppGui.exe
Taskkill /IM speechModality.exe

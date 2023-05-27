echo "Update program"
taskkill /f /im Foxconn.App.exe
UnRAR.exe x -o+ Update.rar
del Update.rar
start Foxconn.App.exe
exit
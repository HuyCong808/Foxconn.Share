echo "Clear program"
taskkill /f /im Foxconn.App.exe
wmic process where name="Foxconn.App.exe" call terminate
exit
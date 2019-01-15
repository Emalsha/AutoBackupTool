# AutoBackupTool
Simple Console Application to Backup SQL Database, Application file and replace file with updated file.

## Usage.
I  created this tool to help my self on test changes on testing environment.
After every main update I had to take backup of test env database and application main dll file. 
And have to replace dll files and test the services are working.
Since I do this every time and this is a repetative work, I created simple application to do it automatically.

## How to use.
First download the executable.(If you need there is source code and solution. You can run it on VS.)
Update the conf.json file with your data.

```json
{
  "host": "HOST",
  "database": "DATABASE",
  "username": "username",
  "password": "password",
  "sourcepath": "C:\\Users\\dev\\src\\App\\bin",
  "targetpath": "\\\\HOST\\www\\APP\\Production\\bin",
  "backuppath": "\\\\HOST\\temp\\AutoBackup\\",
  "replacingfile": [
    "First.dll",
    "Second.Replace.dll",
    "Third.Replace.File.dll"
  ]
}
```

Replace values with your data and run AutoBackupTool.exe file. 

Thats all.

## What this tool do.
  * Take backup of database.
  * Move given file in the host to backup place (Database backup and file will contain in the same folder).
  * Move your updated file to host.
  
Backup folder will follow "databasename_date_timewithsecond" name format. 
ex: "testdb_20190115_162635"

Database bak file also take the same name format.

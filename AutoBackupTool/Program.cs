using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace AutoBackupTool
{
    /***
     *  Sample Json Configuration template. 
     *  conf.json
     *  
     * {
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
      * */

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (StreamReader reader = new StreamReader(".\\conf.json"))
                {
                    var json = reader.ReadToEnd();
                    var conf = JObject.Parse(json);

                    //Main Configuration
                    var host = (string)conf.Property("host").Value;
                    var sqlUsername = (string)conf.Property("username").Value;
                    var sqlPassword = (string)conf.Property("password").Value;
                    var hostDatabase = (string)conf.Property("database").Value;
                    var backupdllfile = conf.Property("replacingfile").Value;

                    //Backup location conf
                    var sourcepath = (string)conf.Property("sourcepath").Value;
                    var targetpath = (string)conf.Property("targetpath").Value;
                    var backuppath = (string)conf.Property("backuppath").Value;
                    var backupname = hostDatabase + "_" + DateTime.Now.ToString("s").Replace("-", "").Replace(":", "").Replace("T", "_");
                    var folderpath = System.IO.Path.Combine(backuppath, backupname);
                    var backupfile = System.IO.Path.Combine(folderpath, backupname + ".bak");

                    try
                    {
                        System.IO.Directory.CreateDirectory(folderpath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.ReadKey();
                        return;
                    }

                    //Simple show ;) 
                    PrintTitle();

                    //-- Database backup start.
                    //Server Configuration
                    Server server = new Server(host);
                    server.ConnectionContext.LoginSecure = false;
                    server.ConnectionContext.Login = sqlUsername;
                    server.ConnectionContext.Password = sqlPassword;
                    try
                    {
                        server.ConnectionContext.Connect();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.ReadKey();
                        return;
                    }

                    //Database Configuration
                    Database database = server.Databases[hostDatabase];

                    Backup backup = new Backup();

                    //Set backup values
                    backup.Action = BackupActionType.Database;
                    try
                    {
                        backup.Database = database.Name;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Database not found.");
                        Console.ReadKey();
                        return;
                    }
                    backup.Devices.AddDevice(backupfile, DeviceType.File);
                    backup.BackupSetName = "CNC Database Full backup";
                    backup.BackupSetDescription = "Database backup before update";
                    backup.Initialize = true;

                    //Show progress
                    backup.PercentComplete += CompletionStatusInPercent;
                    backup.Complete += BackupCompleted;

                    backup.SqlBackup(server);


                    //Finally
                    if (server.ConnectionContext.IsOpen)
                    {
                        server.ConnectionContext.Disconnect();
                    }

                    //-- Database backup finished, Next : dll file.
                    Console.WriteLine();
                    Console.WriteLine("2) File Backup Start.");
                    foreach (var file in backupdllfile)
                    {
                        var targetfile = System.IO.Path.Combine(targetpath, file.ToString());
                        var destfile = System.IO.Path.Combine(folderpath, file.ToString());
                        if (System.IO.File.Exists(targetfile))
                        {
                            System.IO.File.Copy(targetfile, destfile);
                            Console.WriteLine("File : " + file.ToString() + " successfully moved to backup folder.");
                        }
                        else
                        {
                            Console.WriteLine("Application target path files not found. Please check target path configuration or replacing file is valid.");
                            Console.ReadKey();
                            return;
                        }
                    }

                    //-- Backup finished, Next : move new file.
                    Console.WriteLine();
                    Console.WriteLine("3) File Replace Start.");
                    foreach (var file in backupdllfile)
                    {
                        var sourcefile = System.IO.Path.Combine(sourcepath, file.ToString());
                        var destfile = System.IO.Path.Combine(targetpath, file.ToString());
                        if (System.IO.File.Exists(sourcefile))
                        {
                            System.IO.File.Copy(sourcefile, destfile, true);
                            Console.WriteLine("File : " + file.ToString() + " successfully override.");
                        }
                        else
                        {
                            Console.WriteLine("Source path files not found. Please check source path configuration.");
                            Console.ReadKey();
                            return;
                        }
                    }

                    //-- END
                    Console.WriteLine();
                    Console.WriteLine("Successful!");
                    Console.ReadKey();
                }
            }catch(Exception ex)
            {
                Console.WriteLine("Please check do you have conf.json file. If not follow this template.");
                var ste = @"
                {
                    ""host"": ""HOST"",
                    ""database"": ""DATABASE"",
                    ""username"": ""username"",
                    ""password"": ""password"",
                    ""sourcepath"": ""C:\\Users\\dev\\src\\App\\bin"",
                    ""targetpath"": ""\\\\HOST\\www\\APP\\Production\\bin"",
                    ""backuppath"": ""\\\\HOST\\temp\\AutoBackup\\"",
                    ""replacingfile"": [
                                        ""First.dll"",
                                        ""Second.Replace.dll"",
                                        ""Third.Replace.File.dll""
                                        ]
                }";
                Console.WriteLine(ste);
                Console.ReadKey();
            }
        }

        private static void BackupCompleted(object sender, ServerMessageEventArgs e)
        {
            Console.WriteLine("Backup Complete.");
            Console.WriteLine(e.Error.Message);
        }

        private static void CompletionStatusInPercent(object sender, PercentCompleteEventArgs e)
        {
            Console.Clear();
            PrintTitle();
            Console.WriteLine("1) Database Backup Start.");
            Console.WriteLine("Backup process : " + e.Percent + "%");
        }

        private static void PrintTitle()
        {
            //Simple show ;) 
            Console.WriteLine(String.Concat(Enumerable.Repeat("-", 31)));
            Console.WriteLine(" Automate Deploy v1.0 @Emalsha ");
            Console.WriteLine(String.Concat(Enumerable.Repeat("-", 31)));
            Console.WriteLine();
        }
    }
}

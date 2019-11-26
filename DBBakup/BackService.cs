using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DBBackup
{
    public class BackService
    {
        private Hashtable setting = null;

        private bool isruning = false;
        private System.Timers.Timer bakTimer = null;
        private String backupCommand = "mysqldump -u{0} -p\"{1}\" -h{2} -P{3} {4}>{5}";
        private String compressCommand = "7z a -t7z {0} {1}";
        private String strBakTime = "";
        private String status = "";
        private DateTime? bakTime = null;
        private String user = "";
        private String pwd = "";
        private String url = "";
        private String port = "";
        private String dbName = "";
        private String bakFileName = "";
        private String folderName = "";
        private String savePath = "";
        private String compressName = "";
        private String removeFolder = "";

        public void Start(string[] args)
        {
            String type = args[0].ToString();
            switch (type)
            {
                case "backup":
                    Backup();
                    break;
                case "transfer":
                    Transfer();
                    break;
            }
        }

        public void Backup()
        {
            String res = "";
            Common.WriteLog("backup start");
            setting = Common.GetSetting();
            // 需要数据备份的服务器
            List<Hashtable> serverList = (List<Hashtable>)setting["servers"];
            Common.WriteLog(serverList.Count + " servers need to backup.");
            foreach (Hashtable server in serverList)
            {
                strBakTime = (string)server["bakTime"];
                user = (string)server["user"];
                pwd = (string)server["pwd"];
                url = (string)server["url"];
                port = (string)server["port"];
                folderName = (string)server["folderName"];
                savePath = (string)server["savePath"];
                compressName = (string)server["compressName"];
                removeFolder = (string)server["removeFolder"];
                Common.formatStr(ref folderName);
                Common.formatStr(ref savePath);
                Common.formatStr(ref compressName);
                // 判断savePath是否存在，不存在则创建
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                    Common.WriteLog(savePath + " folder not exists，created。");
                }

                // 判断folderName是否存在，不存在则创建
                if (!Directory.Exists(savePath + "\\" + folderName))
                {
                    Directory.CreateDirectory(savePath + "\\" + folderName);
                    Common.WriteLog((savePath + "\\" + folderName) + " folder not exists，created。");
                }

                List<Hashtable> databases = (List<Hashtable>)server["databases"];
                foreach (Hashtable db in databases)
                {
                    dbName = db["name"].ToString();
                    bakFileName = db["bakFileName"].ToString();
                    Common.formatStr(ref bakFileName);
                    String tempBackUpCmd = String.Format(backupCommand, user, pwd, url, port, dbName,
                        savePath + "\\" + folderName + "\\" + bakFileName);
                    Common.WriteLog(tempBackUpCmd);
                    res = Common.ProcCmd(tempBackUpCmd);
                    Common.WriteLog(res);
                    Common.WriteLog(savePath + "\\" + folderName + "\\" + bakFileName + " ok");
                }

                String tempCompressCommand = compressCommand;
                // 以上数据库备份完成后，压缩文件
                tempCompressCommand = String.Format(tempCompressCommand, savePath + "\\" + compressName,
                    savePath + "\\" + folderName);
                Common.WriteLog(tempCompressCommand);
                res = Common.ProcCmd(tempCompressCommand);
                Common.WriteLog(res);
                Common.WriteLog(savePath + "\\" + compressName + " saved.");
                Common.WriteLog("remove folder:" + removeFolder);
                if (removeFolder == "1")
                {
                    Directory.Delete(savePath + "\\" + folderName, true);
                    Common.WriteLog((savePath + "\\" + folderName) + " folder deleted.");
                }
            }

            Common.WriteLog("backup finished.");
        }

        public void Transfer()
        {
            Common.WriteLog("transfer start...");
            setting = Common.GetSetting();
            String sourcePath = "";
            String destPath = "";
            String files = "";
            String removeSource = "";
            String storeDays = "";
            String removeHistory = "";
            String copyCommand = "copy {0} {1}";
            // 需要数据备份的服务器
            List<Hashtable> transferList = (List<Hashtable>)setting["transfers"];
            foreach (Hashtable transfer in transferList)
            {
                sourcePath = transfer["sourcePath"].ToString();
                destPath = transfer["destPath"].ToString();
                files = transfer["files"].ToString();
                removeSource = transfer["removeSource"].ToString();
                storeDays = transfer["storeDays"].ToString();
                removeHistory = transfer["removeHistory"].ToString();
                Common.formatStr(ref sourcePath);
                Common.formatStr(ref destPath);
                String[] fileList = files.Split(';');
                foreach (string tempFile in fileList)
                {
                    String file = tempFile;
                    Common.formatStr(ref file);
                    if (File.Exists(sourcePath + "\\" + file))
                    {
                        if (!Directory.Exists(destPath))
                        {
                            Directory.CreateDirectory(destPath);
                        }
                        Common.WriteLog("start transfer ");
                        String tempCopyCommand =
                            String.Format(copyCommand, sourcePath + "\\" + file, destPath + "\\" + file);
                        Common.WriteLog(tempCopyCommand);
                        String res = Common.ProcCmd(tempCopyCommand);
                        Common.WriteLog(res);
                        Common.WriteLog((sourcePath + "\\" + file) + " -> " + (destPath + "\\" + file));
                        Common.WriteLog("removeSource:" + removeSource);
                        if (removeSource == "1")
                        {
                            FileInfo sourceFile = new FileInfo(sourcePath + "\\" + file);
                            sourceFile.Delete();
                            Common.WriteLog((sourcePath + "\\" + file) + "deleted.");
                        }

                        // 删除历史备份
                        Common.WriteLog("removeHistory:" + removeHistory);
                        if (removeHistory == "1")
                        {
                            //保留天数
                            DateTime today = DateTime.Now;
                            int intStoreDays = 0;

                            if (storeDays != "" && Int32.TryParse(storeDays, out intStoreDays))
                            {
                                DateTime historyEnd = DateTime.MinValue;
                                
                                // 如果文件名中包含predate，即要迁移的是前一天的文件，那么删除处理时
                                if (tempFile.Contains("{predate}"))
                                {
                                    historyEnd = today.AddDays(-intStoreDays - 1);
                                }

                                if (tempFile.Contains("{date}"))
                                {
                                    historyEnd = today.AddDays(-intStoreDays);
                                }
                                Common.WriteLog("historyEnd:" + historyEnd.ToString("yyyy-MM-dd"));

                                // 删除 (historyEnd - 5) -> historyEnd 的记录
                                for (int i = 0; i < 5; i++)
                                {
                                    if (historyEnd != DateTime.MinValue)
                                    {
                                        String removeFileName = tempFile;
                                        Common.formatHistoryStr(ref removeFileName,historyEnd.AddDays(-i));
                                        // 从destPath中删除文件
                                        if (File.Exists(destPath + "\\" + removeFileName))
                                        {
                                            Common.WriteLog("history - remove:" + (destPath + "\\" + removeFileName));
                                            File.Delete(destPath + "\\" + removeFileName);
                                        }
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        Common.WriteLog((sourcePath + "\\" + file) + " not exists.");
                    }
                }
            }

            Common.WriteLog("transfer finished.");
        }
    }
}
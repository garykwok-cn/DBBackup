using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace DBBackup
{
    public class Common
    {
        public static Hashtable GetSetting()
        {
            Hashtable settingTable = new Hashtable();
            try
            {
                XmlDocument doc = GetXmlDoc("setting.xml");
                XmlNodeList dbservers = doc.GetElementsByTagName("dbServers");
                XmlNodeList transferSets = doc.GetElementsByTagName("transferSet");
                List<Hashtable> serverList = new List<Hashtable>();
                foreach (XmlNode dbserver in dbservers)
                {
                    XmlNodeList servers = dbserver.SelectNodes("server");
                    Hashtable server = null;

                    foreach (XmlNode serverNode in servers)
                    {
                        server = new Hashtable();
                        server.Add("url", serverNode.SelectSingleNode("url").InnerText);
                        server.Add("port", serverNode.SelectSingleNode("port").InnerText);
                        server.Add("user", serverNode.SelectSingleNode("user").InnerText);
                        server.Add("pwd", serverNode.SelectSingleNode("pwd").InnerText);
                        server.Add("folderName", serverNode.SelectSingleNode("folderName").InnerText);
                        server.Add("savePath", serverNode.SelectSingleNode("savePath").InnerText);
                        server.Add("compressName", serverNode.SelectSingleNode("compressName").InnerText);
                        server.Add("removeFolder", serverNode.SelectSingleNode("removeFolder").InnerText);
                        // databases
                        XmlNode databases = serverNode.SelectSingleNode("databases");
                        XmlNodeList dbNodeList = databases.SelectNodes("db");
                        List<Hashtable> dbList = new List<Hashtable>();
                        foreach (XmlNode dbNode in dbNodeList)
                        {
                            Hashtable db = new Hashtable();
                            db.Add("name", dbNode.SelectSingleNode("name").InnerText);
                            db.Add("bakFileName", dbNode.SelectSingleNode("bakFileName").InnerText);
                            dbList.Add(db);
                        }
                        server.Add("databases", dbList);
                        serverList.Add(server);
                    }
                }
                settingTable.Add("servers", serverList);
                List<Hashtable> transferList = new List<Hashtable>();
                foreach (XmlNode transferSet in transferSets)
                {
                    XmlNodeList transfers = transferSet.SelectNodes("transfer");
                    Hashtable transfer = null;
                    foreach (XmlNode transferNode in transfers)
                    {
                        transfer = new Hashtable();
                        transfer.Add("sourcePath", transferNode.SelectSingleNode("sourcePath").InnerText);
                        transfer.Add("destPath", transferNode.SelectSingleNode("destPath").InnerText);
                        transfer.Add("files", transferNode.SelectSingleNode("files").InnerText);
                        transfer.Add("removeSource", transferNode.SelectSingleNode("removeSource").InnerText);
                        XmlNode history = transferNode.SelectSingleNode("history");
                        if (history != null)
                        {
                            transfer.Add("removeHistory", history.SelectSingleNode("removeHistory").InnerText);
                            transfer.Add("storeDays", history.SelectSingleNode("storeDays").InnerText);
                        }
                        transferList.Add(transfer);
                    }
                }
                settingTable.Add("transfers", transferList);

            }
            catch (Exception e)
            {
                WriteLog("读取配置文件出错："+e.Message);
            }
            return settingTable;
        }
        /// <summary>
        /// 构造xml对象
        /// </summary>
        /// <returns></returns>
        private static XmlDocument GetXmlDoc(String path)
        {
            String location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var curDir = Path.GetDirectoryName(location);
            String ResourcesPath = curDir + "\\" + path;
            XmlDocument xml = new XmlDocument();
            xml.Load(ResourcesPath);
            return xml;
        }

        public static void WriteLog(String Info)
        {
            try
            {
                Console.WriteLine(Info);
                String location = System.Reflection.Assembly.GetEntryAssembly().Location;
                var curDir = Path.GetDirectoryName(location);
                String dir = curDir + "\\log\\" + DateTime.Now.ToString("yyyy") + "\\" + DateTime.Now.ToString("MM") + "\\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                String path = dir + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                Info = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + Info + "\r\n";
                StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.GetEncoding("gb2312"));
                sw.Write(Info);
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {


            }
        }

        #region 执行命令行命令
        /// <summary>
        /// 执行命令行命令
        /// </summary> 
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static String ProcCmd(String cmd)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.WriteLine("exit");
            string strRst = p.StandardOutput.ReadToEnd();
            return strRst;
        }
        #endregion

        public static String formatStr(ref String str)
        {
            str = str.Replace("{date}", DateTime.Now.ToString("yyyyMMdd"));
            str = str.Replace("{predate}", DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
            return str;
        }

        public static String formatHistoryStr(ref String str,DateTime historyDate)
        {
            if (str.Contains("{date}"))
            {
                
            }
            str = str.Replace("{date}", historyDate.ToString("yyyyMMdd"));
            str = str.Replace("{predate}", historyDate.AddDays(-1).ToString("yyyyMMdd"));
            return str;
        }
    }
}

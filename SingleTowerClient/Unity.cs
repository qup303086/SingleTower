using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleTowerClient
{
    public class Unity
    {
        /// <summary>
        /// WriteLog
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Message"></param>
        public static void Log(string Title, string Message)
        {
            string DIRNAME = AppDomain.CurrentDomain.BaseDirectory + @"\Log\";
            string FILENAME = DIRNAME + Title + DateTime.Now.ToString("yyyyMMdd") + ".txt";

            if (!Directory.Exists(DIRNAME))
                Directory.CreateDirectory(DIRNAME);

            if (!File.Exists(FILENAME))
            {
                File.Create(FILENAME).Close();
            }
            using (StreamWriter sw = File.AppendText(FILENAME))
            {
                WriteLog(Message, sw);
            }
        }
 
        private static void WriteLog(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }

        /// <summary>
        /// 定時清除
        /// </summary>
        /// <param name="fileDirect">路徑</param>
        /// <param name="saveDay">刪除天數</param>
        public void DeleteFile(string filePath, int saveDay)
        {
            DateTime nowTime = DateTime.Now;
            DirectoryInfo root = new DirectoryInfo(filePath);
            FileInfo[] dics = root.GetFiles();//獲取文件
            FileAttributes attr = File.GetAttributes(filePath);
            if (attr == FileAttributes.Directory)//判断是不是文件夹
            {
                foreach (FileInfo file in dics)//遍历文件夹
                {
                    TimeSpan t = nowTime - file.CreationTime;  //当前时间  减去 文件创建时间
                    int day = t.Days;
                    if (day > saveDay)   //保存的时间 ；  单位：天
                    {
                        file.Delete();
                    }
                }
            }
        }

    }
}

// 21.11.2016   --Model--   CollUser.cs
using System.IO;
using System;
using System.Windows;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
namespace MeineFinanzen.Model {
    public class CollUser : ObservableCollection<User> { }
    public class User  {        
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public static User CreateUser() {
            return new User { UserName = "luke", UserPassword = "1234" };
        }
        static XmlSerializer xmlserializer = new XmlSerializer(typeof(List<Model.User>));
        public void DeSerializeReadLogin(string filename, out List<Model.User> log) {
            log = null;
            try {
                using (StreamReader _reader = new StreamReader(filename)) {
                    log = (List<Model.User>)xmlserializer.Deserialize(_reader);
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Fehler: DeSerializeReadLogin -Read- " + ex);
            }
            AktualisiereLoginDaten(log);
            //Console.WriteLine("===>DeSerializeReadLogin Read = " + log.Count);
        }
        public void SerializeWriteLogin(string filename, List<Model.User> einD) {
            // Write             
            try {
                using (Stream writer = new FileStream(filename, FileMode.Create)) {
                    xmlserializer.Serialize(writer, einD);
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Fehler: SerializeWriteLogin -Write-  " + ex);
            }
            //Console.WriteLine("===>SerializeWriteLogin Write  = " + einD.Count);
        }
        private void AktualisiereLoginDaten(List<Model.User> einD) {
            FileInfo fiExe = (new FileInfo(Assembly.GetEntryAssembly().Location));
            DateTime dtLeUmw = File.GetLastWriteTime(fiExe.FullName);
        }
        public void InitCollUser(string filename) {
            if (!File.Exists(filename)) {
               /*
                user.Add(new Model.User() { UserName = "luke", UserPassword = "1234" });
                liUser.Add(new Model.User() { UserName = "LuKe", UserPassword = "1234" });
                liUser.Add(new Model.User() { UserName = "emil", UserPassword = "1234" });
                liUser.Add(new Model.User() { UserName = "Emil", UserPassword = "1234" });
                liUser.Add(new Model.User() { UserName = "john", UserPassword = "1234" });
                liUser.Add(new Model.User() { UserName = "John", UserPassword = "1234" });
                liUser.Insert(2, new Model.User() { UserName = "Joseph", UserPassword = "1834" });
                liUser.RemoveAt(3);
                liUser.Remove(new Model.User() { UserName = "emil", UserPassword = "1234" });

                DirectoryInfo di = Directory.CreateDirectory(filename);
                FileStream f = System.IO.File.Create(filename + @"\LoginDaten.xml");
                f.Close();
                Helpers.GlobalRef.g_User.SerializeWriteLogin(
                filename + @"\\LoginDaten.xml", Helpers.GlobalRef.g_User);  */
            }
        }
    }
}
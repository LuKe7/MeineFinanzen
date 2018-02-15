// 25.11.2016   -View-  LoginView.cs 
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
namespace MeineFinanzen.View {
    public partial class LoginView : Window {
        string strxxx = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
               + @"\" + Assembly.GetExecutingAssembly().GetName().Name + @"\MyDepot\Login";
        public LoginView() {
            Helpers.GlobalRef.g_User.InitCollUser(strxxx);
            InitializeComponent();
            this.DataContext = this.Resources["loginViewModel"];    //Set the DataContext for this Window            
        }
        public void LoginView_Loaded(object sender, RoutedEventArgs e) {
            // .ShowDialog()                             
        }
        public void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) {
            PasswordBox pBox = sender as PasswordBox;   //Cast the 'sender' to a PasswordBox
            //Set this "EncryptedPassword" dependency property to the "SecurePassword" of the PasswordBox.
            ViewModel.PasswordBoxMVVMAttachedProperties.SetEncryptedPassword(pBox, pBox.SecurePassword);
        }
        // Since we’re not worried about the Cancel button, I’ll use it to save a record into the database.
        // So go ahead and double-click the Cancel button and add the following code.
        // REMEMBER THAT THIS IS NOT PART OF THE MVVM PATTERN AND REALLY JUST A SECTION
        // THAT WE’RE SMUGGLING IN TO SAVE DATA INTO THE DATABASE.:
        private void Registrieren_Click(object sender, RoutedEventArgs e) {  //Get the ViewModel so that we can retrieve the username and password that have been entered.
            try {
                ViewModel.LoginViewModel viewModel = (ViewModel.LoginViewModel)this.DataContext;
                string username = viewModel.Username;
                byte[] password = ViewModel.PasswordHashing.CalculateHash(
                            ViewModel.SecureStringManipulation.ConvertSecureStringToByteArray(
                                viewModel.PasswordSecureString));
                //Helpers.GlobalRef.g_User.users[0].UserName = null;
                //Helpers.GlobalRef.g_User.users[0].UserPassword = null; 
                List<Model.User> liUser = new List<Model.User>();
                // NOCH Helpers.GlobalRef.g_User.Add(new Model.User() { UserName = username, UserPassword = password.ToString() });
                // NOCH FEHLER Helpers.GlobalRef.g_User.SerializeWriteLogin(strxxx + @"\\LoginDaten.xml", Helpers.GlobalRef.g_User);
                MessageBox.Show("NICHT Login details saved!");
                DialogResult = true;
                return;
            }
            catch (Exception) {
                //DialogResult = false;
                return;
            }
        }
        private void Abbrechen_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            return;
        }
    }
}
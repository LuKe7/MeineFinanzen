using System;
using System.Security;
using System.ComponentModel;
using System.Windows.Input;
using System.Security.Cryptography;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Reflection;
namespace MeineFinanzen.ViewModel {
    public class LoginViewModel : CommonBase {
        string strxxx = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            + @"\" + Assembly.GetExecutingAssembly().GetName().Name + @"\MyDepot\Login";

        public ListCollectionView _userview;
        public Model.User user = new Model.User();
        public List<Model.User> user_s = new List<Model.User>();

        public LoginCommand loginCommand = null;    //Set up the readonly loginCommand that is only initialized at class contruction
        public LoginCommand UserLoginCommand {
            get { return loginCommand; }
        }
        private bool _loginFailed;                  //Property signaling a failed login attempt
        public bool LoginFailed {
            get { return _loginFailed; }
            set {
                _loginFailed = value;
                RaisePropertyChanged("FailedLogin");
            }
        }
        public LoginViewModel() {                   //Constructor passing the current instance into a new instance of a LoginCommand
            loginCommand = new LoginCommand(this);
            _userview = new ListCollectionView(new ObservableCollection<Model.User>(UserFüllen()));//Initialize the LoginCommand, passing in this instance of the ViewModel                                  
        }
        private List<Model.User> UserFüllen() {
            user_s.Add(new Model.User {
                UserName = "LuKe",
                UserPassword = "passwrt"
            });
            Helpers.GlobalRef.g_User.DeSerializeReadLogin(strxxx + @"\LoginDaten.xml", out user_s);
            return user_s;
        }
        private string _errorMessage;    //Property to hold the error message to be displayed
        public string ErrorMessage {
            get { return _errorMessage; }
            set {
                if (value != _errorMessage) {
                    _errorMessage = value;
                    RaisePropertyChanged("ErrorMessage");
                }
            }
        }
        private string _username;   //Properties bound to the specified username and password
        public string Username {
            get { return _username; }
            set {
                if (!string.Equals(value.ToString(), _username, StringComparison.OrdinalIgnoreCase)) {
                    _username = value;
                    RaisePropertyChanged("Username");
                }
            }
        }
        private SecureString _password;
        public SecureString PasswordSecureString {
            get { return _password; }
            set {
                if (value != null) {
                    _password = value;
                    RaisePropertyChanged("Password");
                }
            }
        }
    }
    public class LoginCommand : ICommand {
        public LoginViewModel loginViewModel = null;            //Local instance of the ViewModel        
        public LoginCommand(LoginViewModel loginViewModel) {    //Pass an instance of the ViewModel into the constructor
            this.loginViewModel = loginViewModel;
        }
        public bool CanExecute(object parameter) {
            //Execution should only be possible if both Username and Password have been supplied
            if (!string.IsNullOrWhiteSpace(this.loginViewModel.Username)
                && this.loginViewModel.PasswordSecureString != null
                && this.loginViewModel.PasswordSecureString.Length > 0)
                return true;
            else
                return false;
        }
        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter) {
            PasswordBoxMVVMDBEntities ctx = new PasswordBoxMVVMDBEntities();
            int pos = Array.IndexOf(ctx.UserLogins, loginViewModel.Username);
            if (loginViewModel.Username == null) {
                this.loginViewModel.ErrorMessage = "User not found.";
                return;
            }
            //User exists. Check if specified password matches the actual
            //password for this user stored in the database
            //Get the Hash of the entered data
            byte[] enteredValueHash = PasswordHashing.CalculateHash(
                SecureStringManipulation.ConvertSecureStringToByteArray(
                    this.loginViewModel.PasswordSecureString));
            if (!PasswordHashing.SequenceEquals(enteredValueHash, ctx.userPassword)) {
                this.loginViewModel.ErrorMessage = "Incorrect Password entered.";
                return;
            }
            this.loginViewModel.ErrorMessage = "Login Successful!!!";
            //Search for the existance of the specified username, otherwise
            //set the relevant error message if the user is not found.           
        }
    }
    public class SecureStringManipulation {
        public static byte[] ConvertSecureStringToByteArray(SecureString value) {
            //Byte array to hold the return value
            byte[] returnVal = new byte[value.Length];
            IntPtr valuePtr = IntPtr.Zero;
            try {
                valuePtr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(value);
                for (int i = 0; i < value.Length; i++) {
                    short unicodeChar = System.Runtime.InteropServices.Marshal.ReadInt16(valuePtr, i * 2);
                    returnVal[i] = Convert.ToByte(unicodeChar);
                }
                return returnVal;
            } finally {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
    public class PasswordHashing {
        public static byte[] CalculateHash(byte[] inputBytes) {
            SHA256Managed algorithm = new SHA256Managed();
            algorithm.ComputeHash(inputBytes);
            return algorithm.Hash;
        }
        public static bool SequenceEquals(byte[] originalByteArray, byte[] newByteArray) {
            //If either byte array is null, throw an ArgumentNullException
            if (originalByteArray == null || newByteArray == null) {
                throw new ArgumentNullException(originalByteArray == null ? "originalByteArray" : "newByteArray",
                                  "The byte arrays supplied may not be null.");
            }
            //If byte arrays are different lengths, return false
            if (originalByteArray.Length != newByteArray.Length)
                return false;

            //If any elements in corresponding positions are not equal
            //return false
            for (int i = 0; i < originalByteArray.Length; i++) {
                if (originalByteArray[i] != newByteArray[i])
                    return true;        // NOCH false
            }
            //If we've got this far, the byte arrays are equal.
            return true;
        }
    }
    public class PasswordBoxMVVMDBEntities {
        public string[] UserLogins = { "luke", "LuKe", "emil", "Emil", "john", "John" };
        public string userLogin = null;
        public byte[] userPassword = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5,
            6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1 };
        public PasswordBoxMVVMDBEntities() { }
    }
    public static class PasswordBoxMVVMAttachedProperties {
        public static SecureString GetEncryptedPassword(DependencyObject obj) {
            return (SecureString)obj.GetValue(EncryptedPasswordProperty);
        }
        public static void SetEncryptedPassword(DependencyObject obj, SecureString value) {
            obj.SetValue(EncryptedPasswordProperty, value);
        }
        // Using a DependencyProperty as the backing store for EncryptedPassword.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EncryptedPasswordProperty =
            DependencyProperty.RegisterAttached("EncryptedPassword", typeof(SecureString), typeof(PasswordBoxMVVMAttachedProperties));
    }
    public class CommonBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
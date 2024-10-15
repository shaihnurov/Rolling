using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;

namespace Rolling.ViewModels
{
    public class RegisterViewModel : ObservableObject
    {
        private const string FromPassword = "uvos umpw nqqh lxpk";
        private const string FromAddress = "shaihnurov3@gmail.com";
        private string _verifyCode;
        
        private string _name;
        private string _age;
        private string _email;
        private string _password;
        private string _code;
        private Button _regBtn;
        private bool _isVisibleUserData = true;
        private bool _isVisibleInputCode;
        
        private readonly MainWindowViewModel _mainWindowViewModel;
        public AsyncRelayCommand RegisterUserCommand { get; set; }
        public AsyncRelayCommand ConfirmCodeRegCommand { get; set; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string Email
        {
            get => _email;
            set
            {
                if (IsValidEmail(value))
                {
                    SetProperty(ref _email, value);
                }
                else
                {
                    _mainWindowViewModel.TitleTextInfoBar = "Register";
                    _mainWindowViewModel.MessageInfoBar = "Please state your correct email";
                    _mainWindowViewModel.IsInfoBarVisible = true;
                    _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                    _mainWindowViewModel.StatusInfoBar = 3;
                    
                    Task.Run(async() =>
                    {
                        await Task.Delay(3000);
                        _mainWindowViewModel.IsInfoBarVisible = false;
                    });
                }
            }
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public string Age
        {
            get => _age;
            set
            {
                if (IsNumeric(value))
                {
                    SetProperty(ref _age, value);
                }
                else
                {
                    _mainWindowViewModel.TitleTextInfoBar = "Register";
                    _mainWindowViewModel.MessageInfoBar = "Please state your correct age";
                    _mainWindowViewModel.IsInfoBarVisible = true;
                    _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                    _mainWindowViewModel.StatusInfoBar = 3;
                    
                    Task.Run(async() =>
                    {
                        await Task.Delay(3000);
                        _mainWindowViewModel.IsInfoBarVisible = false;
                    });
                }
            }
        }
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }
        public bool IsVisibleUserData
        {
            get => _isVisibleUserData;
            set => SetProperty(ref _isVisibleUserData, value);
        }
        public bool IsVisibleInputCode
        {
            get => _isVisibleInputCode;
            set => SetProperty(ref _isVisibleInputCode, value);
        }
        public Button RegBtn
        {
            get => _regBtn;
            set => SetProperty(ref _regBtn, value);
        }
        
        public RegisterViewModel(MainWindowViewModel mainWindowViewModel)
        {
            RegisterUserCommand = new AsyncRelayCommand(async() => {
                if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Age) && !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password))
                {
                    await BtnSendCodeUser();
                }
                else
                {
                    _mainWindowViewModel!.TitleTextInfoBar = "Register";
                    _mainWindowViewModel!.MessageInfoBar = "Please fill in all available fields";
                    _mainWindowViewModel.IsInfoBarVisible = true;
                    _mainWindowViewModel.StatusInfoBar = 2;

                    await Task.Delay(3000);
                    _mainWindowViewModel.IsInfoBarVisible = false;
                }
            });
            ConfirmCodeRegCommand = new AsyncRelayCommand(RegisterUser);

            _mainWindowViewModel = mainWindowViewModel;
        }

        private async Task BtnSendCodeUser()
        {
            using (ApplicationContextDb db = new())
            {
                var uniqueEmail = await db.UserModels.Where(s => s.Email == Email).ToListAsync();

                if (uniqueEmail.Count == 0)
                {
                    IsVisibleInputCode = true;
                    IsVisibleUserData = false;
                    
                    _verifyCode = GenerateCode();
                    await SendVerificationCode(Email, _verifyCode);
                }
                else
                {
                    _mainWindowViewModel.TitleTextInfoBar = "Register";
                    _mainWindowViewModel.MessageInfoBar = "This mail is occupied by another user";
                    _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                    _mainWindowViewModel.IsInfoBarVisible = true;
                    _mainWindowViewModel.StatusInfoBar = 3;
                
                    await Task.Delay(3000);
                    _mainWindowViewModel.IsInfoBarVisible = false;
                }
            }
        }
        private async Task RegisterUser()
        {
            using (ApplicationContextDb db = new())
            {
                if (_verifyCode == Code)
                {
                    var userData = new UserData
                    {
                        Email = Email
                    };

                    await UserDataStorage.SaveUserData(userData);
                    _mainWindowViewModel.UserService.UpdateUserData();
                    
                    var userModel = new UserModel
                    {
                        Name = Name,
                        Age = int.Parse(Age),
                        Email = Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(Password),
                        Level = 1,
                        Permission = "User"
                    };
                
                    await db.UserModels.AddAsync(userModel);
                    await db.SaveChangesAsync();

                    _mainWindowViewModel.IsVisibleBtnUserAcc = true;
                    _mainWindowViewModel.IsVisibleBtnAuthOrReg = false;
                    _mainWindowViewModel.TitleTextInfoBar = "Register";
                    _mainWindowViewModel.MessageInfoBar = "Registration successfully completed";
                    _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                    _mainWindowViewModel.IsInfoBarVisible = true;
                    _mainWindowViewModel.StatusInfoBar = 1;
                    
                    _mainWindowViewModel.CurrentView = new HomeViewModel();
                
                    await Task.Delay(3000);
                    _mainWindowViewModel.IsInfoBarVisible = false;   
                }
                else
                {
                    _mainWindowViewModel.TitleTextInfoBar = "Register";
                    _mainWindowViewModel.MessageInfoBar = "You have entered an invalid code";
                    _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                    _mainWindowViewModel.IsInfoBarVisible = true;
                    _mainWindowViewModel.StatusInfoBar = 3;
                
                    await Task.Delay(3000);
                    _mainWindowViewModel.IsInfoBarVisible = false;
                }
            }
        }
        private string GenerateCode()
        {
            byte[] data = new byte[6];
            RandomNumberGenerator.Fill(data);
            return BitConverter.ToString(data).Replace("-", "").Substring(0, 6);
        }
        private async Task SendVerificationCode(string email, string code)
        {
            var fromAddress = new MailAddress(FromAddress, "Rolling");
            var toAddress = new MailAddress(email);
            
            const string subject = "Registering an account with Rolling";
            string body = $"{Name}, your confirmation code - {code}";

            var smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, FromPassword)
            };

            try
            {
                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                };
                smtp.Send(message);
            }
            catch (SmtpException)
            {
                _mainWindowViewModel.TitleTextInfoBar = "Register";
                _mainWindowViewModel!.MessageInfoBar = $"Error sending confirmation code to mail {email}";
                _mainWindowViewModel.IsInfoBarVisible = true;
                _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                _mainWindowViewModel.StatusInfoBar = 3;

                await Task.Delay(3000);
                _mainWindowViewModel.IsInfoBarVisible = false;
            }
            catch (Exception)
            {
                _mainWindowViewModel.TitleTextInfoBar = "Register";
                _mainWindowViewModel!.MessageInfoBar = $"There was an internal error";
                _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                _mainWindowViewModel.IsInfoBarVisible = true;
                _mainWindowViewModel.StatusInfoBar = 3;

                await Task.Delay(3000);
                _mainWindowViewModel.IsInfoBarVisible = false;
            }
        }
        private bool IsNumeric(string age)
        {
            return !string.IsNullOrEmpty(age) && age.All(char.IsDigit);
        }
        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        }
    }
}
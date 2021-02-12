using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendTech.BLL.Interfaces
{
    public interface IAuthenticateManager
    {
        bool IsEmailExist(string email);
        UserModel GetDetailsbyUser(string email, string password);
        ActionOutput AddTokenDevice(LoginAPIModel model);
        bool IsTokenAlreadyExists(long userId);
        bool DeleteGenerateToken(long userId);
        string GenerateToken(UserModel user, DateTime IssuedOn);
        int InsertToken(TokenModel token);
        ActionOutput<long> SignUp(SignUpModel model);
        ActionOutput ForgotPassword(string email, string otp);
        bool IsValidForgotRequest(long userId, string token);
        ActionOutput Logout(long userId,string token);
        ActionOutput ResetPassword(ResetPasswordModel model);
        ActionOutput SaveAccountVerificationRequest(long userId, string token);
        ActionOutput VerifyAccountVerificationCode(VerifyAccountVerificationCodeMOdel model);
        bool IsUserNameExists(string userName);
        List<CountryModel> GetCountries();
        List<CityModel> GetCities(int countryId=0);
        ActionOutput<string> SaveChangePasswordOTP(long userId, string oldPassword, string otp);
        ActionOutput VerifyChangePasswordOTP(ResetPasswordModel model);
        int GetLogoutTime();
        ActionOutput SaveLogoutTime(SaveLogoutTimeModel model);
        ActionOutput ChangePassword(ChangePasswordModel model);
        bool IsUserAccountActive(string email, string password);
        bool ConfirmThisUser(ChangePasswordModel model);
    }
    
}

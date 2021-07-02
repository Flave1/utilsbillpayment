using VendTech.BLL.Interfaces;
using VendTech.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using VendTech.DAL;
using VendTech.BLL.Common;
using System.Data.Entity.Validation;

namespace VendTech.BLL.Managers
{
    public class AuthenticateManager : BaseManager, IAuthenticateManager
    {
        bool IAuthenticateManager.IsEmailExist(string email)
        {
            return Context.Users.Any(p => p.Email.ToLower() == email.ToLower() && p.Status != (int)UserStatusEnum.Deleted);
        }


        bool IAuthenticateManager.IsValidForgotRequest(long userId, string token)
        {
            var record = Context.ForgotPasswordRequests.SingleOrDefault(p => p.UserId == userId && p.OTP == token);
            if (record != null)
            {
                Context.ForgotPasswordRequests.Remove(record);
                Context.SaveChanges();
                return true;
            }
            return false;
        }
        ActionOutput IAuthenticateManager.SaveAccountVerificationRequest(long userId, string token)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            if (user == null)
                return ReturnError("USer not exist.");
            var alreadyGeneratedCodes = user.AccountVerificationOTPs.ToList();
            var dbAccountVerificationRequest = new AccountVerificationOTP();
            dbAccountVerificationRequest.CreatedAt = DateTime.UtcNow;
            dbAccountVerificationRequest.OTP = token;
            dbAccountVerificationRequest.UserId = userId;
            Context.AccountVerificationOTPs.Add(dbAccountVerificationRequest);
            if (alreadyGeneratedCodes.Count() > 0) Context.AccountVerificationOTPs.RemoveRange(alreadyGeneratedCodes);
            Context.SaveChanges();
            return ReturnSuccess("Code saved successully");
        }
        ActionOutput<string> IAuthenticateManager.SaveChangePasswordOTP(long userId, string oldPassword, string otp)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);

            if (user == null)
                return ReturnError<string>("User not exist.");
            var encryptedPassword = Utilities.EncryptPassword(oldPassword);
            if (user.Password != encryptedPassword)
                return ReturnError<string>("Old password did not match.");

            var existingRequest = Context.ForgotPasswordRequests.Where(p => p.UserId == userId);
            var request = new ForgotPasswordRequest();
            request.CreatedAt = DateTime.UtcNow;
            request.UserId = user.UserId;
            request.OTP = otp;
            Context.ForgotPasswordRequests.Add(request);
            Context.ForgotPasswordRequests.RemoveRange(existingRequest);
            Context.SaveChanges();
            return ReturnSuccess(user.Email, "OTP sent to your email");
        }

        ActionOutput<string> IAuthenticateManager.FirstTimeLoginChangePassword(long userId, string oldPassword, string newPassword)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);

            if (user == null)
                return ReturnError<string>("User not exist.");
            var encryptedPassword = Utilities.EncryptPassword(oldPassword);
            if (user.Password != encryptedPassword)
                return ReturnError<string>("Old password did not match.");

            user.IsEmailVerified = true;
            user.Password = Utilities.EncryptPassword(newPassword);
            Context.SaveChanges();
            return ReturnSuccess(user.Email, "Email succesfuly verified");
        }

        ActionOutput IAuthenticateManager.VerifyChangePasswordOTP(ResetPasswordModel model)
        {
            var user = Context.Users.Single(p => p.UserId == model.UserId);
            var changePasswordOtp = user.ForgotPasswordRequests.Any(p => p.UserId == model.UserId && model.Otp == p.OTP);
            if (!changePasswordOtp)
                return ReturnError("OTP not matched");
            user.Password = Utilities.EncryptPassword(model.Password);
            Context.ForgotPasswordRequests.RemoveRange(user.ForgotPasswordRequests.ToList());
            if (user.Status == (int)UserStatusEnum.PasswordNotReset)
                user.Status = (int)UserStatusEnum.Active;
            Context.SaveChanges();
            return ReturnSuccess("Password has been reset successfully.");
        }

        ActionOutput IAuthenticateManager.ResetPassword(ResetPasswordModel model)
        {
            var user = Context.Users.Single(p => p.UserId == model.UserId);
            if (!string.IsNullOrEmpty(model.OldPassword))
            {
                var encryptedPassword = Utilities.EncryptPassword(model.OldPassword);
                if (user.Password != encryptedPassword)
                    return ReturnError("Old password did not match.");
            }
            user.Password = Utilities.EncryptPassword(model.Password);
            Context.SaveChanges();
            return ReturnSuccess("Password has been reset successfully.");
        }

        ActionOutput IAuthenticateManager.ChangePassword(ChangePasswordModel model)
        {
            var user = Context.Users.Single(p => p.UserId == model.UserId);
            if (!string.IsNullOrEmpty(model.OldPassword))
            {
                var encryptedPassword = Utilities.EncryptPassword(model.OldPassword);
                if (user.Password != encryptedPassword)
                    return ReturnError("Old password did not match.");
            }
            user.Password = Utilities.EncryptPassword(model.Password);
            if (user.Status == (int)UserStatusEnum.PasswordNotReset)
                user.Status = (int)UserStatusEnum.Active;
            Context.SaveChanges();
            return ReturnSuccess("Password has been reset successfully.");
        }
        bool IAuthenticateManager.IsUserAccountActive(string email, string password)
        {
            string encryptPassword = Utilities.EncryptPassword(password.Trim());
            var result = Context.Users.Where(x => (x.Email == email || x.UserName.ToLower() == email.ToLower()) && x.Password == encryptPassword && (UserRoles.AppUser == x.UserRole.Role || UserRoles.Vendor == x.UserRole.Role) && (x.Status == (int)UserStatusEnum.Block))
                .ToList()
                .Select(x => new UserModel(x))
                .FirstOrDefault();
            if (result == null)
                return true;
            return false;
        }
        bool IAuthenticateManager.IsUserPosEnabled(string email, string password)
        {
            bool userAssignedPos = false;
            string encryptPassword = Utilities.EncryptPassword(password.Trim());
            var result = Context.Users.Where(x => (x.Email == email || x.UserName.ToLower() == email.ToLower()) &&
            x.Password == encryptPassword && (UserRoles.AppUser == x.UserRole.Role || UserRoles.Vendor == x.UserRole.Role)
            && (x.Status == (int)UserStatusEnum.Active ||
            x.Status == (int)UserStatusEnum.PasswordNotReset ||
            x.Status == (int)UserStatusEnum.Pending))
                .FirstOrDefault();
            if (result != null && result.POS.Any())
            {

                if (result.UserRole.Role == UserRoles.Vendor)
                    userAssignedPos = result.POS.All(p => p.Enabled == false);
                else if (result.UserRole.Role == UserRoles.AppUser && result.User1 != null)
                    userAssignedPos = result.User1.POS.All(p => p.Enabled == false);
                return userAssignedPos;
            }
            return userAssignedPos;

        }

        bool IAuthenticateManager.IsUserPosEnable(long userId)
        {
            try
            {
                var result = Context.Users.Where(x => x.UserId == userId
            && (x.Status == (int)UserStatusEnum.Active ||
            x.Status == (int)UserStatusEnum.PasswordNotReset ||
            x.Status == (int)UserStatusEnum.Pending))
                .FirstOrDefault();
                if (result != null)
                {
                    bool userAssignedPos = false;
                    if (result.UserRole.Role == UserRoles.Vendor)
                        userAssignedPos = result.POS.All(p => p.Enabled == false);
                    else if (result.UserRole.Role == UserRoles.AppUser && result.User1 != null)
                        userAssignedPos = result.User1.POS.All(p => p.Enabled == false);
                    if (userAssignedPos)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        UserModel IAuthenticateManager.GetDetailsbyUser(string email, string password)
        {
            try
            {
                string encryptPassword = Utilities.EncryptPassword(password.Trim());
                var decryptedPass = Utilities.DecryptPassword("dGVzdHZpY3RvcjE=");
                var result = Context.Users
                    .Where(x => (x.Email == email || x.UserName.ToLower() == email.ToLower())
                && x.Password == encryptPassword
                && (UserRoles.AppUser == x.UserRole.Role
                || UserRoles.Vendor == x.UserRole.Role)
                && (x.Status == (int)UserStatusEnum.Active
                || x.Status == (int)UserStatusEnum.Pending
                || x.Status == (int)UserStatusEnum.PasswordNotReset))
                    .ToList()
                    .Select(x => new UserModel(x))
                    .FirstOrDefault();
                if (result != null)
                {
                    Context.Users.FirstOrDefault(d => d.UserId == result.UserId).AppLastUsed = DateTime.UtcNow;
                    Context.SaveChanges();
                }
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        UserModel IAuthenticateManager.GetUserDetailByPassCode(string passCode)
        {
            try
            {
                var userModel = new UserModel();
                var pos = Context.POS.Where(x => x.PassCode == passCode).FirstOrDefault();
                if (pos != null)
                {
                    userModel = Context.Users.Where(x => x.FKVendorId == pos.VendorId && (UserRoles.AppUser == x.UserRole.Role
               || UserRoles.Vendor == x.UserRole.Role)
               && (x.Status == (int)UserStatusEnum.Active
               || x.Status == (int)UserStatusEnum.Pending
               || x.Status == (int)UserStatusEnum.PasswordNotReset))
                   .ToList()
                   .Select(x => new UserModel(x))
                   .FirstOrDefault();
                    if (userModel != null)
                    {
                        return userModel;
                    }
                    userModel = Context.Users.Where(x => x.UserId == pos.VendorId && (UserRoles.AppUser == x.UserRole.Role
               || UserRoles.Vendor == x.UserRole.Role)
               && (x.Status == (int)UserStatusEnum.Active
               || x.Status == (int)UserStatusEnum.Pending
               || x.Status == (int)UserStatusEnum.PasswordNotReset))
                   .ToList()
                   .Select(x => new UserModel(x))
                   .FirstOrDefault();
                    return userModel;
                }
                return userModel;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        ActionOutput IAuthenticateManager.ForgotPassword(string email, string otp)
        {
            var user = Context.Users.OrderByDescending(p => p.CreatedAt).FirstOrDefault(p => p.Email.ToLower().Trim() == email.ToLower().Trim());
            if (user == null)
                return ReturnError("User with this email not exist.");

            // Pending person also can reset password:
            //if (user.Status != (int)UserStatusEnum.Active)
            //    return ReturnError("User is not active.");

            var request = new ForgotPasswordRequest();
            request.CreatedAt = DateTime.UtcNow;
            request.UserId = user.UserId;
            request.OTP = otp;
            Context.ForgotPasswordRequests.Add(request);
            try
            {
                Context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                return ReturnError(ex?.Message);
            }

            return ReturnSuccess(user.UserId, "Token generate successfully.");
        }

        //ActionOutput IAuthenticateManager.AddTokenDevice(LoginAPIModel model)
        //{
        //    string encryptPassword = Utilities.EncryptPassword(model.Password.Trim());
        //    //var data = Context.Users.Where(x => x.Email == model.Email && x.Password == encryptPassword).FirstOrDefault();
        //    var userModel = new User();

        //    var data = Context.Users.Where(x => x.Email == model.Email && x.Password == encryptPassword).FirstOrDefault();

        //    userModel.DeviceToken = model.DeviceToken == null ? "" : model.DeviceToken.Trim();
        //    userModel.AppType = string.IsNullOrEmpty(model.AppType) ? (int)AppTypeEnum.IOS : (int)AppTypeEnum.Android;
        //    Context.SaveChanges();
        //    return ReturnSuccess();
        //}

        ActionOutput IAuthenticateManager.AddTokenDevice(LoginAPIPassCodeModel model)
        {
            //string encryptPassword = Utilities.EncryptPassword(model.Password.Trim());
            //var data = Context.Users.Where(x => x.Email == model.Email && x.Password == encryptPassword).FirstOrDefault();

            var pos = Context.POS.FirstOrDefault(x => x.PassCode == model.PassCode);
            var userModel = Context.Users.FirstOrDefault(x => x.UserId == pos.VendorId);
            //var data = Context.Users.Where(x => x.Email ==  && x.Password == encryptPassword).FirstOrDefault();

            userModel.DeviceToken = model.DeviceToken == null ? "" : model.DeviceToken.Trim();
            userModel.AppType = string.IsNullOrEmpty(model.AppType) ? (int)AppTypeEnum.IOS : (int)AppTypeEnum.Android;
            Context.SaveChanges();
            return ReturnSuccess();
        }

        ActionOutput IAuthenticateManager.Logout(long userId, string token)
        {
            var user = Context.Users.FirstOrDefault(p => p.UserId == userId);
            Context.TokensManagers.RemoveRange(user.TokensManagers.Where(p => p.TokenKey == token).ToList());
            user.DeviceToken = string.Empty;
            Context.SaveChanges();
            return ReturnSuccess("User successfully logout.");
        }
        bool IAuthenticateManager.IsTokenAlreadyExists(long userId, string posNumber)
        {
            try
            {
                var result = Context.TokensManagers.Where(x => x.UserId == userId && x.PosNumber == posNumber).Count();
                //var result = (from token in _context.TokensManager
                //              where token.CompanyID == CompanyID
                //              select token).Count();
                return result > 0;
            }
            catch (Exception)
            {

                throw;
            }
        }

        bool IAuthenticateManager.ConfirmThisUser(ChangePasswordModel model)
        {
            var user = Context.Users.Single(p => p.UserId == model.UserId);

            user.Password = Utilities.EncryptPassword(model.Password);
            user.IsEmailVerified = true;
            if (user.Status == (int)UserStatusEnum.PasswordNotReset)
                user.Status = (int)UserStatusEnum.Active;
            Context.SaveChanges();
            return true;
        }
        int IAuthenticateManager.GetLogoutTime()
        {
            int second = 3;
            try
            {
                var db = new VendTechEntities();

                var record = db.AppSettings.FirstOrDefault(p => p.Name == AppSettings.LogoutTime);
                if (record != null)
                    second = Convert.ToInt32(record.Value);
                return second;
            }
            catch (Exception)
            {

                return second;
            }

        }
        ActionOutput IAuthenticateManager.SaveLogoutTime(SaveLogoutTimeModel model)
        {
            var record = Context.AppSettings.FirstOrDefault(p => p.Name == AppSettings.LogoutTime);
            if (record == null)
                return ReturnError("Some error occured while saving time.");
            record.Value = model.Time.ToString();
            Context.SaveChanges();
            return ReturnSuccess("Logout time saved successfully.");
        }
        bool IAuthenticateManager.DeleteGenerateToken(long userId, string posNumber)
        {
            try
            {
                var token = Context.TokensManagers.Where(x => x.UserId == userId && x.PosNumber == posNumber).ToList();
                if (token.Count() > 0)
                {
                    Context.TokensManagers.RemoveRange(token);
                    Context.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        string IAuthenticateManager.GenerateToken(UserModel user, DateTime IssuedOn)
        {
            try
            {
                string randomnumber =
                   string.Join(":", new string[]
                   {   Convert.ToString(user.UserId),
                Utilities.GetUniqueKey(),

                Convert.ToString(IssuedOn.Ticks),
                user.Email
                   });

                return Utilities.Encrypt(randomnumber);
            }
            catch (Exception)
            {

                throw;
            }
        }
        int IAuthenticateManager.InsertToken(TokenModel token)
        {
            try
            {
                TokensManager newToken = new TokensManager();
                newToken.CreatedAt = token.CreatedOn;
                newToken.ExpiresOn = token.ExpiresOn;
                newToken.TokenKey = token.TokenKey;
                newToken.UserId = token.UserId;
                newToken.DeviceToken = token.DeviceToken == null ? "" : token.DeviceToken.Trim();
                newToken.PosNumber = token.PosNumber;
                newToken.AppType = string.IsNullOrEmpty(token.AppType) ? (int)AppTypeEnum.IOS : (int)AppTypeEnum.Android;
                Context.TokensManagers.Add(newToken);
                Context.SaveChanges();
                return 1;
            }
            catch (Exception)
            {

                throw;
            }
        }
        ActionOutput<long> IAuthenticateManager.SignUp(SignUpModel model)
        {
            if (Context.Users.Any(p => p.Email.ToLower() == model.Email.ToLower()))
                return ReturnError<long>("User already exist with this email.");
            if (Context.Users.Any(p => p.Phone.ToLower() == model.Phone.ToLower()))
                return ReturnError<long>("User already exist with this phone number.");
            if (!string.IsNullOrEmpty(model.ReferralCode))
            {
                var referralCode = Context.ReferralCodes.FirstOrDefault(p => p.Code == model.ReferralCode && !p.IsUsed);
                if (referralCode == null)
                    return ReturnError<long>("Invalid referral code.");
                Context.ReferralCodes.Remove(referralCode);
            }
            var newUser = new User();
            newUser.Email = model.Email;
            newUser.Password = Utilities.EncryptPassword(model.Password);
            newUser.Name = model.FirstName;
            newUser.SurName = model.LastName;
            newUser.UserName = model.UserName;
            newUser.IsEmailVerified = false;
            newUser.Status = (int)UserStatusEnum.Pending;
            newUser.UserType = Utilities.GetUserRoleIntValue(UserRoles.AppUser);
            newUser.AppUserType = (int)model.AppUserType;
            newUser.CreatedAt = DateTime.UtcNow;
            newUser.CountryId = model.Country;
            newUser.Phone = model.Phone;
            newUser.Address = model.Address;
            newUser.CityId = model.City;
            newUser.CompanyName = model.CompanyName;
            Context.Users.Add(newUser);
            Context.SaveChanges();
            return ReturnSuccess<long>(newUser.UserId, "User added successfully.");
        }
        bool IAuthenticateManager.IsUserNameExists(string userName)
        {
            return Context.Users.Any(p => p.UserName.ToLower() == userName.ToLower());
        }

        ActionOutput IAuthenticateManager.VerifyAccountVerificationCode(VerifyAccountVerificationCodeMOdel model)
        {
            var record = Context.AccountVerificationOTPs.FirstOrDefault(p => p.UserId == model.UserId && p.OTP == model.Code);
            if (record != null)
            {
                record.User.IsEmailVerified = true;
                record.User.Status = (int)UserStatusEnum.Active;

                Context.AccountVerificationOTPs.Remove(record);
                Context.SaveChanges();


                return ReturnSuccess("Email verified successfully.");
            }
            return ReturnError("OTP did not match.");
        }

        List<CountryModel> IAuthenticateManager.GetCountries()
        {
            return Context.Countries.Where(p => !p.IsDeleted).ToList().OrderByDescending(p => p.CountryId).Select(x => new CountryModel
            {
                CountryId = x.CountryId,
                Name = x.Name
            }).ToList();
        }
        List<CityModel> IAuthenticateManager.GetCities(int countryId)
        {
            var query = Context.Cities.Where(p => !p.IsDeleted);
            if (countryId > 0)
                query = query.Where(p => p.CountryId == countryId);
            return query.ToList().Select(x => new CityModel
            {
                CountryId = x.CountryId,
                Name = x.Name,
                CityId = x.CityId,
                CountryName = x.Country.Name
            }).ToList();
        }
    }


}

using Database.User;

using DevExpress.Xpo;

using Microsoft.Extensions.Logging;

namespace MinimalGrpcTemplate.Server.Services.UserManagement;

public class UserManagementService(ILogger<UserManagementService> c_logger, UnitOfWork c_unitOfWork)
{
    public User CreateUser(string p_username)
    {
        var user = new User(c_unitOfWork)
                   {
                       Username = p_username
                   };
        return user;
    }

    public void SaveUser(User p_user)
    {
        //c_unitOfWork.Save(p_user);
        c_unitOfWork.CommitChanges();
    }
}
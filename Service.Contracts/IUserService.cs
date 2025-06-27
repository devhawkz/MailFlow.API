using System.Runtime.CompilerServices;

namespace Service.Contracts;

public interface IUserService
{
    Task AuthorizeUser();
}

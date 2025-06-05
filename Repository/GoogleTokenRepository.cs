

using Contracts;
using Entities.Models;
using System.Linq.Expressions;

namespace Repository;

public class GoogleTokenRepository : RepositoryBase<GoogleToken>, IGoogleTokenRepository
{
    public GoogleTokenRepository(DataContext dataContext) : base(dataContext)
    {
        
    }
}

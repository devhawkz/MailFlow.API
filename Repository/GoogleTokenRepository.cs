

using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repository;

public class GoogleTokenRepository : RepositoryBase<GoogleToken>, IGoogleTokenRepository
{
    private static readonly Guid _userId = Guid.Parse("02d9cd73-990c-437c-827b-fac07e08ba09"); 
    public GoogleTokenRepository(DataContext dataContext) : base(dataContext)
    {
        
    }
    public void UpdateTokenAsync(GoogleToken token) => Update(token);

    public GoogleToken GetLatestTokenForUserAsync(bool trackChanges) =>
        FindByCondition(t => t.UserId == _userId, trackChanges)
            .OrderByDescending(t => t.ExpiresAt)
            .SingleOrDefault()!;

}


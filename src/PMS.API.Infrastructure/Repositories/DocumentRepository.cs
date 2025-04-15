using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Interfaces.Repositories;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Infrastructure.Repositories;

public class DocumentRepository : EfRepository<Document>, IDocumentRepository
{
  private readonly AppDbContext _context;
  public DocumentRepository(AppDbContext context) : base(context)
  {
    _context = context;
  }
}

